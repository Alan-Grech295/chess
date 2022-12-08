using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.DebugUI;

public abstract class Piece
{
    public enum Colour { BLACK, WHITE, NONE}
    public enum Type { PAWN, BISHOP, KNIGHT, ROOK, QUEEN, KING, NONE }

    public Colour colour = Colour.NONE;
    public Type type = Type.NONE;
    public int numMoves = 0;

    private byte piecePos;
    protected Colour enemyColour;

    public Vector2Int Position{
        get { return Moves.ToVec2(piecePos);}
        set { piecePos = Moves.FromVec2(value);}
    }

    protected Piece(Colour colour, Vector2Int position)
    {
        this.colour = colour;
        Position = position;
        enemyColour = colour == Colour.WHITE ? Colour.BLACK : Colour.WHITE;
    }

    public abstract Moves GetMoves(Vector2Int position, bool filterChecks = true);

    public static Piece Create(Colour colour, Type type, Vector2Int position)
    {
        switch (type)
        {
            case Type.PAWN:
                return new Pawn(colour, position);
            case Type.BISHOP:
                return new Bishop(colour, position);
            case Type.KNIGHT:
                return new Knight(colour, position);
            case Type.ROOK:
                return new Rook(colour, position);
            case Type.QUEEN:
                return new Queen(colour, position);
            case Type.KING:
                return new King(colour, position);
        }

        return null;
    }

    //Converts a move 
    protected static Vector2Int ConvertWhiteMove(Colour colour, Vector2Int move)
    {
        if(Board.whiteIsBottom)
        {
            if (colour == Colour.WHITE)
                return move;
            return new Vector2Int(move.x, 7 - move.y);
        }
        else
        {
            if (colour == Colour.BLACK)
                return move;
            return new Vector2Int(move.x, 7 - move.y);
        }
    }
    protected static bool AddMoveIfValid(Vector2Int position, ref Moves moves, Colour mask = Colour.NONE)
    {
        if (Board.ValidPosition(position) && !Board.HasPiece(position, mask))
        {
            moves.Add(new Moves.Move(position));
            return true;
        }
        return false;
    }

    protected static bool AddMoveIfEnemy(Vector2Int position, ref Moves moves, Colour enemy)
    {
        if (Board.ValidPosition(position) && Board.HasPiece(position, enemy))
        {
            Moves.Move move = new Moves.Move(position, Moves.Move.Flag.CAPTURE);
            moves.Add(move);
            moves.AddCapturePosition(move);
            return true;
        }

        return false;
    }

    public static Moves[] KingMoves(Colour colour, Vector2Int position)
    {
        Moves[] moves = new Moves[Enum.GetValues(typeof(Type)).Length - 1];
        foreach (Type type in Enum.GetValues(typeof(Type)))
        {
            if (type == Type.NONE)
                continue;

            Piece p = Create(colour, type, position);
            Moves kingMoves = moves[(int)type] = p.GetMoves(position, false);
        }

        return moves;
    }

