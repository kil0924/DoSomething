using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    public TestUnit unitPrefab;
    public List<TestUnit> unitList = new List<TestUnit>();

    public void Start()
    {
        Application.targetFrameRate = 60;
    }
    
    public void MakeUnit()
    {
        var unit = Instantiate(unitPrefab);
        unit.transform.localPosition = new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-10f, 10f));
        unitList.Add(unit);
    }

    public void Clear()
    {
        foreach (var unit in unitList)
        {
            Destroy(unit.gameObject);
        }
        unitList.Clear();
    }
    
    private bool _unitActive = true;
    private bool _vfxActive = true;
    public void ToggleUnit()
    {
        _unitActive = !_unitActive;
        unitList.ForEach(x => x.ActiveUnit(_unitActive));
    }
    public void ToggleVFX()
    {
        _vfxActive = !_vfxActive;
        unitList.ForEach(x => x.ActiveVFX(_vfxActive));
    }
}
