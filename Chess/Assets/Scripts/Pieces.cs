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

    protected Piece(Colour colour)
    {
        this.colour = colour;
        enemyColour = colour == Colour.WHITE ? Colour.BLACK : Colour.WHITE;
    }

    public abstract Moves GetMoves(Vector2Int position);

    public static Piece Create(Colour colour, Type type)
    {
        switch (type)
        {
            case Type.PAWN:
                return new Pawn(colour);
            case Type.BISHOP:
                return new Bishop(colour);
            case Type.KNIGHT:
                return new Knight(colour);
            case Type.ROOK:
                return new Rook(colour);
            case Type.QUEEN:
                return new Queen(colour);
            case Type.KING:
                return new King(colour);
        }

        return null;
    }

    public static bool AddMoveIfValid(Vector2Int position, ref Moves moves, Colour mask = Colour.NONE)
    {
        if (Board.ValidPosition(position) && !Board.HasPiece(position, mask))
        {
            moves.Add(position);
            return true;
        }
        return false;
    }

    public static bool AddMoveIfEnemy(Vector2Int position, ref Moves moves, Colour enemy)
    {
        if (Board.ValidPosition(position) && Board.HasPiece(position, enemy))
        {
            moves.Add(position);
            return true;
        }

        return false;
    }
}

public struct Moves
{
    private byte startPos;
    public List<byte> _endPositions;
    public Vector2Int StartPos
    {
        get { return new Vector2Int(startPos & 0xF, startPos & 0xF0); }
        set { startPos = (byte)((value.x & 0xF) | (value.y & 0xF0)); }
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

    public bool Contains(Vector2Int pos)
    {
        if (_endPositions == null)
            return false;
        byte posByte = (byte)((pos.x & 0xF) | ((pos.y & 0xF) << 4));
        return _endPositions.Contains(posByte);
    }
}

public class Pawn : Piece
{
    public Pawn(Colour colour) : base(colour)
    {
        type = Type.PAWN;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
        Vector2Int dir = colour == Colour.WHITE ? Vector2Int.down : Vector2Int.up;

        //Check if pawn can move 2 squares forward
        if (position.y == 6)
        {
            AddMoveIfValid(position + dir * 2, ref moves);
        }

        //Check if pawn can move 1 square forward
        AddMoveIfValid(position + dir, ref moves);

        //Check if pawn can go left
        AddMoveIfEnemy(position + dir + Vector2Int.left, ref moves, enemyColour);

        //Check if pawn can go right
        AddMoveIfEnemy(position + dir + Vector2Int.right, ref moves, enemyColour);

        return moves;
    }
}

public class Bishop : Piece
{
    public Bishop(Colour colour) : base(colour)
    {
        type = Type.BISHOP;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
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
                    break;
                }

                moves.Add(newPos);
            }
        }
        return moves;
    }
}

public class Knight : Piece
{
    public Knight(Colour colour) : base(colour)
    {
        type = Type.KNIGHT;
        this.colour = colour;
    }
    //TODO: Algorithm to enter moves not manual
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
        bool add = AddMoveIfValid(position + Vector2Int.down * 2 + Vector2Int.left, ref moves) || AddMoveIfEnemy(position + Vector2Int.down * 2 + Vector2Int.left, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down * 2 + Vector2Int.right, ref moves) || AddMoveIfEnemy(position + Vector2Int.down * 2 + Vector2Int.right, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up * 2 + Vector2Int.left, ref moves) || AddMoveIfEnemy(position + Vector2Int.up * 2 + Vector2Int.left, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up * 2 + Vector2Int.right, ref moves) || AddMoveIfEnemy(position + Vector2Int.up * 2 + Vector2Int.right, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down + Vector2Int.left * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.down + Vector2Int.left * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up + Vector2Int.left * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.up + Vector2Int.left * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.down + Vector2Int.right * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.down + Vector2Int.right * 2, ref moves, enemyColour);
        add = AddMoveIfValid(position + Vector2Int.up + Vector2Int.right * 2, ref moves) || AddMoveIfEnemy(position + Vector2Int.up + Vector2Int.right * 2, ref moves, enemyColour);
        return moves;
    }
}

public class Rook : Piece
{
    public Rook(Colour colour) : base(colour)
    {
        type = Type.ROOK;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
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
                    break;
                }

                moves.Add(newPos);
            }
        }

        return moves;
    }
}

public class Queen : Piece
{
    public Queen(Colour colour) : base(colour)
    {
        type = Type.QUEEN;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
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
                    break;
                }

                moves.Add(newPos);
            }
        }
        return moves;
    }
}

public class King : Piece
{
    public King(Colour colour) : base(colour)
    {
        type = Type.KING;
        this.colour = colour;
    }
    public override Moves GetMoves(Vector2Int position)
    {
        Moves moves = new Moves();
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
        return moves;
    }
}
