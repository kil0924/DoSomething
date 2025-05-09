using System.Collections.Generic;
using UnityEngine;

public class AppleResourceManager : ResourceManagerBase<AppleResourceManager>
{
    #region ========== Cell ==========

    private List<AppleCell> unUsedCells = new List<AppleCell>();
    public AppleCell GetUnusedCell()
    {
        AppleCell cell = null;
        if (unUsedCells.Count > 0)
        {
            cell = unUsedCells[0];
            unUsedCells.RemoveAt(0);
        }
        else
        {
            var go = Instantiate(GetPrefab("Apple/Cell"));
            if (go != null)
            {
                cell = go.GetComponent<AppleCell>();
            }
        }

        if (cell != null)
        {
            cell.gameObject.SetActive(true);
            return cell;
        }
        return null;
    }
    public void ReturnCell(AppleCell cell)
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
