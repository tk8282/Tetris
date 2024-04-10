using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostPiece : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public GamePiece trackPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position{ get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for(int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePos = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePos, null);
        }
    }

    private void Copy()
    {
        for(int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackPiece.cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = trackPiece.position;

        int current = position.y;
        int bottom = -board.boardSize.y / 2 - 1;

        board.Clear(trackPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (board.validPosition(trackPiece, position)) {
                this.position = position;
            } else {
                break;
            }
        }

        board.Set(trackPiece);
    }

    private void Set()
    {
        for(int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePos = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePos, this.tile);
        }
    }
}
