using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TetrisBlockType
{
    I = 0,
    J,
    L,
    O,
    S,
    Z,
    T,
    End,
}
public class TetrisBlock
{
    public TetrisBlockType type;
    public (int x, int y) position;
    public List<(int x, int y)> blocks;
    public Color color = Color.red;
    public TetrisBlock()
    {
        type = (TetrisBlockType)Random.Range((int)TetrisBlockType.I, (int)TetrisBlockType.End);
        switch (type)
        {
            case TetrisBlockType.I:
                blocks = new List<(int x, int y)>()
                {
                    (-1, 0),
                    (0, 0),
                    (1, 0),
                    (2, 0),
                };
                color = Color.cyan;
                break;
            case TetrisBlockType.J:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (0, 1),
                    (0, 2),
                    (-1, 0),
                };
                color = Color.green;
                break;
            case TetrisBlockType.L:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (0, 1),
                    (0, 2),
                    (1, 0),
                };
                color = Color.blue;
                break;
            case TetrisBlockType.O:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (1, 0),
                    (0, -1),
                    (1, -1),
                };
                color = Color.yellow;
                break;
            case TetrisBlockType.S:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (1, 0),
                    (-1, -1),
                    (0, -1),
                };
                color = Color.magenta;
                break;
            case TetrisBlockType.Z:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (-1, 0),
                    (0, -1),
                    (1, -1),
                };
                color = Color.red;
                break;
            case TetrisBlockType.T:
                blocks = new List<(int x, int y)>()
                {
                    (0, 0),
                    (-1, 0),
                    (0, -1),
                    (1, 0),
                };
                color = (Color.red + Color.yellow)/2;
                break;
        }
    }

    public int GetTop()
    {
        int top = 0;
        foreach (var block in blocks)
        {
            if (block.y > top)
                top = block.y;
        }
        return top;
    }

    public int GetBottom()
    {
        int bottom = 999;
        foreach (var block in blocks)
        {
            if (block.y < bottom)
                bottom = block.y;
        }
        return bottom;
    }

    public void Move(int deltaX, int deltaY)
    {
        position = (position.x + deltaX, position.y + deltaY);
    }
    public void Rotate(bool isClockwise)
    {
        for(int i=0; i<blocks.Count; i++)
        {
            var current = blocks[i];
            if (isClockwise)
                blocks[i] = (current.y, -current.x);
            else
                blocks[i] = (-current.y, current.x);
        }
    }
}
