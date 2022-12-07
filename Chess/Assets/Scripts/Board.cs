using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Distance
{
    public byte[] distances;
    public static Vector2Int[] straightDirections = new Vector2Int[4]{
        Vector2Int.left, Vector2Int.right,
        Vector2Int.down, Vector2Int.up
    };
    public static Vector2Int[] diagonalDirections = new Vector2Int[4]{
        Vector2Int.left + Vector2Int.down, Vector2Int.right + Vector2Int.down,
        Vector2Int.left + Vector2Int.up, Vector2Int.right + Vector2Int.up
    };

    public Distance(int left, int right, int up, int down)
    {
        distances = new byte[4];
        distances[0] = (byte)left;
        distances[1] = (byte)right;
        distances[2] = (byte)up;
        distances[3] = (byte)down;
    }

    public byte GetDiagonalDistance(int index)
    {
        return Math.Min(distances[index & 1], distances[((index & 2) >> 1) + 2]);
    }
}

public static class Board
{
    public static Piece[,] board = new Piece[8, 8];
    public static bool whiteIsBottom = true;

    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static Distance[,] distanceToEdge;

    public class InvalidFENException : Exception
    {
        public InvalidFENException(string message) : base(message)
        {
        }
    }

    static Dictionary<char, Piece.Type> pieceTypeFromSymbol = new Dictionary<char, Piece.Type>()
    {
        ['k'] = Piece.Type.KING,
        ['p'] = Piece.Type.PAWN,
        ['n'] = Piece.Type.KNIGHT,
        ['b'] = Piece.Type.BISHOP,
        ['r'] = Piece.Type.ROOK,
        ['q'] = Piece.Type.QUEEN
    };

    public static void InitializeBoard()
    {
        board = new Piece[8, 8];
        distanceToEdge = new Distance[8, 8];
        for(int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                distanceToEdge[x, y] = new Distance(x, 7 - x, y, 7 - y);
            }
        }

        InitializeBoardFromFEN(startFEN);
    }

    public static int GetDir(Piece.Colour col)
    {
        int dir = (col == Piece.Colour.BLACK ? 1 : -1);
        return whiteIsBottom ? dir : -dir;
    }

    public static Vector3 PositionFromCoord(int file, int rank, float depth = 0)
    {
        if (whiteIsBottom)
        {
            return new Vector3(-4.375f + file * 1.25f, 4.375f - rank * 1.25f, depth);
        }
        return new Vector3(4.375f - file * 1.25f, -4.375f + rank * 1.25f, depth);

    }

    public static Vector3 PositionFromCoord(Vector2Int pos, float depth = 0)
    {
        return PositionFromCoord(pos.x, pos.y, depth);
    }

    public static void InitializeBoardFromFEN(string fen) 
    {
        string[] sections = fen.Split(' ');

        //Board layout
        string[] ranks = sections[0].Split("/");
        if(ranks.Length != 8)
        {
            throw new InvalidFENException("Number of ranks given is not 8");
        }

        int rank = 0;
        foreach(string r in ranks)
        {
            int file = 0;
            foreach(char c in r)
            {
                if(char.IsDigit(c))
                {
                    file += (int)char.GetNumericValue(c);
                }
                else
                {
                    Piece.Colour colour = char.IsUpper(c) ? Piece.Colour.WHITE : Piece.Colour.BLACK;
                    char lower = char.ToLower(c);
                    if (!pieceTypeFromSymbol.ContainsKey(lower))
                        throw new InvalidFENException("Invalid character entered (" + c + ")");

                    Piece.Type type = pieceTypeFromSymbol[lower];
                    board[file, rank] = Piece.Create(colour, type);

                    file++;
                }
            }
            rank++;
        }

        whiteIsBottom = sections[1][0] == 'w';
    }

    public static Vector2Int GetBoardCoordFromWorld(Vector2 worldCoord)
    {
        int file = (int)Math.Min(7, Math.Floor(Math.Max((worldCoord.x + 4.375f + (1.25f / 2)) / 1.25f, 0)));
        int rank = (int)Math.Min(7, Math.Floor(Math.Max(8 - (worldCoord.y + 4.375f + (1.25f / 2)) / 1.25f, 0)));

        if (whiteIsBottom)
        {            
            return new Vector2Int(file, rank);
        }

        return new Vector2Int(7 - file, 7 - rank);
    }

    public static void MovePiece(Vector2Int start, Vector2Int end)
    {
        board[end.x, end.y] = board[start.x, start.y];
        board[start.x, start.y] = null;
    }

    public static bool HasPiece(Vector2Int position, Piece.Colour mask = Piece.Colour.NONE)
    {
        

        Piece boardPiece = board[position.x, position.y];
        if (boardPiece == null)
            return false;

        if (mask == Piece.Colour.NONE)
            return true;

        return boardPiece.colour == mask;
    }

    public static bool ValidPosition(Vector2Int position)
    {
        if (position.x < 0 || position.y < 0 || position.x > 7 || position.y > 7)
            return false;
        return true;
    }
}
