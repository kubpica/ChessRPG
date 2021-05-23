using ChessRPG;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Columns : MonoBehaviourSingleton<Columns>
{
    private List<GameObject> _columns;
    /// <summary>
    /// If true the <see cref="_columns"/> may be not up to date.
    /// </summary>
    private bool isDirty = true;

    /// <summary>
    /// Access list of tracked columns.
    /// </summary>
    /// <remarks>
    /// It's safer to use <see cref="GetAll()"/> as it makes sure deleted columns are not included, this one may include nulls. 
    /// </remarks>
    public List<GameObject> ColumnsList
    {
        get
        {
            if(isDirty)
            {
                updateCoinsList();
            }

            return _columns;
        }
    }

    /// <summary>
    /// You should call this function, every time you <see cref="Player.AddCoin(char, char, Vector3, Vector3)">add a new coin</see>.
    /// </summary>
    public void MarkAsDirty()
    {
        isDirty = true;
    }

    private void updateCoinsList()
    {
        if (_columns == null)
            _columns = new List<GameObject>();
        else
            _columns.Clear();

        foreach(var c in GetComponentsInChildren<Column>())
        {

        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var team = transform.GetChild(i);
            for (int j = 0; j < team.transform.childCount; j++)
            {
                _columns.Add(team.transform.GetChild(j).gameObject);
            }
        }
        isDirty = false;
    }

    public Transform GetNearest(Vector3 point, GameObject coinsHolder)
    {
        float minDistance = float.MaxValue;
        Transform nearestCoin = null;

        for (int i = 0; i < coinsHolder.transform.childCount; i++)
        {
            var coin = coinsHolder.transform.GetChild(i);
            var distance = Vector3.Distance(point, coin.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCoin = coin;
            }
        }

        return nearestCoin;
    }

    public Transform GetNearest(Vector3 point)
    {
        return GetNearest(point, GetAll());
    }

    public Transform GetNearestExcept(Vector3 point, GameObject except)
    {
        return GetNearest(point, GetAll(), except);
    }

    /// <summary>
    /// Return coin nearest to the specified point.
    /// </summary>
    /// <param name="point"> The point.</param>
    /// <param name="coins"> Checked coins.</param>
    /// <param name="except"> Parent of coins we want to ignore.</param>
    /// <returns> Transform of the nearest coin.</returns>
    public Transform GetNearest(Vector3 point, IEnumerable<Transform> coins, GameObject except = null)
    {
        float minDistance = float.MaxValue;
        Transform nearestCoin = null;

        foreach (var coin in coins)
        {
            if (coin.parent.gameObject == except)
                continue;

            var distance = Vector3.Distance(point, coin.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCoin = coin;
            }
        }

        return nearestCoin;
    }
   
    /// <summary>
    /// Returns tranforms of coins exisiting in the scene.
    /// </summary>
    /// <returns> Enumerable coins' Transforms.</returns>
    public IEnumerable<Transform> GetAll()
    {
        return ColumnsList
            .Where(rb => rb != null)
            .Select(rb => rb.transform);
    }

    /// <summary>
    /// Returns tranforms of coins exisiting in the scene except of one team.
    /// </summary>
    /// <param name="except"> Parent of coins we don't want to include.</param>
    /// <returns> Enumerable coins' Transforms.</returns>
    public IEnumerable<Transform> GetAllExcept(GameObject except)
    {
        return ColumnsList
            .Where(rb => rb != null && rb.transform.parent.gameObject != except)
            .Select(rb => rb.transform);
    }

    /// <summary>
    /// Returns tranforms of coins exisiting in the scene.
    /// </summary>
    /// <param name="except"> Parent of coins we don't want to include.</param>
    /// <returns> Newly created list of the coins' Transforms.</returns>
    public List<Transform> GetNewList(GameObject except = null)
    {
        var list = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var team = transform.GetChild(i);
            if (team.gameObject == except)
                continue;

            for (int j = 0; j < team.childCount; j++)
            {
                list.Add(team.GetChild(j));
            }
        }
        return list;
    }
}
