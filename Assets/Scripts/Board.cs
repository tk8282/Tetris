using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    public GamePiece currentPiece { get; private set; }

    public Tilemap tilemap { get; private set; }

    public Vector3Int spawnPos;

    public Vector2Int boardSize = new Vector2Int(10,20);

    //calculates bounds
    //built in rect function
    public RectInt Bounds
    {
        get
        {
            //used to offset and get the corner of the board
            Vector2Int position = new Vector2Int(-this.boardSize.x /2, -this.boardSize.y /2);
            return new RectInt(position, this.boardSize);
        }
    }

    //array of tetromino data
    public TetrominoData[] tetrominoes;

    //call when component is initialized
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.currentPiece = GetComponentInChildren<GamePiece>();

        //loop through and call initialize from tetromino class
        for(int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {   
        //chooses a random tetromino from array
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.currentPiece.Initialize(this, this.spawnPos, data);

        //check for game over, if piece is not in a valid place
        if(validPosition(this.currentPiece, this.spawnPos))
        {
            Set(this.currentPiece);
        }
        else
        {
            GameOver();
        }

    }

    //function to show game over
    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
        SceneManager.LoadScene("GameOver");
    }

    //putting the pieces on the tile map
    public void Set(GamePiece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePos = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePos, piece.data.tile);
        }
    }

    //clear where the piece used to be before moving to the new location
    public void Clear(GamePiece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePos = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePos, null);
        }
    }

    //function to check if the next position is a valid position
    //make sure every cell of the piece is valid
    public bool validPosition(GamePiece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePos = piece.cells[i] + position;

            //if the position is not contained in the bounds, means it is out of bounds
            if(!bounds.Contains((Vector2Int)tilePos))
            {
                return false;
            }

            //checks if there is already a tile in a cell
            if(this.tilemap.HasTile(tilePos))
            {
                return false;
            }
        }
        return true;
    }

    //method to clear lines
    //go through every row and determine if every column is full
    public void ClearLines()
    {
        //iterate from bottom to top
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if(LineFull(row))
            {
                ClearLine(row);
            }
            else
            {
                row++;
            }
        }

    }

    //function to check if a line is completed
    private bool LineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for(int column = bounds.xMin; column < bounds.xMax; column++)
        {
            Vector3Int position = new Vector3Int(column, row, 0);

            //check if tile map does not have a tile at that position, line is false
            if(!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        //if all lines do not return false on the check, line is full
        return true;
    }

    //
    private void ClearLine(int row)
    {
        RectInt bounds = this.Bounds;

        for(int column = bounds.xMin; column < bounds.xMax; column++)
        {
            Vector3Int position = new Vector3Int(column, row, 0);
            //clear all the tiles from that set of tiles
            this.tilemap.SetTile(position, null);
        }

        //will move rows above the cleared row, down
        while(row < bounds.yMax)
        {
            for(int column = bounds.xMin; column < bounds.xMax; column++)
            {
                Vector3Int position = new Vector3Int(column, row + 1, 0);
                TileBase aboveTile = this.tilemap.GetTile(position);

                position = new Vector3Int(column, row, 0);
                this.tilemap.SetTile(position, aboveTile);
            }
            row++;
        }
    }
}
