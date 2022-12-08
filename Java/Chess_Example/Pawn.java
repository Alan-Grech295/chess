
/**
 * Write a description of class Pawn here.
 *
 * @author (your name)
 * @version (a version number or a date)
 */
public class Pawn extends Piece
{
    public Pawn(Colour colour, int x, int y)
    {
        super(Type.PAWN, colour, x, y);
        
    }
    
    public Moves GenerateMoves()
    {
        Moves moves = new Moves();
        moves.startX = x;
        moves.startY = y;
        moves.moves.add(new Move(x, y - 1));
        return moves;
    }
    
    public char GetPieceChar()
    {
        if(colour == Piece.Colour.WHITE)
            return 'P';
        return 'p';
    }
}
