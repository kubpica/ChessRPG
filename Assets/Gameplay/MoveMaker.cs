using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChessRPG
{
    public class MoveMaker : MonoBehaviourSingleton<MoveMaker>
    {
        [GlobalComponent] private Board board;
        [GlobalComponent] private PiecesSpawner spawner;
        private const float PIECE_HEIGHT = 0.5f;

        public StringEvent onMoveStarted;
        public UnityEvent onMoveEnded;

        public Camera cam;
        
        private Column _selectedColumn;
        private Column SelectedColumn 
        {
            get => _selectedColumn;

            set
            {
                if(_selectedColumn != value)
                {
                    if (_selectedColumn != null && splitPiece != null)
                        _selectedColumn.UnmarkDarkAll();

                    _selectedColumn = value;
                    splitPiece = null;
                }
            }
        }

        private string displayedMsg;
        private GUIStyle guiStyle = new GUIStyle();
        private bool inputLocked;

        private LinkedListNode<Piece> splitPiece;
        private bool splitFromBottom;

        private string pieceToSpawn = "g";
        
        private void Start()
        {
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.normal.textColor = Color.green;

            if (cam == null)
                cam = Camera.main;
        }

        private bool SelectColumn(string square)
        {
            var column = board.GetColumnAt(square);
            if(column == null)
            {
                //Debug.Log("Tried to select empty square.");
                return false;
            }

            return SelectColumn(column);
        }

        private bool SelectColumn(Square square)
        {
            if (square.Column == null)
                return false;

            return SelectColumn(square.Column);
        }

        /// <summary>
        /// Selects column to be moved.
        /// </summary>
        /// <param name="column"> Column to move/select.</param>
        /// <returns> True, if the player can move the piece; otherwise false.</returns>
        private bool SelectColumn(Column column)
        {
            if (_selectedColumn != null)
                board.UnmarkAll();

            SelectedColumn = column;
            splitPiece = null;
            board.MarkSquare(column.Square, Color.yellow);

            return true;
        }

        /// <summary>
        /// Moves the selected column to the specified <c>square</c>, if that move is legal. 
        /// </summary>
        /// <param name="square"> Target square.</param>
        /// <returns> True if the move can be made, false if the move is illegal.</returns>
        private bool SelectMove(string square)
        {
            if (SelectedColumn == null)
            {
                Debug.Log("Column to move not selected yet!");
                return false;
            }

            board.UnmarkAll();

            string move = _selectedColumn.Position + "-" + square;

            MakeMove(move);
            return true;
        }

        private void MakeMove(string move)
        {
            onMoveStarted.Invoke(move);
            inputLocked = true;

            var squares = move.Split('-');
            if (squares.Length == 3)
            {
                // Take
                var takenColumn = board.GetColumnAt(squares[1]);
                var targetSquare = board.GetSquareAt(squares[2]);

                var killer = _selectedColumn.Commander;
                var victim = takenColumn.Commander;
                displayedMsg = killer.Mianownik + " z " + killer.Position
                    + " bierze do niewoli " + victim.Biernik + " z " + victim.Position
                    + " na " + targetSquare.coordinate + "\n";
                StartCoroutine(animateJumpTake(takenColumn, targetSquare, new List<Piece> { victim }));
            }
            else
            {
                // Move
                var targetSquare = board.GetSquareAt(squares[1]);

                var piece = _selectedColumn.Commander;
                displayedMsg = piece.Mianownik + " przeskakuje z " + piece.Position + " na " + targetSquare.coordinate + "\n";
                StartCoroutine(animateMove(targetSquare));
            }
        }

        private IEnumerator animateJumpTake(Column takenColumn, Square targetSquare, List<Piece> takenPieces)
        {
            // Jump
            yield return jump(targetSquare, 1.5f + 0.5f * takenColumn.Pieces.Count);

            // Animate the takes 
            yield return animateTake(takenPieces);
        }

        private IEnumerator animateTake(List<Piece> takenPieces)
        {
            // Darken taken pieces
            foreach (var p in takenPieces)
            {
                p.MarkDark();
            }

            // Animate the takes 
            yield return takeAnimation(takenPieces);

            // Perform takes on the logic level and end the move
            foreach (var p in takenPieces)
            {
                _selectedColumn.Take(p.Column);
            }

            endMove();
        }

        private IEnumerator animateTake(Column takenColumn, Column targetColumn)
        {
            // Animate the takes 
            yield return takeAnimation(takenColumn, targetColumn);

            // Perform takes on the logic level and end the move
            targetColumn.AddToBottom(takenColumn);

            endMove();
        }

        private IEnumerator animateMove(Square targetSquare)
        {
            // Jump
            yield return jump(targetSquare, 2);

            endMove();
        }

        private void endMove()
        {
            SelectedColumn = null;
            inputLocked = false;
            onMoveEnded.Invoke();
        }

        private IEnumerator takeAnimation(Column takenColumn, Column targetColumn)
        {
            float height = takenColumn.Pieces.Count * PIECE_HEIGHT;

            // Lift the target column
            var p = targetColumn.transform.position;
            p.y += height;
            StartCoroutine(move(targetColumn.gameObject, p, 0.3f));

            // Move taken column under the killer
            yield return move(takenColumn.gameObject, p.Y(0));

            // Remove "darken" effect from the taken pieces
            foreach (var piece in takenColumn.Pieces)
            {
                piece.UnmarkDark();
            }
        }

        private IEnumerator takeAnimation(List<Piece> takenPieces)
        {
            float height = takenPieces.Count * PIECE_HEIGHT;

            // Lift taken pieces (remove commanders from columns)
            foreach (var piece in takenPieces)
            {
                StartCoroutine(move(piece.gameObject, piece.transform.position.Y(piece.transform.position.y + PIECE_HEIGHT)));
            }

            // Lift the "killer"
            var p = _selectedColumn.transform.position;
            p.y += height;
            yield return move(_selectedColumn.gameObject, p);

            // Move taken pieces under the killer
            for(int i = 0; i<takenPieces.Count-1; i++)
            {
                var piece = takenPieces[i];
                p = _selectedColumn.transform.position;
                p.y -= (i+1)*PIECE_HEIGHT;
                StartCoroutine(move(piece.gameObject, p, 0.5f + i*0.2f));
            }

            p = _selectedColumn.transform.position;
            p.y = 0; //-= takenPieces.Count * pieceHeight;
            var j = takenPieces.Count - 1;
            yield return move(takenPieces[j].gameObject, p, 0.5f + j * 0.2f);

            // Remove "darken" effect from the taken pieces
            foreach(var piece in takenPieces)
            {
                piece.UnmarkDark();
            }
        }

        /// <summary>
        /// Lineary moves <c>go</c> to <c>target</c> position.
        /// </summary>
        private IEnumerator move(GameObject go, Vector3 target, float time = 0.5f)
        {
            Vector3 basePos = go.transform.position;
            var offset = target - basePos;
            
            for (float passed = 0.0f; passed < time;)
            {
                passed += Time.deltaTime;
                float f = passed / time;
                if (f > 1) f = 1;

                var p = basePos + offset * f;
                go.transform.position = p;

                yield return 0;
            }
        }

        private IEnumerator jump(Square targetSquare, float height)
        {
            yield return jump(_selectedColumn.gameObject, targetSquare.transform.position.Y(0), height);

            Debug.Log("Jumped from " + _selectedColumn.Square.coordinate + " to " + targetSquare.coordinate);
            _selectedColumn.Square = targetSquare;
        }

        private IEnumerator animateMergeJump(Column targetColumn)
        {
            split();
            board.UnmarkAll();
            displayedMsg = _selectedColumn.Commander.Mianownik + " z " + _selectedColumn.Position 
                + " wskakuje na " + targetColumn.Commander.Biernik + " na " + targetColumn.Position + "\n";
            onMoveStarted.Invoke(_selectedColumn.Position + "-" + targetColumn.Position);
            inputLocked = true;

            var y = targetColumn.Pieces.Count * PIECE_HEIGHT;
            yield return jump(_selectedColumn.gameObject, targetColumn.transform.position.Y(y), y + 2);

            if (splitPiece != null)
            {
                foreach (var piece in _selectedColumn.Pieces)
                {
                    piece.UnmarkDark();
                }
            }

            Debug.Log("Jumped from " + _selectedColumn.Square.coordinate + " to " + targetColumn.Position);
            targetColumn.AddOnTop(_selectedColumn);

            endMove();
        }

        private IEnumerator mergeWithSinglePiece(Column column, Piece piece)
        {
            yield return move(column.gameObject, piece.transform.position.Y(PIECE_HEIGHT));
            column.Take(piece);
        }

        /// <summary>
        /// Animates jump anlong parabola.
        /// </summary>
        private IEnumerator jump(GameObject go, Vector3 target, float height, float time = 0.5f)
        {
            Vector3 basePos = go.transform.position;
            var direction = target - basePos;
            float distance = direction.magnitude;
            direction = direction.normalized;
            
            float x1 = 0;
            float y1 = 0;
            float x2 = distance / 2.0f;
            float y2 = height;
            float x3 = distance;
            float y3 = target.y-basePos.y;

            float denom = (x1 - x2) * (x1 - x3) * (x2 - x3);
            float A = (x3 * (y2 - y1) + x2 * (y1 - y3) + x1 * (y3 - y2)) / denom;
            float B = (x3 * x3 * (y1 - y2) + x2 * x2 * (y3 - y1) + x1 * x1 * (y2 - y3)) / denom;
            float C = (x2 * x3 * (x2 - x3) * y1 + x3 * x1 * (x3 - x1) * y2 + x1 * x2 * (x1 - x2) * y3) / denom;


            for (float passed = 0.0f; passed < time;)
            {
                passed += Time.deltaTime;
                float f = passed / time;
                if (f > 1) f = 1;

                float x = distance * f;
                float y = A * x * x + B * x + C;

                var p = basePos + direction * x;
                p.y = basePos.y + y;
                go.transform.position = p;

                yield return 0;
            }
        }

        private void split()
        {
            if (splitPiece == null)
            {
                splitFromBottom = false;
                var piece = _selectedColumn.Pieces.First;
                while (piece.Next != null && piece.Next.Value.CanBeTaken)
                    piece = piece.Next;

                if (piece != _selectedColumn.Pieces.Last)
                    splitPiece = piece;
                else
                    return;
            }

            var oldColumn = _selectedColumn;
            _selectedColumn = oldColumn.Split(splitPiece.Value, splitFromBottom);
            _selectedColumn.SetSquareSilently(oldColumn.Square);

            if (splitFromBottom)
                StartCoroutine(move(oldColumn.gameObject, oldColumn.transform.position.Y(0)));
        }

        private void squareClicked(Square square, int button)
        {
            if (square.Column != null)
            {
                columnClicked(square.Column, button);
                return;
            }

            // Empty square clicked
            switch (button)
            {
                case 0: // LMB
                    if(_selectedColumn != null)
                    {
                        split();
                        SelectMove(square.coordinate);
                    }

                    break;
                case 1: // RMB
                    break;
                case 2: // Scroll (MMB)
                    if(_selectedColumn != null)
                    {
                        if(splitPiece == null)
                        {
                            // Move column but not the bottom piece
                            if (_selectedColumn.Pieces.Count > 1)
                            {
                                var piece = _selectedColumn.Pieces.Last;
                                do
                                {
                                    piece = piece.Previous;
                                }
                                while (piece.Previous != null && piece.Value.CanBeTaken);

                                if(piece == _selectedColumn.Pieces.First)
                                {
                                    var bottomPiece = _selectedColumn.ReleaseBottom();
                                    bottomPiece.Column.Square = _selectedColumn.Square;
                                }
                                else
                                {
                                    splitPiece = piece;
                                    var oldColumn = _selectedColumn;
                                    splitFromBottom = true;
                                    split();

                                    var bottomPiece = _selectedColumn.ReleaseBottom();
                                    StartCoroutine(mergeWithSinglePiece(oldColumn, bottomPiece));
                                }
                            }
                            SelectMove(square.coordinate);
                        }
                        else
                        {
                            deselectPieces();
                        }
                    }
                    else
                    {
                        // Spawn piece
                        spawner.SpawnColumn(pieceToSpawn, square);
                    }
                    break;
                case 3: // 4th mouse button
                    if (_selectedColumn != null)
                    {
                        // Move only the bottom piece of the selected column
                        if (_selectedColumn.Pieces.Count > 1)
                        {
                            selectBottomPiece();
                            split();
                        }
                        SelectMove(square.coordinate);
                    }
                    break;
                case 4: // 5th mouse button
                    if (_selectedColumn != null)
                    {
                        // Move only commander of the slected column
                        if (_selectedColumn.Pieces.Count > 1)
                        {
                            selectTopPiece();
                            split();
                        }
                        SelectMove(square.coordinate);
                    }
                    break;
            }
        }

        private void columnClicked(Column column, int button)
        {
            switch (button)
            {
                case 0: // LMB
                    // Deselect
                    if(SelectedColumn == column)
                    {
                        board.UnmarkAll();
                        SelectedColumn = null;
                        return;
                    }

                    if(SelectedColumn == null)
                    {
                        SelectColumn(column);
                    }
                    else
                    {
                        // Animate merge
                        StartCoroutine(animateMergeJump(column));
                    }

                    break;
                case 1: // RMB
                    break;
                case 2: // Scroll (MMB)
                    if(_selectedColumn != null)
                    {
                        if(splitPiece == null)
                        {
                            if (_selectedColumn == column && column.Pieces.Count <= 1)
                                return;

                            board.UnmarkAll();
                            inputLocked = true;
                            onMoveStarted.Invoke(_selectedColumn.Position + "-" + column.Position + "-" + _selectedColumn.Position);
                            displayedMsg = _selectedColumn.Commander.Mianownik + " z " + _selectedColumn.Position + " pochłania " + column.Commander.Biernik + " z " + column.Position + "\n"; 
                            StartCoroutine(animateTake(new List<Piece> { column.Commander }));
                        }
                        else
                        {
                            // Take selected pieces under the clicked column
                            if(_selectedColumn == column && (splitFromBottom || splitPiece.Value == _selectedColumn.BottomPiece))
                                return;

                            split();
                            board.UnmarkAll();
                            inputLocked = true;
                            onMoveStarted.Invoke(column.Position + "-" + _selectedColumn.Position + "-" + column.Position);
                            displayedMsg = column.Commander.Mianownik + " z " + column.Position + " pochłania " + _selectedColumn.Commander.Biernik + " z " + _selectedColumn.Position + "\n";
                            StartCoroutine(animateTake(_selectedColumn, column));
                        }
                    }
                    else
                    {
                        // Delete clicked column
                        Destroy(column.gameObject);
                    }
                    break;
                case 3: // 4th mouse button
                    if (SelectedColumn == null)
                        if (!SelectColumn(column))
                            return;

                    selectBottomPiece();
                    
                    if (_selectedColumn != column)
                        StartCoroutine(animateMergeJump(column));

                    break;
                case 4: // 5th mouse button
                    if (SelectedColumn == null)
                        if (!SelectColumn(column))
                            return;

                    selectTopPiece();

                    if(_selectedColumn != column)
                        StartCoroutine(animateMergeJump(column));

                    break;
            }
        }

        /// <summary>
        /// Select pieces to split from top
        /// </summary>
        private void selectTopPiece()
        {
            if (splitPiece == null || splitFromBottom)
            {
                _selectedColumn.UnmarkDarkAll();
                splitFromBottom = false;
                splitPiece = _selectedColumn.Pieces.First;
                splitPiece.Value.MarkDark();
            }
            else
            {
                var next = splitPiece.Next;
                if (next != null)
                {
                    splitPiece = next;
                    splitPiece.Value.MarkDark();
                }
            }
        }

        /// <summary>
        /// Select pieces to split from bottom
        /// </summary>
        private void selectBottomPiece()
        {
            if (splitPiece == null || !splitFromBottom)
            {
                _selectedColumn.UnmarkDarkAll();
                splitFromBottom = true;
                splitPiece = _selectedColumn.Pieces.Last;
                splitPiece.Value.MarkDark();
            }
            else
            {
                var next = splitPiece.Previous;
                if (next != null)
                {
                    splitPiece = next;
                    splitPiece.Value.MarkDark();
                }
            }
        }

        /// <summary>
        /// Deslect marked pieces
        /// </summary>
        private void deselectPieces()
        {
            _selectedColumn.UnmarkDarkAll();
            splitPiece = null;
        }
        
        private void pieceClicked(Piece piece, int button)
        {
            columnClicked(piece.Column, button);
        }

        public static void DrawOutline(Rect pos, string text, GUIStyle style, Color outColor, Color inColor)
        {
            GUIStyle backupStyle = style;
            style.normal.textColor = outColor;
            pos.x--;
            GUI.Label(pos, text, style);
            pos.x += 2;
            GUI.Label(pos, text, style);
            pos.x--;
            pos.y--;
            GUI.Label(pos, text, style);
            pos.y += 2;
            GUI.Label(pos, text, style);
            pos.y--;
            style.normal.textColor = inColor;
            GUI.Label(pos, text, style);
            style = backupStyle;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), "ChessRPG", guiStyle);

            string msg = "";
            if (displayedMsg != null)
                msg += displayedMsg;

            msg += "\n WASD(QERF) + Mouse2 = Poruszanie kamerą\n Shift = Przyśpieszenie poruszania\n Spacja = Obrót wokół wskazanej monety\n";

            if (_selectedColumn == null)
            {
                msg += "\n Bierka do postawienia: " + pieceToSpawn + "\n 1 = Zielony gracz (g)\n 2 = Cyanowy gracz (c)\n 3 = Żółty gracz (y)\n 4 = Czerwony gracz (r)\n 5 = Przyjazny NPC (G)\n 6 = Wrogi NPC (R)\n 7 = Budynek (B)\n 8 = Skrzynia (h)\n 9 = Miecz (s)\n 0 = Mikstura (p)\n";
                msg += " Mouse1 = Zaznacz kolumnę\n Mouse3 = Postaw bierkę lub usuń kolumnę\n Mouse4 = Zaznacz ostatnią bierkę w kolumnie\n Mouse5 = Zaznacz pierwszą bierkę w kolumnie\n";
            }
            else
            {
                msg += "\n Mouse1 = Przenieś zaznaczoną kolumnę\n Mouse4 = Przenieś tylko ostatnią bierkę w kolumnie\n Mouse5 = Przenieś tylko pierwszą bierkę w kolumnie\n";
                if (splitPiece == null)
                    msg += " Mouse3+PustePole = Przenieś całą kolumne oprócz ostatniej bierki\n Mouse3+Kolumna = Pochłoń pierwszą bierkę z innej kolumny\n";
                else
                    msg += " Mouse3 = Przenieś zaznaczone bierki pod wybraną kolumnę\n";
            }
            
            DrawOutline(new Rect(10, 30, 1900, 1000), msg, guiStyle, Color.black, guiStyle.normal.textColor);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                pieceToSpawn = "g";
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                pieceToSpawn = "c";
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                pieceToSpawn = "y";
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                pieceToSpawn = "r";
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                pieceToSpawn = "G";
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                pieceToSpawn = "R";
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                pieceToSpawn = "B";
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                pieceToSpawn = "h";
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                pieceToSpawn = "s";
            else if (Input.GetKeyDown(KeyCode.Alpha0))
                pieceToSpawn = "p";

            if (inputLocked)
                return;

            int button = -1;
            if (Input.GetKeyDown(KeyCode.Mouse0))
                button = 0;
            else if (Input.GetKeyDown(KeyCode.Mouse1))
                button = 1;
            else if (Input.GetKeyDown(KeyCode.Mouse2))
                button = 2;
            else if (Input.GetKeyDown(KeyCode.Mouse3))
                button = 3;
            else if (Input.GetKeyDown(KeyCode.Mouse4))
                button = 4;
            else
                return;

            if (button > -1)
            {
                var clicked = cam.GetColliderUnderMouse();
                if (clicked != null)
                    Debug.Log("clicked " + clicked.gameObject.name);

                if (clicked != null && clicked.transform.parent != null)
                {
                    if (clicked.gameObject.CompareTag("Board"))
                    {
                        var square = clicked.GetComponent<Square>();
                        if (square != null)
                            squareClicked(square, button);
                        else
                            Debug.LogError("Square component not found at " + clicked.gameObject.name);

                        Debug.Log("square clicked " + clicked);
                    }
                    else
                    {
                        var piece = clicked.GetComponent<Piece>();
                        if (piece != null)
                        {
                            pieceClicked(piece, button);
                        }

                        Debug.Log("clicked piece " + piece);
                    }
                }
            }
        }
    }
}

