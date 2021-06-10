using System.Linq;
using UnityEngine;

namespace ChessRPG
{
    public class PiecesSpawner : MonoBehaviourSingleton<PiecesSpawner>
    {
        public GameObject greenSoldier;
        public GameObject redSoldier;
        public GameObject cyanSoldier;
        public GameObject yellowSoldier;
        public GameObject greenOfficer;
        public GameObject redOfficer;
        public GameObject building;
        public GameObject chest;
        public GameObject sword;
        public GameObject potion;

        [GlobalComponent] private Board board;

        private Column spawnColumn(string c)
        {
            var commander = SpawnPiece(c[0]);

            var column = commander.Column;
            if (c.Length > 1)
            {
                for (int i = 1; i < c.Length; i++)
                {
                    var p = SpawnPiece(c[i]);
                    column.Take(p);
                }
            }

            return column;
        }

        public Column SpawnColumn(string c, int fileId, int rankId)
        {
            var square = board.GetSquareAt(fileId, rankId);
            return SpawnColumn(c, square);
        }

        public Column SpawnColumn(string c, Square square)
        {
            var column = spawnColumn(c);

            Vector3 p = square.transform.position;
            p.y = 0;

            column.transform.position = p;
            foreach (var piece in column.Pieces.Reverse())
            {
                piece.transform.position = p;
                p.y += piece.transform.localScale.y;
            }

            board.AddColumn(column, square);

            return column;
        }

        public Piece SpawnPiece(char p)
        {
            Piece piece;
            switch (p)
            {
                case 'g':
                    piece = Instantiate(greenSoldier).GetComponent<Piece>();
                    //piece.transform.eulerAngles = new Vector3(0, 180, 0);
                    break;
                case 'G':
                    piece = Instantiate(greenOfficer).GetComponent<Piece>();
                    break;
                case 'r':
                    piece = Instantiate(redSoldier).GetComponent<Piece>();
                    break;
                case 'R':
                    piece = Instantiate(redOfficer).GetComponent<Piece>();
                    break;
                case 'c':
                    piece = Instantiate(cyanSoldier).GetComponent<Piece>();
                    break;
                case 'y':
                    piece = Instantiate(yellowSoldier).GetComponent<Piece>();
                    break;
                case 'B':
                    piece = Instantiate(building).GetComponent<Piece>();
                    break;
                case 'h':
                    piece = Instantiate(chest).GetComponent<Piece>();
                    break;
                case 's':
                    piece = Instantiate(sword).GetComponent<Piece>();
                    break;
                case 'p':
                    piece = Instantiate(potion).GetComponent<Piece>();
                    break;
                default:
                    Debug.LogError("Piece '" + p + "' not found.");
                    return null;
            }

            piece.transform.parent = transform;

            return piece;
        }
    }
}