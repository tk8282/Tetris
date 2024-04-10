using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    //used to store a copy of cell data
    public Vector3Int[] cells { get; private set; }

    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int position { get; private set; }

    private bool level2 = false;
    private bool level3  = false;
    private bool level4  = false;
    private bool level5  = false;
    private bool level6  = false;
    private bool level7  = false;
    private bool level8  = false;
    private bool level9  = false;
    private bool level10  = false;

    //store current index
    public int rotationIndex { get; private set; }

    //"Step" is when the game forces the piece down one level (1 second at default)
    public float stepDelay = 1f;

    //lock is the amount of time before the game "Locks" the piece for the player 
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;
    
    //re initialized every time for a new piece. takes in spawn positon, tetromino data, reference to board
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;

        //if array was never initialized
        if(this.cells == null)
        {
            this.cells = new Vector3Int[4];
        }

        for(int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    //handling player input
    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        //rotate piece left
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(1);
        }
        //rotate piece right
        else if(Input.GetKeyDown(KeyCode.E))
        {
            Rotate(-1);
        }
        //left
        if(Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        //right
        else if(Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }
        //softdrop
        else if(Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector2Int.down);
        }
        //hard drop
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            //as long as move is successful, the while loop will stop moving the piece down
            while(Move(Vector2Int.down))
            {
                continue;
            }
        }
        //checks current time, if it does call step
        if(Time.time >= this.stepTime)
        {
            Step();
        }

        LevelChange(ref level2, 30);
        LevelChange(ref level3, 60);
        LevelChange(ref level4, 90);
        LevelChange(ref level5, 120);
        LevelChange(ref level6, 150);
        LevelChange(ref level7, 180);
        LevelChange(ref level8, 210);
        LevelChange(ref level9, 240);
        LevelChange(ref level10, 270);

        this.board.Set(this);

    }

    private void Step()
    {
        //pushing time a little further into future to have it called again
        this.stepTime = Time.time + this.stepDelay;

        Move(Vector2Int.down);

        //check for a lock
        if(this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    //function for locking a piece
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    //function for moving piece
    private bool Move(Vector2Int translation)
    {
        //verifying position before movement
        //calculating position
        Vector3Int newPos = this.position;
        newPos.x += translation.x;
        newPos.y += translation.y;

        //checks that the next move is valid
        bool valid = this.board.validPosition(this,newPos);

        if(valid)
        {
            this.position = newPos;
            //will reset locktime when player makes a move (will also apply to both reg move and rotates)
            this.lockTime = 0f;
        }
        return valid;
    }

    //function from rotating piece
    private void Rotate(int direction)
    {
        //in event of wall kick test failing
        int startRotation = this.rotationIndex;

        //need to update rotation index and apply rotation matrix
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        UseRotationMatrix(direction);
       
        //wall kick tests
        if(!WallKickTest(this.rotationIndex, direction))
        {
            this.rotationIndex = startRotation;
            UseRotationMatrix(-direction);
        }
    }

    private void UseRotationMatrix(int direction)
    {
        //rotate matrix (Using SRS)
        for(int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            //new coordinates
            int x,y;

            switch(this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                break;
                
                //for default rotations (I and O are special cases)
                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                break;
            }

            //assigning new rotated coordinates
            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    //used to ensure that when the index is changed by rotate, that it wraps in the correct fashion
    private int Wrap(int input, int min, int max)
    {
        if(input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    //function to test wall kicks
    private bool WallKickTest(int rotationIndex, int rotationalDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationalDirection);

        for(int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            //using the data tests
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
            //if move is true, wall kick is valid
            if(Move(translation))
            {
                return true;
            }
        }
        //else return false 
        return false;
    }

    //returns the index for wallkicktest so it knows which type of test to run
    private int GetWallKickIndex(int rotationIndex, int rotationalDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if(rotationalDirection < 0)
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0 , this.data.wallKicks.GetLength(0));
    }

    //when the game reaches a certain time frame, decrease the step delay
    private void LevelChange(ref bool triggered, float timeCondition)
    {
        if (!triggered && Time.time > timeCondition)
        {
            stepDelay -= 0.2f;
            triggered = true; // Mark as triggered so it doesn't run again
        }
    }


}
