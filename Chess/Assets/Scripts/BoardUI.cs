using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BoardUI : MonoBehaviour
{
    public Color lightColour;
    public Color darkColour;
    public Sprite[] pieceSprites;
    public Sprite squareSprite;
    public Color availablePositions;

    private SpriteRenderer[,] pieces;
    private SpriteRenderer[,] boardSquares;

    private Vector2Int startDragPos;
    private GameObject draggingPiece;
    private Moves currentPossibleMoves;

    private GameObject piecesObject;
    private GameObject boardObject;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseBoard();
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

    private void Update()
    {
        
    }

    public void StartDrag(Vector2 coord)
    {
        startDragPos = Board.GetBoardCoordFromWorld(coord);

        if (Board.board[startDragPos.x, startDragPos.y] != null)
        {
            draggingPiece = pieces[startDragPos.x, startDragPos.y].gameObject;
            pieces[startDragPos.x, startDragPos.y].sortingOrder = 2;
            currentPossibleMoves = Board.board[startDragPos.x, startDragPos.y].GetMoves(startDragPos);
            ShowAvailableSquares(currentPossibleMoves);
        }
    }

    public void DropPiece(Vector2 coord)
    {
        Vector2Int boardCoord = Board.GetBoardCoordFromWorld(coord);

        if(currentPossibleMoves.Contains(boardCoord))
        {
            Board.MovePiece(startDragPos, boardCoord);

            draggingPiece.transform.position = Board.PositionFromCoord(boardCoord);
            pieces[startDragPos.x, startDragPos.y].sortingOrder = 1;

            pieces[boardCoord.x, boardCoord.y] = pieces[startDragPos.x, startDragPos.y];
            pieces[startDragPos.x, startDragPos.y] = null;
        }
        else
        {
            draggingPiece.transform.position = Board.PositionFromCoord(startDragPos);
            pieces[startDragPos.x, startDragPos.y].sortingOrder = 1;
        }

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
        for(int i = 0; i < moves.Count; i++)
        {
            Vector2Int pos = moves[i];
            boardSquares[pos.x, pos.y].color *= availablePositions;
        }
    }

    void ResetSquares(in Moves moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            Vector2Int pos = moves[i];
            boardSquares[pos.x, pos.y].color = (pos.x + pos.y) % 2 != 0 ? lightColour : darkColour; ;
        }
    }
}
