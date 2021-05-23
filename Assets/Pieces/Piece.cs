using UnityEngine;

namespace ChessRPG
{
    public abstract class Piece : MonoBehaviour
    {
        private static Board _board;
        public static Board Board
        {
            get
            {
                if (_board == null)
                    _board = Board.Instance;

                return _board;
            }
        }

        private Column _column;
        public Column Column 
        { 
            get 
            {
                if (_column == null)
                    _column = Column.CreateColumn(this);

                return _column;
            }

            set => _column = value;
        }

        public bool HasColumn => _column != null;

        public Square Square => Column.Square;

        /// <summary>
        /// Name of the square at which is the piece.
        /// </summary>
        public string Position => Square.coordinate;

        public bool IsFree => !HasColumn || _column.Commander == this;

        public abstract string Mianownik { get; }
        public abstract string Biernik { get; }
        public abstract bool CanBeTaken { get; }

        private MeshRenderer meshRenderer;
        private Color materialColor;
        private Color materialColorDark;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();

            materialColor = meshRenderer.material.color;

            var dark = new HSBColor(materialColor);
            dark.b = 0.6f;
            materialColorDark = dark.ToColor();
        }

        public void MarkDark()
        {
            meshRenderer.material.color = materialColorDark;
        }

        public void UnmarkDark()
        {
            meshRenderer.material.color = materialColor;
        }
    }
}