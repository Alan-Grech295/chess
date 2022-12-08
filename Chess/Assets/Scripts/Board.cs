using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Piece;
using static UnityEditor.PlayerSettings;

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
    public class InvalidFENException : Exception
    {
        public InvalidFENException(string message) : base(message)
        {
        }
    }

    public struct MoveInfo
    {
        public Vector2Int start, end;
        public Piece movingPiece;
        public Piece capturedPiece;

        public MoveInfo(Vector2Int start, Vector2Int end, Piece movingPiece, Piece capturedPiece = null)
        {
            this.start = start;
            this.end = end;
            this.movingPiece = movingPiece;
            this.capturedPiece = capturedPiece;
        }

        public void Show()
        {
            Debug.Log(start);
            Debug.Log(end);
            Debug.Log(movingPiece.type);
            if(capturedPiece != null)
                Debug.Log(capturedPiece.type);
        }
    }

    public static Piece[,] board = new Piece[8, 8];
    public static bool whiteIsBottom = true;

    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static Distance[,] distanceToEdge;

    public static List<Piece>[] pieces;
    public static Vector2Int[] kingPositions;

    //private static List<MoveInfo> lastMoves;

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

        kingPositions = new Vector2Int[2];
        //lastMoves = new List<MoveInfo>();
        pieces = new List<Piece>[2];
        pieces[0] = new List<Piece>();
        pieces[1] = new List<Piece>();

        InitializeBoardFromFEN(startFEN);
    }

    public static int GetDir(Colour col)
    {
        int dir = (col == Colour.BLACK ? 1 : -1);
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
                    Colour colour = char.IsUpper(c) ? Colour.WHITE : Colour.BLACK;
                    char lower = char.ToLower(c);
                    if (!pieceTypeFromSymbol.ContainsKey(lower))
                        throw new InvalidFENException("Invalid character entered (" + c + ")");

                    Piece.Type type = pieceTypeFromSymbol[lower];
                    if(type == Piece.Type.KING)
                    {
                        kingPositions[(int)colour] = new Vector2Int(file, rank);
                    }
                    board[file, rank] = Piece.Create(colour, type, new Vector2Int(file, rank));
                    pieces[(int)colour].Add(board[file, rank]);

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

    public static MoveInfo[] MakeMove(Vector2Int start, Moves.Move move, bool commit = true)
    {
        Vector2Int end = move.EndPosition;
        Piece piece = board[start.x, start.y];
        Piece targetPiece = board[end.x, end.y];

        if (piece.type == Piece.Type.KING)
        {
            kingPositions[(int)piece.colour] = end;
        }

        //If Castling
        if (move.HasFlag(Moves.Move.Flag.CASTLE))
        {
            Vector2Int newPiecePos = new Vector2Int(end.x == 7 ? 6 : 2, start.y);
            Vector2Int newTargetPos = new Vector2Int(end.x == 7 ? 5 : 3, start.y);
            if (commit)
            {
                piece.numMoves++;
                piece.Position = newPiecePos;
                targetPiece.Position = newTargetPos;
            }

            board[newPiecePos.x, newPiecePos.y] = piece;
            board[newTargetPos.x, newTargetPos.y] = targetPiece;
            board[start.x, start.y] = null;
            board[end.x, end.y] = null;

            return new MoveInfo[] { new MoveInfo(start, newPiecePos, piece, null),
                                        new MoveInfo(end, newTargetPos, targetPiece, null)};
        }

        MoveInfo[] outMoves = new MoveInfo[] { new MoveInfo(start, end, piece) };

        if(commit)
        {
            piece.numMoves++;
            piece.Position = end;
        }

        if (move.HasFlag(Moves.Move.Flag.CAPTURE))
            outMoves[0].capturedPiece = CapturePiece(piece.colour, move, commit);

        board[end.x, end.y] = piece;
        board[start.x, start.y] = null;

        /*if (commit)
            Debug.Log("Final Move");

        DebugShow();
        Debug.Log("Flags: " + move.moveFlags);*/

        return outMoves;
    }

    public static void UnmakeMove(MoveInfo[] lastMoves)
    {
        foreach(MoveInfo lastMove in lastMoves)
        {
            if(lastMove.movingPiece.type == Piece.Type.KING)
                kingPositions[(int)lastMove.movingPiece.colour] = lastMove.start;

            lastMove.movingPiece.Position = lastMove.start;

            board[lastMove.start.x, lastMove.start.y] = lastMove.movingPiece;
            if (lastMove.capturedPiece != null)
                board[lastMove.capturedPiece.Position.x, lastMove.capturedPiece.Position.y] = lastMove.capturedPiece;
            else
                board[lastMove.end.x, lastMove.end.y] = null;
        }
    }

    public static Piece CapturePiece(Colour colour, Moves.Move move, bool commit)
    {
        Piece captured = null;
        if (move.HasFlag(Moves.Move.Flag.EN_PASSANT))
        {
            int enPassantDir = colour == Colour.WHITE ? 1 : -1;
            enPassantDir = whiteIsBottom ? enPassantDir : -enPassantDir;
            captured = board[move.EndPosition.x, move.EndPosition.y + enPassantDir];
            board[move.EndPosition.x, move.EndPosition.y + enPassantDir] = null;
        }
        else
        {
            Vector2Int pos = move.EndPosition;
            captured = board[pos.x, pos.y];
            board[pos.x, pos.y] = null;
        }

        if (captured == null)
        {
            Debug.LogError("Trying to capture null piece " + captured.Position);
            return null;
        }

        pieces[(int)captured.colour].Remove(captured);
        return captured;
    }

    public static bool HasPiece(Vector2Int position, Colour mask = Colour.NONE)
    {
        Piece boardPiece = board[position.x, position.y];
        if (boardPiece == null)
            return false;

        if (mask == Colour.NONE)
            return true;

        return boardPiece.colour == mask;
    }

    public static bool ValidPosition(Vector2Int position)
    {
        if (position.x < 0 || position.y < 0 || position.x > 7 || position.y > 7)
            return false;
        return true;
    }

    public static void DebugShow()
    {
        string str = "";
        for (int y = 0; y < 8; y++)
        {
            for(int x = 0; x < 8; x++)
            {
                if(board[x, y] != null)
                {
                    char c = board[x, y].type.ToString()[0];
                    str += (board[x,y].colour == Colour.WHITE ? c : char.ToLower(c)) + "  ";
                }
                else
                {
                    str += "0  ";
                }
            }
            str += "\n";
        }
        Debug.Log(str);

        //for(int y = 0; y < 8; y++)
        //{
        //    string str = "";
        //    for(int x = 0; x < 8; x++)
        //    {
        //        if(board[x, y] != null)
        //            str += board[x,y].Position + " ";
        //        else
        //            str += "       ";
        //    }
        //    Debug.Log(str);
        //}
    }
}
