
public abstract class Piece
{
    public enum Type { PAWN }
    public enum Colour { WHITE, BLACK }
    
    public Type type;
    public Colour colour;
    public int x;
    public int y;
    
    public Piece(Type type, Colour colour, int x, int y)
    {
        this.type = type;
        this.colour = colour;
        this.x = x;
        this.y = y;
    }
    
    public abstract Moves GenerateMoves();
    
    public abstract char GetPieceChar();
}
