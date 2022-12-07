using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Visuals : MonoBehaviour
{
    public Color lightColour;
    public Color darkColour;
    public Sprite[] pieceSprites;
    public Sprite squareSprite;
    private SpriteRenderer[,] pieces;
    private SpriteRenderer[,] boardSquares;

    public int scale;

    private bool isDragging = false;
    private Vector2Int startDragPos;
    private GameObject draggingPiece;

    private GameObject piecesObject;
    private GameObject boardObject;

    // Start is called before the first frame update
    void Start()
    {
        //SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        //Texture2D texture = new Texture2D(8 * scale, 8 * scale);

        //texture.filterMode = FilterMode.Point;

        //for (int y = 0; y < 8 * scale; y++)
        //{
        //    for (int x = 0; x < 8 * scale; x++)
        //    {
        //        texture.SetPixel(x, y, (y / scale + x / scale) % 2 != 0 ? lightColour : darkColour);
        //    }
        //}
        //texture.Apply();
        //renderer.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), Vector2.zero);

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
                SpriteRenderer squareRenderer = new GameObject("Board").AddComponent<SpriteRenderer>();
                squareRenderer.sprite = squareSprite;
                squareRenderer.transform.parent = boardObject.transform;
                squareRenderer.color = (file + rank) % 2 != 0 ? lightColour : darkColour;
                squareRenderer.transform.position = Board.PositionFromCoord(file, rank);
                squareRenderer.transform.localScale = Vector3.one * 1.25f;
                boardSquares[file, rank] = squareRenderer;
                if (Board.board[file, rank] == null)
                    continue;

                int spriteIndex = (int)(Board.board[file, rank].type) + (Board.board[file, rank].colour == Piece.Colour.WHITE ? 0 : 6);
                
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(Input.GetMouseButtonDown(0))
        {
            startDragPos = Board.GetBoardCoordFromWorld(mousePos);

            if (Board.board[startDragPos.x, startDragPos.y] != null)
            {
                isDragging = true;
                draggingPiece = pieces[startDragPos.x, startDragPos.y].gameObject;
            }
        }

        if(Input.GetMouseButtonUp(0) && draggingPiece != null)
        {
            isDragging = false;
            Vector2Int boardCoord = Board.GetBoardCoordFromWorld(mousePos);
            Board.MovePiece(startDragPos, boardCoord);
            draggingPiece.transform.position = Board.PositionFromCoord(boardCoord.x, boardCoord.y);
            pieces[boardCoord.x, boardCoord.y] = pieces[startDragPos.x, startDragPos.y];
            pieces[startDragPos.x, startDragPos.y] = null;
            draggingPiece = null;
        }

        if(isDragging && draggingPiece != null)
        {
            draggingPiece.transform.position = mousePos;
        }
    }

}
