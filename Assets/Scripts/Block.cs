using System.Collections.Generic;
using UnityEngine;

public enum BLOCK_TYPE 
{
    EMPTY,
    NUMBER,
    MINE
}


public class Block
{
    public BLOCK_TYPE _blockType;

    public Vector3Int _blockPositionInGrid;

    public int _number;

    public bool isBlockRevealed;
    public bool isBlockExploded;
    public bool isBlockFlagged;

    public Block(Vector3Int position, BLOCK_TYPE blockType=BLOCK_TYPE.EMPTY,int number=0,bool isRevealed=false,bool isExploded=false,bool isFlagged=false)
    {
        _blockType = blockType;
        _number = number;
        isBlockRevealed = isRevealed;
        isBlockExploded = isExploded;
        isBlockFlagged = isFlagged;
        _blockPositionInGrid = position;
    }


}
