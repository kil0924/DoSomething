using System.Collections.Generic;
using Core.Singleton;
using UnityEngine;

public class Match3ResourceManager : ResourceManagerBase<Match3ResourceManager>
{
    #region ========== Cell ==========

    private List<Match3Cell> unUsedCells = new List<Match3Cell>();
    public Match3Cell GetUnusedCell()
    {
        Match3Cell cell = null;
        if (unUsedCells.Count > 0)
        {
            cell = unUsedCells[0];
            unUsedCells.RemoveAt(0);
        }
        else
        {
            var go = GetResource("Match3/Cell");
            if (go != null)
            {
                cell = go.GetComponent<Match3Cell>();
            }
        }

        if (cell != null)
        {
            cell.gameObject.SetActive(true);
            return cell;
        }
        return null;
    }
    public void ReturnCell(Match3Cell cell)
    {
        cell.gameObject.SetActive(false);
        cell.transform.SetParent(transform);
#if UNITY_EDITOR
        cell.gameObject.name = "Unused Cell";
#endif
        unUsedCells.Add(cell);
    }

    #endregion
}
