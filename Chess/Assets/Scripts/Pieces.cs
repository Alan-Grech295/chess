using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece
{
    public enum Colour { BLACK, WHITE}
    public enum Type { PAWN, BISHOP, KNIGHT, ROOK, QUEEN, KING }
    public Colour colour;
    public Type type;

    public abstract List<Move> GetMoves(Vector2Int position);

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

    public struct Move
    {
        public Vector2Int start;
        public Vector2Int end;
    }
}

public class Pawn : Piece
{
    public Pawn(Colour colour) {
        type = Type.PAWN;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}

public class Bishop : Piece
{
    public Bishop(Colour colour)
    {
        type = Type.BISHOP;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}

public class Knight : Piece
{
    public Knight(Colour colour)
    {
        type = Type.KNIGHT;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}

public class Rook : Piece
{
    public Rook(Colour colour)
    {
        type = Type.ROOK;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}

public class Queen : Piece
{
    public Queen(Colour colour)
    {
        type = Type.QUEEN;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}

public class King : Piece
{
    public King(Colour colour)
    {
        type = Type.KING;
        this.colour = colour;
    }
    public override List<Move> GetMoves(Vector2Int position)
    {
        return null;
    }
}
