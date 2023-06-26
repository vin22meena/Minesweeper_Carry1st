using UnityEngine;

/// <summary>
/// Minesweeper Utility Class
/// </summary>
public static class MinesweeperUtility
{

    public static bool IsValidIndex(int x,int y,int xLimit,int yLimit)
    {
        return x >= 0 && x < xLimit && y >= 0 && y < yLimit;
    }




    
}
