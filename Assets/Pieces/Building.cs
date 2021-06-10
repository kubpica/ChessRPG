using ChessRPG;

public class Building : Piece
{
    public override string Mianownik => "Budynek";
    public override string Biernik => "budynek";

    public override bool CanBeTaken => false;
}
