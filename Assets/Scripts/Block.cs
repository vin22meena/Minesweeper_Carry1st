using UnityEngine;

/// <summary>
/// Block Types
/// </summary>
public enum BLOCK_TYPE 
{
    EMPTY,
    NUMBER,
    MINE
}


/// <summary>
/// BLOCK is the base of each block in GameBoard Grid.
/// Contains some properties
/// </summary>
public class Block
{
    public BLOCK_TYPE _blockType;

    public Vector3Int _blockPositionInGrid;

    public int _number;

    public bool isBlockRevealed;
    public bool isBlockExploded;
    public bool isBlockFlagged;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="position"></param>
    /// <param name="blockType"></param>
    /// <param name="number"></param>
    /// <param name="isRevealed"></param>
    /// <param name="isExploded"></param>
    /// <param name="isFlagged"></param>
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
