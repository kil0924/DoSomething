using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class TetrisMap : MonoBehaviour
{
    private const int NotValidLine = 4;
    [CanBeNull]
    public TetrisCell this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height + NotValidLine)
            {
                return null;
            }
            return map[y][x];
        }
    }
    
    private int _width;
    public int width => _width;
    private int _height;
    public int height => _height;
    private List<List<TetrisCell>> map = new List<List<TetrisCell>>();
    [SerializeField]
    private GridLayoutGroup _gridLayoutGroup;
    public void ClearMap()
    {
        foreach (var line in map)
        {
            foreach (var cell in line)
            {
                TetrisResourceManager.instance.ReturnCell(cell);    
            }
            line.Clear();
        }
        map.Clear();
    }

    public void MakeMap(int width, int height)
    {
        ClearMap();
        
        _width = width;
        _height = height;
        
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _width;
        
        for (int i = 0; i < _height + NotValidLine; i++)
        {
            var line = new List<TetrisCell>();
            map.Add(line);
            for (int j = 0; j < _width; j++)
            {
                var cell = TetrisResourceManager.instance.GetUnusedCell();
                if (cell == null)
                {
                    Debug.LogError("Cell is null");
                    ClearMap();
                    return;
                }
                cell.Init(j, i, i<height);
                cell.transform.SetParent(transform);
                line.Add(cell);
            }
        }
    }
    
    public int widthTest;
    public int heightTest;
    [ContextMenu( "MakeMapTest" )]
    public void MakeMapTest()
    {   
        MakeMap(widthTest, heightTest);
    }

    public void Repaint(TetrisBlock curBlock)
    {
        foreach (var line in map)
        {
            foreach (var cell in line)
            {
                if (cell.isEmpty)
                {
                    if (cell.isValid == false)
                    {
                        cell.SetColor(Color.gray);
                    }
                    else
                    {
                        cell.SetColor(Color.white);    
                    }
                }
            }
        }

        if (curBlock != null)
        {
            foreach (var block in curBlock.blocks)
            {
                int x = curBlock.position.x + block.x;
                int y = curBlock.position.y + block.y;
                this[x, y]?.SetColor(curBlock.color);
            }
        }
    }

    public void FixBlock(TetrisBlock curBlock)
    {
        foreach (var block in curBlock.blocks)
        {
            int x = curBlock.position.x + block.x;
            int y = curBlock.position.y + block.y;
            this[x, y]?.SetColor(curBlock.color);
            this[x, y]?.Fix();
        }
    }

    public IEnumerator RemoveLine(int line, Action onFinish)
    {
        var flags = new List<bool>();
        for(int x = 0; x < _width; x++)
        {
            flags.Add(false);
            int index = x;
            StartCoroutine(this[x, line]?.Effect(() =>
            {
                flags[index] = true;
            }));
        }
        yield return new WaitUntil(() => flags.TrueForAll(x => x));
        for(int x = 0; x < _width; x++)
        {
            this[x, line]?.SetEmpty();
        }
        onFinish?.Invoke();
    }
}
