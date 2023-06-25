using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Tilemap))]
public class GameBoardView : MonoBehaviour
{

    [SerializeField]Tile[] _tiles; 
    
    public Tilemap GameBoardTileMap { get; private set; }


    [Header("UI SETTINGS")]
    [SerializeField] Sprite[] _restartButtonSprites;
    [SerializeField]TMP_Text _timeCounterText;
    [SerializeField]TMP_Text _remainingMinesCountText;
    [SerializeField]Button _restartButton;


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


    Tile GetNumberedTile(Block block)
    {
        if (block._number < 1 && block._number > 8)
            return _tiles[10];

        return _tiles[block._number - 1];

    }


    public void UpdateGameBoardTimeCounter(int timer)=>_timeCounterText.text = string.Format("{0:000}", timer);
    
    public void UpdateRemainingMinesCount(int count) => _remainingMinesCountText.text = string.Format("{0:000}", count);

    public void AssignListenerToRestartButton(UnityAction action)
    {
        if(_restartButton)
        {
            if (_restartButton.onClick != null)
                _restartButton.onClick.RemoveAllListeners();
        }

        _restartButton.onClick.AddListener(action);
    }
    public void UpdateRestartButtonWinLoseStatus(bool isWin,bool isDefault=false)
    {
        if (isDefault)
            _restartButton.image.sprite = _restartButtonSprites[2];
        
        else
        {
            if (isWin)
                _restartButton.image.sprite = _restartButtonSprites[0];
            else
                _restartButton.image.sprite = _restartButtonSprites[1];
        }


    }


}
