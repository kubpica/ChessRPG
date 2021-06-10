using ChessRPG;

public class Potion : Piece
{
    public override string Mianownik => "Mikstura";
    public override string Biernik => "miksturę";

    public override bool CanBeTaken => true;
}
