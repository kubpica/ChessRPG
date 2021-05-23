﻿using System.Linq;
using UnityEngine;

namespace ChessRPG
{
    public class PiecesSpawner : MonoBehaviourSingleton<PiecesSpawner>
    {
        public GameObject whiteSoldier;
        public GameObject whiteOfficer;
        public GameObject blackSoldier;
        public GameObject blackOfficer;

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
                case 'w':
                    piece = Instantiate(whiteSoldier).GetComponent<Piece>();
                    //piece.transform.eulerAngles = new Vector3(0, 180, 0);
                    break;
                case 'W':
                    piece = Instantiate(whiteOfficer).GetComponent<Piece>();
                    break;
                case 'b':
                    piece = Instantiate(blackSoldier).GetComponent<Piece>();
                    break;
                case 'B':
                    piece = Instantiate(blackOfficer).GetComponent<Piece>();
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