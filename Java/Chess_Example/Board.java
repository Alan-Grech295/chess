
/**
 * Write a description of class Board here.
 *
 * @author (your name)
 * @version (a version number or a date)
 */
public class Board
{
    private static Piece[][] board;
    public static void main(String[] args)
    {
        board = new Piece[8][8];
        
        for(int i = 0; i < 8; i++)
        {
            board[i][6] = new Pawn(Piece.Colour.WHITE, 0, 6);
            board[i][1] = new Pawn(Piece.Colour.BLACK, 0, 6);
        }
        
        Display();
    }
    
    public static void Display()
    {
        for(int y = 0; y < 8; y++){
            String str = "";
            for(int x = 0; x < 8; x++){
                if(board[x][y] == null)
                    str += ((x + y) % 2 != 0 ? "." : "0") + "  ";
                else
                    str += board[x][y].GetPieceChar() + "  ";
            }
            System.out.println(str);
        }
    }
}
