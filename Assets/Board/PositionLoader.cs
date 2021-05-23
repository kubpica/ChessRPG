using UnityEngine;

/// <summary>
/// Loads the <see cref="rpgFEN"/> postion.
/// </summary>
public class PositionLoader : MonoBehaviourExtended
{
    /// <summary>
    /// Position to load - specified in the rpgFEN format. 
    /// </summary>
    public string rpgFEN;

    // Start is called before the first frame update
    private void Start()
    {
        rpgFEN = rpgFEN.Trim().Replace("\n", "").Replace(" ", "");
        Load();
    }

    private Vector3 getWorldPosition(int rank, int file)
    {
        return new Vector3(30 - file * 4, 2, 30 - rank * 4);
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(rpgFEN))
            return;

        var rank = 14; // 14 => position.z=-26; 0 => z=30; position.z=30-rank*4;
        var file = 1; // 1 => position.x=26; 0 => x=30; position.x=30-file*4

        // Read & deserialize level data segments
        var dataSegs = rpgFEN.Split('|');

        // 1st seg = Level/Board Id
        Debug.Log("Loading postion on board " + dataSegs[0]);
        //REMARK: There is only one board currently in the game, but in future it can be used to identify different arenas. 

        // 2nd seg = Piece placement
        var rankDsc = dataSegs[1].Split('/'); // '/' moves to the next rank
        foreach (var r in rankDsc)
        {
            string[] fileDsc;

            // If the rank description starts with '\' then the amount of subsequent '\' indicates how many ranks to go back (upwards)
            if (r.StartsWith("\\"))
            {
                int goBack = 1;
                while (r[goBack] == '\\')
                    goBack++;
                rank += goBack;
                fileDsc = r.Replace("\\", "").Split(',');
            }
            else
            {
                fileDsc = r.Split(',');
            }

            foreach(var f in fileDsc)
            {
                // if there is + instead of , then that means there is one more piece on the same square
                // (both , and + indicate next piece but , moves to the next file while + does not)
                var pieces = f.Split('+');
                foreach(var p in pieces)
                {
                    var pieceData = p.Split(';');

                    if (int.TryParse(pieceData[0], out int emptySquares)) // if starts with a number then it indicates number of empty squares
                    {
                        // (negative values move "back" - to the left)
                        file += emptySquares-1;
                    }
                    else
                    {
                        var pieceId = pieceData[0]; // Piece Id
                        var notes = pieceData.Length > 1 ? pieceData[1] : ""; // Notes applied 

                        //TODO add piece to column and spawn the column
                    }
                }

                file++;
            }

            // Move to the next rank (downwards)
            rank--;
            file = 1;
        }
    }
}
