using ChessRPG;

public class Alien : Piece
{
    public override string Mianownik => "Obcy";
    public override string Biernik => "obcego";

    public override bool CanBeTaken => false;
}
