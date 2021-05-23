using ChessRPG;

public class Soldier : Piece
{
    public override string Mianownik => "Rzymianin";
    public override string Biernik => "rzymianina";

    public override bool CanBeTaken => true;
}
