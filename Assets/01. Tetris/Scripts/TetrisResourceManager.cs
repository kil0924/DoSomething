using System.Collections.Generic;
using Core;
using Core.Singleton;
using UnityEngine;

public class TetrisResourceManager : ResourceManagerBase<TetrisResourceManager>
{
    #region ========== Cell ==========

    private List<TetrisCell> unUsedCells = new List<TetrisCell>();
    public TetrisCell GetUnusedCell()
    {
        TetrisCell cell = null;
        if (unUsedCells.Count > 0)
        {
            cell = unUsedCells[0];
            unUsedCells.RemoveAt(0);
        }
        else
        {
            var go = GetResource("Tetris/Cell");
            if (go != null)
            {
                cell = go.GetComponent<TetrisCell>();
            }
        }

        if (cell != null)
        {
            cell.gameObject.SetActive(true);
            return cell;
        }
        return null;
    }
    public void ReturnCell(TetrisCell cell)
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
