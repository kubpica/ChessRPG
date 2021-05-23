using System.Collections.Generic;
using UnityEngine;

namespace ChessRPG
{
    public class Board : MonoBehaviourSingleton<Board>
    {
        /// <key> Square name/notation eg. d4 <see cref="Square.coordinate"/></key>
        /// <value> The <see cref="Square"/> object.</value>
        private Dictionary<string, Square> squares = new Dictionary<string, Square>();
        private HashSet<MeshRenderer> marked = new HashSet<MeshRenderer>();

        [GlobalComponent] private Columns columns;

        private void Start()
        {
            // Init squares
            for(int i = 0; i < transform.childCount; i++)
            {
                var fileParent = transform.GetChild(i);
                var fileId = fileParent.gameObject.name;

                for(int j = 0; j < fileParent.childCount; j++)
                {
                    var square = fileParent.GetChild(j).gameObject.AddComponent<Square>();
                    var id = fileId + j;
                    square.coordinate = id;
                    squares.Add(id, square);
                }
            }
        }

        public string GetSquareCoordinate(int fileId, int rankId)
        {
            char file = (char)('a' + fileId);
            rankId++;
            return file + "" + rankId;
        }

        public void GetSquareIds(string coordinate, out int fileId, out int rankId)
        {
            fileId = coordinate[0] - 'a';
            rankId = Helpers.stoi(coordinate[1].ToString()) - 1;
        }

        public Square GetSquareAt(string coordinate)
        {
            return squares[coordinate];
        }

        public Square GetSquareAt(int fileId, int rankId)
        {
            return squares[GetSquareCoordinate(fileId, rankId)];
        }

        public Column GetColumnAt(string coordinate) => GetSquareAt(coordinate).Column;

        /// <summary>
        /// Get column (of pieces) at specified square.
        /// </summary>
        /// <param name="fileId"> Start from 0. 'a' file = 0</param>
        /// <param name="rankId"> 1st rank = 0</param>
        /// <returns> A column or null.</returns>
        public Column GetColumnAt(int fileId, int rankId) => GetSquareAt(fileId, rankId).Column;

        public void MarkSquare(Square square, Color color)
        {
            var mr = square.GetComponent<MeshRenderer>();
            mr.material.color = color;
            marked.Add(mr);
        }

        public void UnmarkAll()
        {
            foreach(var mr in marked)
            {
                mr.material.color = Color.white;
            }

            marked.Clear();
        }

        /// <summary>
        /// Adds column to the game - registers it.
        /// </summary>
        /// <remarks>
        /// Every piece in the column should be newly created as the pieces will be added to the player's list of pieces.
        /// </remarks>
        /// <param name="column"> Column to register.</param>
        public void AddColumn(Column column, Square square)
        {
            // Place on the square
            column.Square = square;
            columns.MarkAsDirty();
        }
    }
}
