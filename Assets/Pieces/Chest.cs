using ChessRPG;

public class Chest : Piece
{
    public override string Mianownik => "Skrzynia";
    public override string Biernik => "skrzynię";

    public override bool CanBeTaken => true;
}
