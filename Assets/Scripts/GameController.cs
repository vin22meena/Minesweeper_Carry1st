using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] GameInput _gameInput;
    [SerializeField] CameraController _cameraController;

    [SerializeField] TextAsset _levelData;

    LevelData m_currentCachedLevelData;

    int m_currentCachedWidth = 0;
    int m_currentCachedHeight = 0;
    int m_currentCachedMinesCount = 0;


    GameBoardView m_gameBoardView;
    Block[,] m_blocksCurrentGameState;


    bool IsGameStarted = false;
    bool IsGameOver = false;


    bool IsValidBlocksAvailable = false;

    [Header("GAME SETTINGS")]
    [SerializeField] bool CanOpenAllMinesAtGameEnd;


    float m_currentGameTimer = 0f;
    int m_currentFlaggedMinesCount = 0;


    private void Start()
    {
        m_gameBoardView = GetComponent<GameBoardView>();
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

    void PopulateRandomLevelData()
    {

        int width = Random.Range(4, 32);
        int height = Random.Range(4, 32);

        m_currentCachedWidth = width;
        m_currentCachedHeight = height;
        m_currentCachedMinesCount = Random.Range(0, width * height);

        GenerateLevel(width, height);
    }

    void GenerateLevel(int width, int height)
    {

        IsGameStarted = true;
        IsGameOver = false;

        _cameraController.SetCameraDimension(width, height);

        m_blocksCurrentGameState = new Block[width, height];

        GenerateBlocksOnGameBoard();
        GenerateRandomMinesOnGameBoard();
        GenerateNumbersOnGameBoard();


        //m_gameBoardView.DrawGameUI(m_blocksCurrentGameState);
        m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);
        m_gameBoardView.UpdateRemainingMinesCount(m_currentCachedMinesCount);
        m_gameBoardView.UpdateRestartButtonWinLoseStatus(true, true);
        m_gameBoardView.AssignListenerToRestartButton(ExtractLevelData);


    }



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



    void ExplodeMineOnGameBoard(Block block)
    {
        IsGameStarted = false;
        IsGameOver = true;

        m_currentFlaggedMinesCount = 0;

        block.isBlockRevealed = true;
        block.isBlockExploded = true;
        m_blocksCurrentGameState[block._blockPositionInGrid.x, block._blockPositionInGrid.y] = block;

        m_gameBoardView.UpdateRestartButtonWinLoseStatus(false);

        Debug.Log("Lose");

        if (CanOpenAllMinesAtGameEnd)
            RevealAllAvailableMines();
    }


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


    bool CheckGameWin()
    {


        for (int i = 0; i < m_currentCachedWidth; i++)
        {
            for (int j = 0; j < m_currentCachedHeight; j++)
            {
                Block block = m_blocksCurrentGameState[i, j];

                if (block._blockType != BLOCK_TYPE.MINE && !block.isBlockRevealed)
                    return false;
            }
        }



        IsGameStarted = false;
        IsGameOver = true;
        m_currentFlaggedMinesCount = 0;

        m_gameBoardView.UpdateRestartButtonWinLoseStatus(true);


        if (CanOpenAllMinesAtGameEnd)
            RevealAllAvailableMines();


        return true;
    }




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


    public void RevealRandomBlockAutoPlay()
    {


       if(IsAllValidBlocksRevealed())
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


    public void SetFlagOnRandomBlockAutoPlay(int xPosition,int yPosition)
    {



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


    public void SetFlagOnRandomBlockAutoPlay()
    {


        if (IsAllValidBlocksRevealed())
        {
            Debug.Log("ALL VALID BLOCKS ARE REVEALED!!");
            return;
        }

        int xPos, yPos;
        Block block = GetBlockRandomelyAutoPlay(out xPos, out yPos);


        if (!MinesweeperUtility.IsValidIndex(xPos, yPos, m_currentCachedWidth, m_currentCachedHeight))
        {

            RevealRandomBlockAutoPlay();
            return;
        }



        if (block.isBlockRevealed)
        {
            SetFlagOnRandomBlockAutoPlay();
            return;
        }


        block.isBlockFlagged = !block.isBlockFlagged;
        m_blocksCurrentGameState[xPos, yPos] = block;


        if (block.isBlockFlagged)
            m_currentFlaggedMinesCount++;
        else
            m_currentFlaggedMinesCount--;



        m_gameBoardView.UpdateGameBoard(m_blocksCurrentGameState);

        m_gameBoardView.UpdateRemainingMinesCount(m_currentCachedMinesCount - m_currentFlaggedMinesCount);
    }

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


    public bool GetStatusValidBlocksAvailableAutoPlay()
    {
        return IsValidBlocksAvailable;
    }

    public bool GetStatusGameFinishedAutoPlay()
    {
        return IsGameOver;
    }

    public Block[,] GetCurrentStateAutoPlay()
    {
        return m_blocksCurrentGameState;
    }

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
}
