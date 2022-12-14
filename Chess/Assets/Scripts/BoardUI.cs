using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Moves;
using static UnityEditor.PlayerSettings;
using static UserInput;

public class BoardUI : MonoBehaviour
{
    public Color lightColour;
    public Color darkColour;
    public Sprite[] pieceSprites;
    public Sprite squareSprite;
    public Color availablePositionsColour;
    public Color currentPositionColour;

    [Header("Debug Properties")]
    public Color kingMovesColour;
    public Color kingCaptureColour;
    public Piece.Colour kingColour;
    public Piece.Type displayType;

    private Piece.Type pastDisplayType;
    private Piece.Colour pastDisplayColour;

    public Moves[] kingMoves;

    private SpriteRenderer[,] pieces;
    private SpriteRenderer[,] boardSquares;

    private GameObject draggingPiece;
    private Moves currentPossibleMoves;

    private GameObject piecesObject;
    private GameObject boardObject;

    private bool isDragging;
    private Vector2Int startPos;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseBoard();
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startPos = Board.GetBoardCoordFromWorld(mousePos);
            StartDrag(startPos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            DropPiece(startPos, Board.GetBoardCoordFromWorld(mousePos));
        }

        if (isDragging)
            DragPiece(mousePos);

        if(pastDisplayType != displayType || pastDisplayColour != kingColour)
        {
            pastDisplayType = displayType;
            pastDisplayColour = kingColour;
            ResetKingSquares();
            kingMoves = Piece.KingMoves(kingColour, Board.kingPositions[(int)kingColour]);
            DisplayKingSquares();
        }
    }

    void InitialiseBoard()
    {
        Board.InitializeBoard();
        //Clear Old Board
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        piecesObject = new GameObject("Pieces");
        boardObject = new GameObject("Board Pieces");

        pieces = new SpriteRenderer[8, 8];
        boardSquares = new SpriteRenderer[8, 8];

        piecesObject.transform.parent = transform;
        boardObject.transform.parent = transform;
        
        for (int rank = 0; rank < 8; rank++)
        {
            for(int file = 0; file < 8; file++)
            {
                //Display Board Square
                SpriteRenderer squareRenderer = new GameObject("Board").AddComponent<SpriteRenderer>();
                squareRenderer.sprite = squareSprite;
                squareRenderer.transform.parent = boardObject.transform;
                squareRenderer.color = (file + rank) % 2 != 0 ? lightColour : darkColour;
                squareRenderer.transform.position = Board.PositionFromCoord(file, rank);
                squareRenderer.transform.localScale = Vector3.one * 1.25f;
                boardSquares[file, rank] = squareRenderer;

                if (Board.board[file, rank] == null)
                    continue;

                //Get the index of the sprite in the sprite array
                int spriteIndex = (int)(Board.board[file, rank].type) + (Board.board[file, rank].colour == Piece.Colour.WHITE ? 0 : 6);
                
                //Display Piece
                SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
                pieceRenderer.sprite = pieceSprites[spriteIndex];
                pieceRenderer.sortingOrder = 1;
                pieceRenderer.transform.parent = piecesObject.transform;
                pieceRenderer.transform.position = Board.PositionFromCoord(file, rank);
                pieceRenderer.transform.localScale = Vector3.one * 0.39f;

                pieces[file, rank] = pieceRenderer;
            }
        }
    }

    public void StartDrag(Vector2Int pos)
    {
        if (Board.board[pos.x, pos.y] != null && Board.board[pos.x, pos.y].colour == Board.nextMoveColour)
        {
            draggingPiece = pieces[pos.x, pos.y].gameObject;
            pieces[pos.x, pos.y].sortingOrder = 2;
            currentPossibleMoves = Board.board[pos.x, pos.y].GetMoves(pos);
            ShowAvailableSquares(currentPossibleMoves);
        }
    }

    public void DropPiece(Vector2Int start, Vector2Int end)
    {
        //DEBUG
        ResetKingSquares();
        //
        Moves.Move curMove = currentPossibleMoves.GetMove(end);
        if (currentPossibleMoves.Contains(end))
        {
            Board.MoveInfo move = Board.MakeMove(start, curMove);

            if (move.otherPiece != null)
            {
                //Debug.Log(move.otherPiece.type);
                Destroy(pieces[move.otherPiece.Position.x, move.otherPiece.Position.y].gameObject);
            }
            pieces[move.start.x, move.start.y].sortingOrder = 1;
            pieces[move.start.x, move.start.y].transform.position = Board.PositionFromCoord(move.end);

            if (move.HasFlag(Move.Flag.CONVERT_QUEEN))
            {
                pieces[move.start.x, move.start.y].sprite = pieceSprites[(int)Piece.Type.QUEEN];
            }

            pieces[move.end.x, move.end.y] = pieces[move.start.x, move.start.y];
            pieces[move.start.x, move.start.y] = null;
        }
        else
        {
            if(draggingPiece != null)
            {
                draggingPiece.transform.position = Board.PositionFromCoord(start);
                pieces[start.x, start.y].sortingOrder = 1;
            }
            
        }

        //Board.DebugShow();

        //DEBUG
        if (kingColour != Piece.Colour.NONE)
            kingMoves = Piece.KingMoves(kingColour, Board.kingPositions[(int)kingColour]);
        else
            kingMoves = null;
        DisplayKingSquares();
        //

        ResetSquares(currentPossibleMoves);

        draggingPiece = null;
        currentPossibleMoves = new Moves();
    }

    public void DragPiece(Vector2 coord)
    {
        if(draggingPiece != null)
            draggingPiece.transform.position = coord;
    }

    void ShowAvailableSquares(in Moves moves)
    {
        DisplayKingSquares();
        Vector2Int pos = moves.StartPos;
        boardSquares[pos.x, pos.y].color *= currentPositionColour;

        for (int i = 0; i < moves.Count; i++)
        {
            pos = moves[i].EndPosition;
            boardSquares[pos.x, pos.y].color *= availablePositionsColour;
        }
    }

    void DisplayKingSquares()
    {
        if (kingMoves == null)
            return;

        if(displayType == Piece.Type.NONE)
        {
            foreach (Moves moves in kingMoves)
            {
                if (moves.endPositions == null)
                    continue;
                Color squareCol = ((moves.StartPos.x + moves.StartPos.y) % 2) != 0 ? lightColour : darkColour;
                boardSquares[moves.StartPos.x, moves.StartPos.y].color = squareCol * kingMovesColour;
                for (int i = 0; i < moves.Count; i++)
                {
                    Vector2Int pos = moves[i].EndPosition;
                    squareCol = ((pos.x + pos.y) % 2) != 0 ? lightColour : darkColour;
                    boardSquares[pos.x, pos.y].color = squareCol * kingMovesColour;
                }
            }

            foreach (Moves moves in kingMoves)
            {
                if (moves.capturePositions == null)
                    continue;
                Color squareCol;
                foreach (Moves.Move m in moves.capturePositions)
                {
                    Vector2Int pos = m.EndPosition;
                    squareCol = ((pos.x + pos.y) % 2) != 0 ? lightColour : darkColour;
                    boardSquares[pos.x, pos.y].color = squareCol * kingCaptureColour;
                }
            }
        }
        else
        {
            Moves moves = kingMoves[(int)displayType];
            Color squareCol = ((moves.StartPos.x + moves.StartPos.y) % 2) != 0 ? lightColour : darkColour;

            if (moves.endPositions != null)
            {
                boardSquares[moves.StartPos.x, moves.StartPos.y].color = squareCol * kingMovesColour;
                for (int i = 0; i < moves.Count; i++)
                {
                    Vector2Int pos = moves[i].EndPosition;
                    squareCol = ((pos.x + pos.y) % 2) != 0 ? lightColour : darkColour;
                    boardSquares[pos.x, pos.y].color = squareCol * kingMovesColour;
                }
            }

            if (moves.capturePositions != null)
            {
                foreach (Moves.Move m in moves.capturePositions)
                {
                    Vector2Int pos = m.EndPosition;
                    squareCol = (pos.x + pos.y % 2) != 0 ? lightColour : darkColour;
                    boardSquares[pos.x, pos.y].color = squareCol * kingCaptureColour;
                }
            }
        }
    }

    void ResetKingSquares()
    {
        if (kingMoves == null)
            return;

        foreach(Moves moves in kingMoves)
        {
            ResetSquares(moves, true);
        }
    }

    void ResetSquares(in Moves moves, bool king = false)
    {
        Vector2Int pos = moves.StartPos;
        boardSquares[pos.x, pos.y].color = (pos.x + pos.y) % 2 != 0 ? lightColour : darkColour;
        for (int i = 0; i < moves.Count; i++)
        {
            pos = moves[i].EndPosition;
            boardSquares[pos.x, pos.y].color = (pos.x + pos.y) % 2 != 0 ? lightColour : darkColour;
        }

        if(!king)
            DisplayKingSquares();
    }
}
