using UnityEngine.Tilemaps;
using UnityEngine;

//storing block types
public enum Tetromino
{
    I,O,T,J,L,S,Z
}

//store data for each tetromino
[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;
    //arraylist to define coordinates to form the shapes
    public Vector2Int[] cells { get; private set; }
    //init wall kick data
    public Vector2Int[,] wallKicks { get; private set; }

    //assigns the data to the coordinates for the cells from the Data class
    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino];
        this.wallKicks = Data.WallKicks[tetromino];
    }
}