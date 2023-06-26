using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Responsible for Overall Game Logic and Update States on GameBoard
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] bool CanOpenAllMinesAtGameEnd;
    [SerializeField] GameInput _gameInput;
    [SerializeField] CameraController _cameraController;
    [SerializeField] TextAsset _levelData;


    #region PRIVATE_VARIABLES


    /// <summary>
    /// Reference Of Level Data Asset (JSON FILE)
    /// </summary>
    LevelData m_currentCachedLevelData;

    GameBoardView m_gameBoardView;
    Block[,] m_blocksCurrentGameState;

    int m_currentCachedWidth = 0;
    int m_currentCachedHeight = 0;
    int m_currentCachedMinesCount = 0;
    int m_currentFlaggedMinesCount = 0;

    bool IsGameStarted = false;
    bool IsGameOver = false;
    bool IsValidBlocksAvailable = false;
    bool isAutoPlayEnabled =>m_gameBoardView.GetAutoPlayToggleStatus();


    float m_currentGameTimer = 0f;

    #endregion

    private void Start()
    {
        m_gameBoardView = GetComponent<GameBoardView>();

        m_gameBoardView.UpdateInstructionText($"Hold {_gameInput._cameraMoveKey} to Move The Camera\n" +
            $"Hold {_gameInput._cameraZoomKey} to Zoom In-Out The Camera By Mouse Scroll");

        ExtractLevelData();
    }



    private void Update()
    {

        if (!IsGameStarted)
        {
            m_currentGameTimer = 0f;
            return;
        }


        if (_gameInput == null)
            return;

        if (m_gameBoardView == null)
            return;





        m_currentGameTimer += Time.deltaTime;

        m_gameBoardView.UpdateGameBoardTimeCounter(Mathf.RoundToInt(m_currentGameTimer));


        if (isAutoPlayEnabled)
            return;


        if (_gameInput.IsBlockOpenKeyPressed)
        {
            RevealBlockOnGameBoard();

            m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);

        }
        else if (_gameInput.IsBlockMarkFlagKeyPressed)
        {
            SetFlagOnGameBoard();

            m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);
        }


    }


    /// <summary>
    /// Read JSON Data and fetching all the required details from it. 
    /// </summary>
    void ExtractLevelData()
    {
        if (_levelData == null || string.IsNullOrEmpty(_levelData.text))
        {
            PopulateRandomLevelData();
            return;
        }

        m_currentCachedLevelData = JsonUtility.FromJson<LevelData>(_levelData.text);

        m_currentCachedMinesCount = m_currentCachedLevelData._totalMinesCount;
        m_currentCachedWidth = m_currentCachedLevelData._levelWidth;
        m_currentCachedHeight = m_currentCachedLevelData._levelHeight;

        GenerateLevel(m_currentCachedLevelData._levelWidth, m_currentCachedLevelData._levelHeight);
    }

    /// <summary>
    /// Random Level Generation
    /// </summary>
    void PopulateRandomLevelData()
    {

        int width = Random.Range(4, 32);
        int height = Random.Range(4, 32);

        m_currentCachedWidth = width;
        m_currentCachedHeight = height;
        m_currentCachedMinesCount = Random.Range(1, width * height);

        GenerateLevel(width, height);
    }


    /// <summary>
    /// Generate Level using either random data or Json data
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void GenerateLevel(int width, int height)
    {

        IsGameStarted = true;
        IsGameOver = false;



        _cameraController.SetCameraDimension(width, height);

        m_blocksCurrentGameState = new Block[width, height];

        //Generating Random Empty Block, Mine Blocks, Number Block
        GenerateBlocksOnGameBoard();
        GenerateRandomMinesOnGameBoard();
        GenerateNumbersOnGameBoard();


        //Updating the Gameboard using Current Status and Timer,MineCounts,Winning Losing Status, Autoplay Toggle status, Restart Button Data
        m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);
        m_gameBoardView.UpdateRemainingMinesCount(m_currentCachedMinesCount);
        m_gameBoardView.UpdateRestartButtonWinLoseStatus(true, true);
        m_gameBoardView.AssignListenerToRestartButton(()=> 
        {

            m_gameBoardView.GameBoardTileMap.ClearAllTiles();
            ExtractLevelData();     
        
        });
        m_gameBoardView.UpdateAutoplayToggleStatus(false);


    }


    /// <summary>
    /// Blocks Generation On GameBoard
    /// </summary>
    void GenerateBlocksOnGameBoard()
    {
        for (int x = 0; x < m_currentCachedWidth; x++)
        {
            for (int y = 0; y < m_currentCachedHeight; y++)
            {
                Block newBlock = new Block(new Vector3Int(x, y));
                m_blocksCurrentGameState[x, y] = newBlock;
            }
        }
    }

    /// <summary>
    /// Random Mine Blocks Generation By making sure not to consider the same block again logic
    /// </summary>
    void GenerateRandomMinesOnGameBoard()
    {
        for (int i = 0; i < m_currentCachedMinesCount; i++)
        {
            int xPos = Random.Range(0, m_currentCachedWidth);
            int yPos = Random.Range(0, m_currentCachedHeight);

            while (m_blocksCurrentGameState[xPos, yPos]._blockType == BLOCK_TYPE.MINE)
            {
                xPos++;

                if (xPos >= m_currentCachedWidth)
                {
                    xPos = 0;
                    yPos++;

                    if (yPos >= m_currentCachedHeight)
                    {
                        yPos = 0;
                    }

                }

            }

            m_blocksCurrentGameState[xPos, yPos]._blockType = BLOCK_TYPE.MINE;

        }
    }

    /// <summary>
    /// Random Number Generation by using Nearby mines available information.
    /// </summary>
    void GenerateNumbersOnGameBoard()
    {
        for (int x = 0; x < m_currentCachedWidth; x++)
        {
            for (int y = 0; y < m_currentCachedHeight; y++)
            {

                Block block = m_blocksCurrentGameState[x, y];

                if (block._blockType == BLOCK_TYPE.MINE)
                    continue;


                block._number = GetNearbyAdjacentMines(x, y);

                //If Any block doesn't have any adjacent mine than that block cosidered as EMPTY Block.

                if (block._number > 0)
                    block._blockType = BLOCK_TYPE.NUMBER;

                m_blocksCurrentGameState[x, y] = block;

            }
        }
    }

    /// <summary>
    /// Adjancy Mines Count. Helpful for Generating Numbers
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <returns></returns>
    int GetNearbyAdjacentMines(int xPos, int yPos)
    {
        int adjacentMinesCount = 0;


        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {

                if (i == 0 && j == 0)
                    continue;


                int adjacentXPos = xPos + i;
                int adjacentYPos = yPos + j;

                if (!MinesweeperUtility.IsValidIndex(adjacentXPos, adjacentYPos, m_currentCachedWidth, m_currentCachedHeight))
                    continue;

                if (m_blocksCurrentGameState[adjacentXPos, adjacentYPos]._blockType == BLOCK_TYPE.MINE)
                    adjacentMinesCount++;

            }

        }

        return adjacentMinesCount;

    }


    /// <summary>
    /// Set a Flag on GameBoard by making sure important conditions
    /// </summary>
    void SetFlagOnGameBoard()
    {
        int xPos, yPos;
        Block block = GetBlockOverMousePosition(out xPos, out yPos);

        if (block == null)
        {
            Debug.Log("Block Not Found!!");
            return;
        }

        if (block.isBlockRevealed)
            return;


        block.isBlockFlagged = !block.isBlockFlagged;
        m_blocksCurrentGameState[xPos, yPos] = block;


        if (block.isBlockFlagged)
            m_currentFlaggedMinesCount++;
        else
            m_currentFlaggedMinesCount--;


        m_gameBoardView.UpdateRemainingMinesCount(m_currentCachedMinesCount - m_currentFlaggedMinesCount);
    }


    /// <summary>
    /// Reveal Clicked block and check what is underneath. If Nothing that It'll open all the empty blocks till numbers appears.
    /// </summary>
    void RevealBlockOnGameBoard()
    {
        int xPos, yPos;
        Block block = GetBlockOverMousePosition(out xPos, out yPos);

        if (block == null)
        {
            Debug.Log("Block Not Found!!");
            return;
        }

        if (block.isBlockRevealed || block.isBlockFlagged)
            return;


        switch (block._blockType)
        {
            case BLOCK_TYPE.MINE:
                {
                    ExplodeMineOnGameBoard(block);
                    break;
                }
            case BLOCK_TYPE.EMPTY:
                {
                    OpenAdjacentEmptyBlocks(block);
                    CheckGameWin();
                    break;
                }
            default:
                {
                    block.isBlockRevealed = true;
                    m_blocksCurrentGameState[xPos, yPos] = block;
                    CheckGameWin();
                    break;
                }
        }

    }


    /// <summary>
    /// Revealed a Mine and Losing Logic
    /// </summary>
    /// <param name="block"></param>
    void ExplodeMineOnGameBoard(Block block)
    {
        IsGameStarted = false;
        IsGameOver = true;

        m_currentFlaggedMinesCount = 0;

        block.isBlockRevealed = true;
        block.isBlockExploded = true;
        m_blocksCurrentGameState[block._blockPositionInGrid.x, block._blockPositionInGrid.y] = block;

        m_gameBoardView.UpdateRestartButtonWinLoseStatus(false);
        m_gameBoardView.UpdateAutoplayToggleStatus(true);

        if (CanOpenAllMinesAtGameEnd)
            RevealAllAvailableMines();
    }

    /// <summary>
    /// Extra function, If User wants to see all the available mine on Game End.
    /// </summary>
    void RevealAllAvailableMines()
    {
        for (int i = 0; i < m_currentCachedWidth; i++)
        {
            for (int j = 0; j < m_currentCachedHeight; j++)
            {
                Block fetchedBlock = m_blocksCurrentGameState[i, j];

                if (fetchedBlock._blockType == BLOCK_TYPE.MINE)
                {
                    fetchedBlock.isBlockRevealed = true;
                    m_blocksCurrentGameState[i, j] = fetchedBlock;
                }

            }
        }
    }


    /// <summary>
    /// Game Winning Logic
    /// </summary>
    /// <returns></returns>
    bool CheckGameWin()
    {


        for (int i = 0; i < m_currentCachedWidth; i++)
        {
            for (int j = 0; j < m_currentCachedHeight; j++)
            {
                Block block = m_blocksCurrentGameState[i, j];

                //Checking the logic, If there's some blocks available those are yet to be revealed and those are not mines.
                if (block._blockType != BLOCK_TYPE.MINE && !block.isBlockRevealed)
                    return false;
            }
        }



        IsGameStarted = false;
        IsGameOver = true;
        m_currentFlaggedMinesCount = 0;

        m_gameBoardView.UpdateRestartButtonWinLoseStatus(false,false,true);
        m_gameBoardView.UpdateAutoplayToggleStatus(true);


        if (CanOpenAllMinesAtGameEnd)
            RevealAllAvailableMines();


        return true;
    }

    /// <summary>
    /// Game Tie Condition When there's no Valid blocks available for Autoplay
    /// </summary>
    void TieGameCondition()
    {
        IsGameStarted = false;
        IsGameOver = true;
        m_currentFlaggedMinesCount = 0;

        m_gameBoardView.UpdateRestartButtonWinLoseStatus(true);
        m_gameBoardView.UpdateAutoplayToggleStatus(true);


        if (CanOpenAllMinesAtGameEnd)
            RevealAllAvailableMines();
    }


    /// <summary>
    /// Opening all the empty blocks near to it in all direction till founds any number
    /// </summary>
    /// <param name="block"></param>
    void OpenAdjacentEmptyBlocks(Block block)
    {
        if (block == null)
            return;

        if (block.isBlockRevealed || block.isBlockFlagged || block._blockType == BLOCK_TYPE.MINE)
            return;

        int xPos = block._blockPositionInGrid.x;
        int yPos = block._blockPositionInGrid.y;

        block.isBlockRevealed = true;
        m_blocksCurrentGameState[xPos, yPos] = block;


        if (block._blockType == BLOCK_TYPE.EMPTY)
        {
            OpenAdjacentEmptyBlocks(GetBlock(xPos - 1, yPos));//Left Direction Check
            OpenAdjacentEmptyBlocks(GetBlock(xPos + 1, yPos));//Right Direction Check
            OpenAdjacentEmptyBlocks(GetBlock(xPos, yPos - 1));//Down Direction Check
            OpenAdjacentEmptyBlocks(GetBlock(xPos, yPos + 1));//Up Direction Check
        }


    }





    /// <summary>
    /// Get Block By Position
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <returns></returns>
    Block GetBlock(int xPos, int yPos)
    {
        if (MinesweeperUtility.IsValidIndex(xPos, yPos, m_currentCachedWidth, m_currentCachedHeight))
        {
            return m_blocksCurrentGameState[xPos, yPos];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get Block by MouseClick
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <returns></returns>
    Block GetBlockOverMousePosition(out int xPos, out int yPos)
    {

        xPos = 0;
        yPos = 0;

        if (_cameraController == null)
        {
            Debug.Log("Camera Controller Can't be Null!");
            return null;
        }

        Camera cam = _cameraController.GetCurrentCamera();

        if (cam == null)
        {
            Debug.Log("Camera Can't be Null!");
            return null;
        }


        Vector3 capturedWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector3Int blockPositionOnGameBoard = m_gameBoardView.GameBoardTileMap.WorldToCell(capturedWorldPosition);

        Block block = GetBlock(blockPositionOnGameBoard.x, blockPositionOnGameBoard.y);

        xPos = blockPositionOnGameBoard.x;
        yPos = blockPositionOnGameBoard.y;

        return block;
    }


    //------------------------------------------------AUTOPLAY FEATURE----------------------------------


    #region AUTOPLAY_SPECIFIC_FUNCTIONS

    /// <summary>
    /// Get Random Block
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <returns></returns>
    Block GetBlockRandomelyAutoPlay(out int xPos, out int yPos)
    {

        xPos = -1;
        yPos = -1;

       int xPosIndex = Random.Range(0, m_currentCachedWidth);
       int yPosIndex = Random.Range(0, m_currentCachedHeight);


        if (MinesweeperUtility.IsValidIndex(xPosIndex, yPosIndex, m_currentCachedWidth, m_currentCachedHeight))
        {
            xPos = xPosIndex;
            yPos = yPosIndex;

            return m_blocksCurrentGameState[xPosIndex, yPosIndex];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Reveal the Random Block
    /// Same Function as before just uses random blocks
    /// </summary>
    public void RevealRandomBlockAutoPlay()
    {


        if (!IsBlocksAvailableForAutoPlay())
        {
            TieGameCondition();
            return;

        }

        if (IsAllValidBlocksRevealed())
        {
            Debug.Log("ALL VALID BLOCKS ARE REVEALED!!");
            return;
        }


        int xPos = -1;
        int yPos = -1;

        Block block = GetBlockRandomelyAutoPlay(out xPos,out yPos);





        if (!MinesweeperUtility.IsValidIndex(xPos, yPos, m_currentCachedWidth, m_currentCachedHeight))
        {

            RevealRandomBlockAutoPlay();
            return;
        }


        if (block.isBlockRevealed)
        {
            RevealRandomBlockAutoPlay();
            return;
        }


        if (block.isBlockFlagged)
        {
            RevealRandomBlockAutoPlay();

            return;
        }


        switch (block._blockType)
        {
            case BLOCK_TYPE.MINE:
                {
                    ExplodeMineOnGameBoard(block);
                    break;
                }
            case BLOCK_TYPE.EMPTY:
                {
                    OpenAdjacentEmptyBlocks(block);
                    CheckGameWin();
                    break;
                }
            default:
                {
                    block.isBlockRevealed = true;
                    m_blocksCurrentGameState[xPos, yPos] = block;
                    CheckGameWin();
                    break;
                }
        }

        m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);

    }


   /// <summary>
   /// Set Flag on Random Block but making sure if there's a number available and by using little knowledge of it.
   /// </summary>
   /// <param name="xPosition"></param>
   /// <param name="yPosition"></param>
    public void SetFlagOnRandomBlockAutoPlay(int xPosition,int yPosition)
    {

        if (!IsBlocksAvailableForAutoPlay())
        {
            TieGameCondition();
           return;
        
        }


        if (IsAllValidBlocksRevealed())
        {
            Debug.Log("ALL VALID BLOCKS ARE REVEALED!!");
            return;
        }     


        if (!MinesweeperUtility.IsValidIndex(xPosition, yPosition, m_currentCachedWidth, m_currentCachedHeight))
        {

            Debug.Log("NOT VALID LOCATION FOR FLAGGING");


            return;
        }


        Block block = m_blocksCurrentGameState[xPosition, yPosition];



        block.isBlockFlagged = !block.isBlockFlagged;
        m_blocksCurrentGameState[xPosition, yPosition] = block;


        if (block.isBlockFlagged)
            m_currentFlaggedMinesCount++;
        else
            m_currentFlaggedMinesCount--;



        m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);

        m_gameBoardView.UpdateRemainingMinesCount(m_currentCachedMinesCount - m_currentFlaggedMinesCount);
    }



    /// <summary>
    /// Check if all the valid non-mine blocks are revealed or not
    /// </summary>
    /// <returns></returns>
    bool IsAllValidBlocksRevealed()
    {
        for(int i=0;i<m_currentCachedWidth;i++)
        {
            for(int j=0;j<m_currentCachedHeight;j++)
            {
                if (!m_blocksCurrentGameState[i,j].isBlockRevealed && m_blocksCurrentGameState[i, j]._blockType!=BLOCK_TYPE.MINE)
                {
                    IsValidBlocksAvailable = true;
                    return false;
                }
                
            }
        }

        IsValidBlocksAvailable = false;
        return true;
    }

    bool IsBlocksAvailableForAutoPlay()
    {
        for (int i = 0; i < m_currentCachedWidth; i++)
        {
            for (int j = 0; j < m_currentCachedHeight; j++)
            {
                if (!m_blocksCurrentGameState[i, j].isBlockRevealed || m_blocksCurrentGameState[i, j]._blockType != BLOCK_TYPE.MINE || !m_blocksCurrentGameState[i, j].isBlockFlagged)
                {
                    return true;
                }

            }
        }

        return false;
    }


    /// <summary>
    /// Get Status Of Valid Blocks Available or not. 
    /// Valid Block means Which can be revealed without any harm.
    /// </summary>
    /// <returns></returns>
    public bool GetStatusValidBlocksAvailableAutoPlay()
    {
        return IsValidBlocksAvailable;
    }

    /// <summary>
    /// Get GameOver Status
    /// </summary>
    /// <returns></returns>
    public bool GetStatusGameFinishedAutoPlay()
    {
        return IsGameOver;
    }

    /// <summary>
    /// Get Game Start Status
    /// </summary>
    /// <returns></returns>
    public bool GetStatusGameStartedAutoPlay()
    {
        return IsGameStarted;
    }

    /// <summary>
    /// Get Current Gameboard State
    /// </summary>
    /// <returns></returns>
    public Block[,] GetCurrentStateAutoPlay()
    {
        return m_blocksCurrentGameState;
    }

    /// <summary>
    /// Get Status of Autoplay, Whether it is enabled or not
    /// </summary>
    /// <returns></returns>
    public bool GetAutoPlayStatus()
    {
        return isAutoPlayEnabled;
    }

    /// <summary>
    /// Get Adjancy blocks for AutoPlay Flagging by knowledge of numbers and nearby mines
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public List<Block> GetAdjancyBlocksAutoPlay(Block block)
    {

        List<Block> adjancyBlocks = new List<Block>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {

                if (i == 0 && j == 0)
                    continue;


                int adjacentXPos = block._blockPositionInGrid.x + i;
                int adjacentYPos = block._blockPositionInGrid.y + j;

                if (!MinesweeperUtility.IsValidIndex(adjacentXPos, adjacentYPos, m_currentCachedWidth, m_currentCachedHeight))
                    continue;

                adjancyBlocks.Add(m_blocksCurrentGameState[adjacentXPos, adjacentYPos]);

            }

        }


        return adjancyBlocks;
    }

    #endregion
}
