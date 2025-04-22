using System;
using System.Collections.Generic;
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
        string[] guids = AssetDatabase.FindAssets("t:SkeletonDataAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            if (asset != null)
            {
                asset.scale = scale;
                EditorUtility.SetDirty(asset);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"SkeletonDataAsset scale updated to {scale} on {guids.Length} assets.");
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

    public bool isLeft;
    [ContextMenu( "Test2" )]
    public void Test2()
    {
        Debug.Log("asdf");
        unit.SetSide(isLeft);
        isLeft = !isLeft;
    }

    public void OnEnable()
    {
        var renderer = GetComponent<Renderer>();
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = -1;
    }

}