    protected static bool KingInCheck(Colour colour, Vector2Int position)
    {
        foreach(Type type in Enum.GetValues(typeof(Type)))
        {
            if(type == Type.NONE)
                continue;

            Piece p = Create(colour, type, position);
            Moves kingMoves = p.GetMoves(position, false);

            if(kingMoves.capturePositions != null)
            {
                foreach(Moves.Move m in kingMoves.capturePositions)
                {
                    Vector2Int capturePos = m.EndPosition;
                    if(Board.board[capturePos.x, capturePos.y].type == type)
                    {
                        //Debug.Log("Capture at " + capturePos + " by " + type + "\n" + kingMoves.ToString());
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected static Moves FilterMoves(Colour colour, Moves moves)
    {
        Moves newMoves = new Moves(moves);

        for(int i = 0; i < moves.Count; i++)
        {
            Moves.Move move = moves[i];
            Board.MoveInfo[] infos = Board.MakeMove(moves.StartPos, move, false);
            
            if(KingInCheck(colour, Board.kingPositions[(int)colour]))
                newMoves.Remove(move);

            Board.UnmakeMove(infos);
        }

        return newMoves;
    }
}

public struct Moves
{
    public struct Move
    {
        public byte endPosition;
        public Flag moveFlags;
        public enum Flag
        {
            NONE = 0,
            CAPTURE = 1,
            CASTLE = 2,
            EN_PASSANT = 4,
            CONVERT_QUEEN = 8,
        }

        public Vector2Int EndPosition
        {
            get { return ToVec2(endPosition); }
            set { endPosition = FromVec2(value); }
        }

        public Move(Vector2Int endPosition, Flag moveFlags = Flag.NONE)
        {
            this.endPosition = FromVec2(endPosition);
            this.moveFlags = moveFlags;
        }

        public bool HasFlag(Flag flag)
        {
            return (moveFlags & flag) != 0;
        }
    }

    private byte startPos;
    public List<Move> endPositions;
    public List<Move> capturePositions;

    public Moves(Vector2Int startPos) : this()
    {
        StartPos = startPos;
    }

    public Moves(Moves copy)
    {
        this.startPos = copy.startPos;
        if(copy.endPositions != null)
            this.endPositions = new List<Move>(copy.endPositions);
        else
            this.endPositions = null;

        if(copy.capturePositions != null)
            this.capturePositions = new List<Move>(copy.capturePositions);
        else
            this.capturePositions = null;
    }

    public Vector2Int StartPos
    {
        get { return ToVec2(startPos); }
        set { startPos = FromVec2(value); }
    }

    public int Count
    {
        get { return endPositions == null ? 0 : endPositions.Count; }
    }

    public Move this[int index]
    {
        get { return endPositions[index]; }
        set { endPositions[index] = value; }
    }

    public void Add(Move move)
    {
        if(endPositions == null)
            endPositions = new List<Move>();

        endPositions.Add(move);
    }

    public void Remove(Move move)
    {
        endPositions.Remove(move);
        //capturePositions.Remove(FromVec2(pos));
    }

    public void AddCapturePosition(Move move)
    {
        if(capturePositions == null)
            capturePositions = new List<Move>();

        capturePositions.Add(move);
    }

    public bool Contains(Move move)
    {
        if (endPositions == null)
            return false;

        return endPositions.Contains(move);
    }

    public bool Contains(Vector2Int move)
    {
        if (endPositions == null)
            return false;

        return endPositions.Where(i => i.EndPosition == move).GetEnumerator().MoveNext();
    }

    public Move GetMove(Vector2Int move)
    {
        if (endPositions == null)
            return new Move();
        return endPositions.Where(i => i.EndPosition == move).FirstOrDefault();
    }

    public static Vector2Int ToVec2(byte b)
    {
        return new Vector2Int(b & 0xF, ((b & 0xF0) >> 4));
    }

    public static byte FromVec2(Vector2Int vec)
    {
        return (byte)((vec.x & 0xF) | ((vec.y & 0xF) << 4));
    }

    //For Debug
    public override string ToString()
    {
        string str = "Moves:\n";
        if(endPositions != null)
        {
            foreach (Move m in endPositions)
            {
                str += "From " + ToVec2(startPos) + " to " + ToVec2(m.endPosition) + "\n";
            }
        }
        
        if(capturePositions != null)
        {
            str += "Captures:\n";
            foreach (Move m in capturePositions)
            {
                str += "From " + ToVec2(startPos) + " to " + ToVec2(m.endPosition) + "\n";
            }
        }

        return str;
    }
}

public class Pawn : Piece
{
    public Pawn(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.PAWN;
        this.colour = colour;
    }

    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        Vector2Int dir = colour == Colour.WHITE ? Vector2Int.down : Vector2Int.up;
        dir = Board.whiteIsBottom ? dir : -dir;

        //Check if pawn can move 1 square forward
        bool canMoveForward = AddMoveIfValid(position + dir, ref moves);

        //Check if pawn can move 2 squares forward
        if (canMoveForward && ConvertWhiteMove(colour, position).y == 6)
        {
            AddMoveIfValid(position + dir * 2, ref moves);
        }

        //Check if pawn can go left
        AddMoveIfEnemy(position + dir + Vector2Int.left, ref moves, enemyColour);

        //Check if pawn can go right
        AddMoveIfEnemy(position + dir + Vector2Int.right, ref moves, enemyColour);

        if(numMoves == 2 && ConvertWhiteMove(colour, position).y == 3)
        {
            //Check left for en passant
            if (Board.ValidPosition(position + Vector2Int.left) && Board.HasPiece(position + Vector2Int.left, enemyColour))
            {
                moves.Add(new Moves.Move(position + Vector2Int.left + dir, Moves.Move.Flag.CAPTURE | Moves.Move.Flag.EN_PASSANT));
            }

            //Check right for en passant
            if (Board.ValidPosition(position + Vector2Int.right) && Board.HasPiece(position + Vector2Int.right, enemyColour))
            {
                moves.Add(new Moves.Move(position + Vector2Int.right + dir, Moves.Move.Flag.CAPTURE | Moves.Move.Flag.EN_PASSANT));
            }
        }

        if (filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class Bishop : Piece
{
    public Bishop(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.BISHOP;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        Distance dist = Board.distanceToEdge[position.x, position.y];

        //Diagonal moves
        for (int j = 0; j < 4; j++)
        {
            byte d = dist.GetDiagonalDistance(j);
            Vector2Int dir = Distance.diagonalDirections[j];

            for (int i = 1; i <= d; i++)
            {
                Vector2Int newPos = position + dir * i;
                if (Board.HasPiece(newPos, colour))
                    break;

                Moves.Move move = new Moves.Move(newPos);

                if (Board.HasPiece(newPos, enemyColour))
                {
                    move.moveFlags = Moves.Move.Flag.CAPTURE;
                    moves.Add(move);
                    moves.AddCapturePosition(move);
                    break;
                }

                moves.Add(move);
            }
        }
        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class Knight : Piece
{
    public Knight(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.KNIGHT;
        this.colour = colour;
    }
    //TODO: Algorithm to enter moves not manual
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        bool add = AddMoveIfValid(position + Vector2Int.down * 2 + Vector2Int.left, ref moves) || AddMoveIfEnemy(position + Vector2Int.down * 2 + Vector2Int.left, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down * 2 + Vector2Int.right, ref moves) || AddMoveIfEnemy(position + Vector2Int.down * 2 + Vector2Int.right, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up * 2 + Vector2Int.left, ref moves) || AddMoveIfEnemy(position + Vector2Int.up * 2 + Vector2Int.left, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up * 2 + Vector2Int.right, ref moves) || AddMoveIfEnemy(position + Vector2Int.up * 2 + Vector2Int.right, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down + Vector2Int.left * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.down + Vector2Int.left * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up + Vector2Int.left * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.up + Vector2Int.left * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down + Vector2Int.right * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.down + Vector2Int.right * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up + Vector2Int.right * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.up + Vector2Int.right * 2, ref moves, enemyColour);
        
        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class Rook : Piece
{
    public Rook(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.ROOK;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        Distance dist = Board.distanceToEdge[position.x, position.y];

        //Straight moves
        for(int j = 0; j < 4; j++)
        {
            byte d = dist.distances[j];
            Vector2Int dir = Distance.straightDirections[j];

            for (int i = 1; i <= d; i++)
            {
                Vector2Int newPos = position + dir * i;
                if (Board.HasPiece(newPos, colour))
                    break;

                Moves.Move move = new Moves.Move(newPos);

                if (Board.HasPiece(newPos, enemyColour))
                {
                    move.moveFlags = Moves.Move.Flag.CAPTURE;
                    moves.Add(move);
                    moves.AddCapturePosition(move);
                    break;
                }

                moves.Add(move);
            }
        }

        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class Queen : Piece
{
    public Queen(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.QUEEN;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        Distance dist = Board.distanceToEdge[position.x, position.y];

        //Straight moves
        for (int j = 0; j < 4; j++)
        {
            byte d = dist.distances[j];
            Vector2Int dir = Distance.straightDirections[j];

            for (int i = 1; i <= d; i++)
            {
                Vector2Int newPos = position + dir * i;
                if (Board.HasPiece(newPos, colour))
                    break;

                Moves.Move move = new Moves.Move(newPos);

                if (Board.HasPiece(newPos, enemyColour))
                {
                    move.moveFlags = Moves.Move.Flag.CAPTURE;
                    moves.Add(move);
                    moves.AddCapturePosition(move);
                    break;
                }

                moves.Add(move);
            }
        }

        //Diagonal moves
        for (int j = 0; j < 4; j++)
        {
            byte d = dist.GetDiagonalDistance(j);
            Vector2Int dir = Distance.diagonalDirections[j];

            for (int i = 1; i <= d; i++)
            {
                Vector2Int newPos = position + dir * i;
                if (Board.HasPiece(newPos, colour))
                    break;

                Moves.Move move = new Moves.Move(newPos);

                if (Board.HasPiece(newPos, enemyColour))
                {
                    move.moveFlags = Moves.Move.Flag.CAPTURE;
                    moves.Add(move);
                    moves.AddCapturePosition(move);
                    break;
                }

                moves.Add(move);
            }
        }

        if (filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class King : Piece
{
    public King(Colour colour, Vector2Int position) : base(colour, position)
    {
        type = Type.KING;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        for(int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                    continue;

                bool addedMove = AddMoveIfValid(position + Vector2Int.left * x + Vector2Int.up * y, ref moves) ||
                AddMoveIfEnemy(position + Vector2Int.left * x + Vector2Int.up * y, ref moves, enemyColour);
            }
        }

        //Castling
        if(numMoves == 0 && filterChecks && !KingInCheck(colour, Position))
        {
            int yPos = Position.y;
            //Left
            Piece leftRook = Board.board[0, yPos];
            if(leftRook != null && leftRook.type == Type.ROOK && leftRook.numMoves == 0)
            {
                bool canCastle = true;
                for(int x = Position.x - 1; x > 0; x--)
                {
                    if(Board.board[x, yPos] != null || KingInCheck(colour, new Vector2Int(x, yPos)))
                    {
                        canCastle = false;
                        break;
                    }
                }

                if(canCastle)
                {
                    moves.Add(new Moves.Move(new Vector2Int(0, yPos), Moves.Move.Flag.CASTLE));
                }
            }

            //Right
            Piece rightRook = Board.board[7, yPos];
            if(rightRook != null && rightRook.type == Type.ROOK && rightRook.numMoves == 0)
            {
                bool canCastle = true;
                for(int x = Position.x + 1; x < 7; x++)
                {
                    if(Board.board[x, yPos] != null || KingInCheck(colour, new Vector2Int(x, yPos)))
                    {
                        canCastle = false;
                        break;
                    }
                }

                if(canCastle)
                {
                    moves.Add(new Moves.Move(new Vector2Int(7, yPos), Moves.Move.Flag.CASTLE));
                }
            }
        }
        
        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}
