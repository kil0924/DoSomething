using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rpg;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<SkeletonDataAsset> list;
    public float scale;
    [ContextMenu( "Test" )]
    public void TestMethod()
    {
        List<int> listA = Enumerable.Range(1, 100000).ToList();
        List<int> listB = Enumerable.Range(1, 100000).ToList();
        var time = 0f;
        time = Time.realtimeSinceStartup;
        listB.ForEach(x =>
        {
            if(x % 10000 == 0)
                Debug.Log(x);
        });
        Debug.Log($"Time:{Time.realtimeSinceStartup - time}");
        time = Time.realtimeSinceStartup;
        foreach (var i in listA)
        {
            if(i % 10000 == 0)
                Debug.Log(i);
        }
        Debug.Log($"Time:{Time.realtimeSinceStartup - time}");
    }

    public string path;
    public Unit unit;
    [ContextMenu( "Test1" )]
    public void Test1()
    {
        var spineData = RpgResourceManager.instance.GetSkeletonDataAsset(path);
        var prefab = RpgResourceManager.instance.GetPrefab($"Rpg/Unit");
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found. prefab:Rpg/Unit");
            return;
        }
            
        var go = Instantiate(prefab);
            
        unit = go.GetComponent<Unit>();
        if (unit == null)
        {
            unit = go.AddComponent<Unit>();
        }
            
        unit.SetSpine(spineData);
    }

    [ContextMenu( "Test2" )]
    public void Test2()
    {
        var list = new List<TestClass>();
        for (int i = 0; i < 10000000; i++)
        {
            list.Add(new TestClass());
        }
        var time = 0f;
        time = Time.realtimeSinceStartup;
        list.ForEach(x =>
        {
            if (x.type == TestType.test)
            {
                
            }
        });
        float a = Time.realtimeSinceStartup - time;
        Debug.Log($"Time:{a}");
        time = Time.realtimeSinceStartup;
        list.ForEach(x =>
        {
            if (x.type == TestType.test)
            {
                
            }
        });
        float b = Time.realtimeSinceStartup - time;
        Debug.Log($"Time:{b}");
        
        Debug.Log($"a / b = {a / b * 100}");
    }

    public enum TestType
    {
        origin,
        test,
    }
    public class TestClass
    {
        public TestType type = TestType.origin;
    }
    public class TestClass2 : TestClass
    {
        public TestType type = TestType.test;
    }
}
