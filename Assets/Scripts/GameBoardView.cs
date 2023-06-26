using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Responsible for Drawing Block Grid and Updating based on Current Game State.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class GameBoardView : MonoBehaviour
{

    /// <summary>
    /// Grid Tilemap Propery
    /// </summary>
    public Tilemap GameBoardTileMap { get; private set; }


    [Header("UI SETTINGS")]
    [SerializeField]Tile[] _tiles; 
    [SerializeField] Sprite[] _restartButtonSprites;
    [SerializeField]TMP_Text _timeCounterText;
    [SerializeField]TMP_Text _remainingMinesCountText;
    [SerializeField]Button _restartButton;
    [SerializeField] TMP_Text _instructionText;
    [SerializeField] Toggle _autoPlayToggle;


    private void Awake()
    {
        GameBoardTileMap = GetComponent<Tilemap>();
    }



    private void OnDisable()
    {
        if(_restartButton)
        {
            if(_restartButton.onClick!=null)
                _restartButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Update the Gameboard based on Current Game State
    /// </summary>
    /// <param name="currentBoardState"></param>
    public void UpdateGameBoard(Block[,] currentBoardState)
    {
              
        int width = currentBoardState.GetLength(0);
        int height = currentBoardState.GetLength(1);

        for(int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            {
                Block fetchedBlock = currentBoardState[i, j];
                GameBoardTileMap.SetTile(fetchedBlock._blockPositionInGrid, GetCorrespondingTile(fetchedBlock));
            }
        }
    }



    /// <summary>
    /// Get the Required TileAsset from Array of Tiles for Placement
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
     Tile GetCorrespondingTile(Block block)
    {
        if(block.isBlockRevealed)
        {
            return GetRevealedTile(block);
        }
        else if(block.isBlockFlagged)
        {
            return _tiles[11];
        }
        else
        {
            return _tiles[10];
        }
        
    }


    /// <summary>
    /// Get the Revealed tile, Because Each Revealed tile has it's own function. So this function is responsible retreiving the revealed tile.
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    Tile GetRevealedTile(Block block)
    {
        switch (block._blockType) 
        {

            case BLOCK_TYPE.EMPTY:
                return _tiles[8];
            case BLOCK_TYPE.MINE:
                return block.isBlockExploded ? _tiles[9]:_tiles[12];
            case BLOCK_TYPE.NUMBER:
                return GetNumberedTile(block);
            default:
                return _tiles[10];
        }

    }


    /// <summary>
    /// Get Numbered Tile based on adjacent mines count
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    Tile GetNumberedTile(Block block)
    {
        if (block._number < 1 && block._number > 8)
            return _tiles[10];

        return _tiles[block._number - 1];

    }

    /// <summary>
    /// Update Time Counter
    /// </summary>
    /// <param name="timer"></param>
    public void UpdateGameBoardTimeCounter(int timer)=>_timeCounterText.text = string.Format("{0:000}", timer);
    
    /// <summary>
    /// Update Remaining Mines Count
    /// </summary>
    /// <param name="count"></param>
    public void UpdateRemainingMinesCount(int count) => _remainingMinesCountText.text = string.Format("{0:000}", count);

    /// <summary>
    /// Restart Button Listener Assignment
    /// </summary>
    /// <param name="action"></param>
    public void AssignListenerToRestartButton(UnityAction action)
    {
        if(_restartButton)
        {
            if (_restartButton.onClick != null)
                _restartButton.onClick.RemoveAllListeners();
        }

        _restartButton.onClick.AddListener(action);
    }

    /// <summary>
    /// Update Status of Restart Button on Different Game Result States.
    /// </summary>
    /// <param name="isWin"></param>
    /// <param name="isDefault"></param>
    public void UpdateRestartButtonWinLoseStatus(bool isWin,bool isDefault=false,bool isTie=false)
    {
        if (isDefault)
            _restartButton.image.sprite = _restartButtonSprites[2];
        
        else
        {
            if (isTie)
            {
                _restartButton.image.sprite = _restartButtonSprites[3];
            }
            else
            {
                if (isWin)
                    _restartButton.image.sprite = _restartButtonSprites[0];
                else
                    _restartButton.image.sprite = _restartButtonSprites[1];
            }

        }


    }

    /// <summary>
    /// Update Instruction Text
    /// </summary>
    /// <param name="instructionMessage"></param>
    public void UpdateInstructionText(string instructionMessage)
    {
        _instructionText.text = instructionMessage;
    }

    /// <summary>
    /// Get Autoplay Toggle isOn boolean
    /// </summary>
    /// <returns></returns>
    public bool GetAutoPlayToggleStatus()
    {
        return _autoPlayToggle.isOn;
    }

    
    /// <summary>
    /// Update Interactable status of Autoplay Toggle
    /// </summary>
    /// <param name="isInteractable"></param>
    public void UpdateAutoplayToggleStatus(bool isInteractable)
    {
        _autoPlayToggle.interactable = isInteractable;
    }

}
