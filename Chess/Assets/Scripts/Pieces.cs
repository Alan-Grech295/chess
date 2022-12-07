using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class Piece
{
    public enum Colour { BLACK, WHITE, NONE}
    public enum Type { PAWN, BISHOP, KNIGHT, ROOK, QUEEN, KING, NONE }
    public Colour colour;
    protected Colour enemyColour;
    public Type type;
    //Index in piece positions list in board
    public int pieceIndex;

    protected Piece(Colour colour, int index)
    {
        this.colour = colour;
        pieceIndex = index;
        enemyColour = colour == Colour.WHITE ? Colour.BLACK : Colour.WHITE;
    }

    public abstract Moves GetMoves(Vector2Int position, bool filterChecks = true);

    public static Piece Create(Colour colour, Type type, int index)
    {
        switch (type)
        {
            case Type.PAWN:
                return new Pawn(colour, index);
            case Type.BISHOP:
                return new Bishop(colour, index);
            case Type.KNIGHT:
                return new Knight(colour, index);
            case Type.ROOK:
                return new Rook(colour, index);
            case Type.QUEEN:
                return new Queen(colour, index);
            case Type.KING:
                return new King(colour, index);
        }

        return null;
    }

    protected static bool AddMoveIfValid(Vector2Int position, ref Moves moves, Colour mask = Colour.NONE)
    {
        if (Board.ValidPosition(position) && !Board.HasPiece(position, mask))
        {
            moves.Add(position);
            return true;
        }
        return false;
    }

    protected static bool AddMoveIfEnemy(Vector2Int position, ref Moves moves, Colour enemy)
    {
        if (Board.ValidPosition(position) && Board.HasPiece(position, enemy))
        {
            moves.Add(position);
            moves.AddCapturePosition(position);
            return true;
        }

        return false;
    }

    protected static Moves FilterMoves(Colour colour, Moves moves)
    {
        Moves newMoves = new Moves(moves);

        for(int i = 0; i < moves.Count; i++)
        {
            Vector2Int move = moves[i];
            Board.MakeMove(moves.StartPos, move);
            foreach(Type type in Enum.GetValues(typeof(Type)))
            {
                if(type == Type.NONE)
                    continue;

                Piece p = Piece.Create(colour, type, -1);
                Moves kingMoves = p.GetMoves(Board.kingPositions[(int)colour], false);

                bool foundCaptureMove = false;
                if(kingMoves.capturePositions != null)
                {
                    foreach(byte b in kingMoves.capturePositions)
                    {
                        Vector2Int capturePos = Moves.ToVec2(b);
                        if(Board.board[capturePos.x, capturePos.y].type == type)
                        {
                            Debug.Log("Capture at " + capturePos + " by " + type);
                            foundCaptureMove = true;
                            break;
                        }
                    }
                }

                if(foundCaptureMove)
                {
                    newMoves.Remove(move);
                    break;
                }
            }
            Board.UnmakeMove();
        }

        return newMoves;
    }
}

public struct Moves
{
    private byte startPos;
    public List<byte> _endPositions;
    public List<byte> capturePositions;

    public Moves(Vector2Int startPos) : this()
    {
        StartPos = startPos;
    }

    public Moves(Moves copy)
    {
        this.startPos = copy.startPos;
        if(copy._endPositions != null)
            this._endPositions = new List<byte>(copy._endPositions);
        else
            this._endPositions = null;

        if(copy.capturePositions != null)
            this.capturePositions = new List<byte>(copy.capturePositions);
        else
            this.capturePositions = null;
    }

    public Vector2Int StartPos
    {
        get { return new Vector2Int(startPos & 0xF, (startPos & 0xF0) >> 4); }
        set { startPos = (byte)((value.x & 0xF) | ((value.y & 0xF) << 4)); }
    }

    public int Count
    {
        get { return _endPositions == null ? 0 : _endPositions.Count; }
    }

    public Vector2Int this[int index]
    {
        get { return new Vector2Int(_endPositions[index] & 0xF, ((_endPositions[index] & 0xF0) >> 4)); }
        set { _endPositions[index] = (byte)((value.x & 0xF) | ((value.y & 0xF) << 4)); }
    }

    public void Add(Vector2Int pos)
    {
        if(_endPositions == null)
            _endPositions = new List<byte>();

        _endPositions.Add((byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4)));
    }

    public void Remove(Vector2Int pos)
    {
        _endPositions.Remove((byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4)));
        //capturePositions.Remove((byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4)));
    }

    public void AddCapturePosition(Vector2Int pos)
    {
        if(capturePositions == null)
            capturePositions = new List<byte>();

        capturePositions.Add((byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4)));
    }

    public bool Contains(Vector2Int pos)
    {
        if (_endPositions == null)
            return false;
        byte posByte = (byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4));
        return _endPositions.Contains(posByte);
    }

    public static Vector2Int ToVec2(byte b)
    {
        return new Vector2Int(b & 0xF, ((b & 0xF0) >> 4));
    }
}

public class Pawn : Piece
{
    public Pawn(Colour colour, int index) : base(colour, index)
    {
        type = Type.PAWN;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position, bool filterChecks = true)
    {
        Moves moves = new Moves(position);
        Vector2Int dir = colour == Colour.WHITE ? Vector2Int.down : Vector2Int.up;

        //Check if pawn can move 2 squares forward
        if (position.y == 6 && colour == Colour.WHITE)
        {
            AddMoveIfValid(position + dir * 2, ref moves);
        }

        if (position.y == 1 && colour == Colour.BLACK)
        {
            AddMoveIfValid(position + dir * 2, ref moves);
        }

        //Check if pawn can move 1 square forward
        AddMoveIfValid(position + dir, ref moves);

        //Check if pawn can go left
        AddMoveIfEnemy(position + dir + Vector2Int.left, ref moves, enemyColour);

        //Check if pawn can go right
        AddMoveIfEnemy(position + dir + Vector2Int.right, ref moves, enemyColour);

        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class Bishop : Piece
{
    public Bishop(Colour colour, int index) : base(colour, index)
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

                if (Board.HasPiece(newPos, enemyColour))
                {
                    moves.Add(newPos);
                    moves.AddCapturePosition(newPos);
                    break;
                }

                moves.Add(newPos);
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
    public Knight(Colour colour, int index) : base(colour, index)
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
    public Rook(Colour colour, int index) : base(colour, index)
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

                if (Board.HasPiece(newPos, enemyColour))
                {
                    moves.Add(newPos);
                    moves.AddCapturePosition(newPos);
                    break;
                }

                moves.Add(newPos);
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
    public Queen(Colour colour, int index) : base(colour, index)
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

                if (Board.HasPiece(newPos, enemyColour))
                {
                    moves.Add(newPos);
                    moves.AddCapturePosition(newPos);
                    break;
                }

                moves.Add(newPos);
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

                if (Board.HasPiece(newPos, enemyColour))
                {
                    moves.Add(newPos);
                    moves.AddCapturePosition(newPos);
                    break;
                }

                moves.Add(newPos);
            }
        }
        
        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}

public class King : Piece
{
    public King(Colour colour, int index) : base(colour, index)
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
        
        if(filterChecks)
            return FilterMoves(colour, moves);
        else
            return moves;
    }
}
