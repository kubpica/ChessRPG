using ChessRPG;

public class Sword : Piece
{
    public override string Mianownik => "Miecz";
    public override string Biernik => "miecz";

    public override bool CanBeTaken => true;
}
