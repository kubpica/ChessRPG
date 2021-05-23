using System.Collections.Generic;
using UnityEngine;

namespace ChessRPG
{
    public class Column : MonoBehaviour
    {
        public static Column CreateColumn(Piece commander)
        {
            var go = new GameObject("Column");
            go.transform.position = commander.transform.position; 
            go.transform.parent = commander.transform.parent;
            commander.transform.parent = go.transform;
            
            var column = go.AddComponent<Column>();
            column.Init(commander);

            return column;
        }

        private LinkedList<Piece> _pieces;
        public LinkedList<Piece> Pieces => _pieces;

        private Square _square;
        public Square Square 
        {
            get => _square;
            
            set
            {
                if (_square == value)
                    return;

                // Empty the old square
                if(_square != null && _square.Column == this)
                    _square.Clear();

                // Place the piece on the new square
                _square = value;
                if(_square != null)
                    _square.PlaceColumn(this);
            }
        }
        public Piece Commander => _pieces.First.Value;
        public Piece BottomPiece => _pieces.Last.Value;
        public string Position => Commander.Position;

        /// <summary>
        /// Sets <see cref="_square"/> of the column without notyfing the <see cref="Square"/> object about the change,
        /// so that officaly there can be another <see cref="Square.Column">column</see> on the square.
        /// </summary>
        /// <remarks>
        /// You should use <see cref="Square"/> setter instead, unless you now what you are doing.
        /// </remarks>
        public void SetSquareSilently(Square square)
        {
            _square = square;
        }

        public void Init(Piece commander)
        {
            if (_pieces != null)
                Debug.LogError("This column was inited already.");

            _pieces = new LinkedList<Piece>();
            _pieces.AddFirst(commander);
        }

        public void UnmarkDarkAll()
        {
            foreach (var p in _pieces)
                p.UnmarkDark();
        }

        public void AddOnTop(Piece piece)
        {
            _pieces.AddFirst(piece);
            piece.Column = this;
            piece.transform.parent = transform;
        }

        public void AddOnTop(Column column)
        {
            var pieces = column.Pieces;
            while (pieces.Count > 0)
            {
                AddOnTop(column.ReleaseBottom());
            }
        }

        public void AddToBottom(Column column)
        {
            var pieces = column.Pieces;
            while (pieces.Count > 0)
            {
                Take(column);
            }
        }

        /// <summary>
        /// Takes commander of the specified column as prisoner of this column.
        /// </summary>
        /// <param name="column"> Column of which commander to take.</param>
        public void Take(Column column)
        {
            var piece = column.ReleaseTop();
            Take(piece);
        }

        public void Take(Piece piece)
        {
            if (piece.HasColumn)
            {
                Take(piece.Column);
                return;
            }

            _pieces.AddLast(piece);

            piece.transform.parent = transform;
            piece.Column = this;
            updatePosition();
        }

        public Column Split(Piece until, bool fromBottom)
        {
            return fromBottom ? SplitFromBottom(until) : SplitFromTop(until);
        }

        public Column SplitFromTop(Piece until)
        {
            if (BottomPiece == until)
                return this;

            var column = ReleaseTop().Column;
            while(column.BottomPiece != until && _pieces.Count > 0)
            {
                column.Take(this);
            }
            return column;
        }

        public Column SplitFromBottom(Piece until)
        {
            if (Commander == until)
                return this;

            var column = ReleaseBottom().Column;
            while (column.Commander != until && _pieces.Count > 0)
            {
                var p = ReleaseBottom();
                column.AddOnTop(p);
            }
            return column;
        }

        /// <summary>
        /// Removes current commander so that the next piece on the stack becomes one.
        /// </summary>
        /// <returns> Released commander - a piece on the top.</returns>
        public Piece ReleaseTop()
        {
            var ex = _pieces.First.Value;
            _pieces.RemoveFirst();

            return release(ex);
        }

        private void updatePosition()
        {
            if (_pieces.Count == 0)
                return;

            var childern = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                childern.Add(c);
            }

            transform.DetachChildren();
            transform.position = _pieces.Last.Value.transform.position;
        
            foreach(var c in childern)
            {
                c.transform.parent = transform;
            }
        }

        public Piece ReleaseBottom()
        {
            var ex = _pieces.Last.Value;
            _pieces.RemoveLast();

            updatePosition();

            return release(ex);
        }

        private Piece release(Piece ex)
        {
            ex.transform.parent = transform.parent;
            ex.Column = null;

            if (_pieces.Count == 0)
            {
                if(_square.Column == this)
                    _square.Clear();
                Destroy(this.gameObject);
            }

            return ex;
        }
    }
}