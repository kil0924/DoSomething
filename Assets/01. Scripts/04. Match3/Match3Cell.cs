using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum Match3CellType
{
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    End,
}
public class Match3Cell : MonoBehaviour
{
    public Image cellImage;
    public Image effectImage;
    public Image borderImage;
    
    public Match3CellType type;
    public Button button;
    public int x;
    public int y;
    public void Init(int x, int y)
    {
#if UNITY_EDITOR
        gameObject.name = $"Cell [{x}, {y}]";
#endif
        SetRandomType();
        SetType(type);    
        this.x = x;
        this.y = y;
        effectImage.gameObject.SetActive(false);
        borderImage.gameObject.SetActive(false);
    }

    public void SetRandomType()
    {
        var type = (Match3CellType)Random.Range((int)Match3CellType.A, (int)Match3CellType.End);
        SetType(type);
    }
    public void SetType(Match3CellType type)
    {
        this.type = type;
        switch (type)
        {
            case Match3CellType.A:
                SetColor(Color.white);
                break;
            case Match3CellType.B:
                SetColor(Color.red);
                break;
            case Match3CellType.C:
                SetColor(Color.blue);
                break;
            case Match3CellType.D:
                SetColor(Color.green);
                break;
            case Match3CellType.E:
                SetColor(Color.yellow);
                break;
            case Match3CellType.F:
                SetColor(Color.magenta);
                break;
            case Match3CellType.G:
                SetColor(Color.cyan);
                break;
        }
    }    
    public void SetColor(Color color)
    {
        cellImage.color = color;
        effectImage.color = color;
    }
    public Color GetColor()
    {
        return cellImage.color;
    }
    
    public void Copy(Match3Cell cell)
    {
        type = cell.type;
    }

    private Action<int, int> onClick;
    public void SetOnClick(Action<int, int> onClick)
    {
        this.onClick = onClick;
        button.onClick.AddListener(()=> this.onClick?.Invoke(x, y));
    }

    public void ActiveBorder(bool isActive)
    {
        borderImage.gameObject.SetActive(isActive);
    }
    public IEnumerator Effect(Action onFinish)
    {
        effectImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        effectImage.gameObject.SetActive(false);
        onFinish?.Invoke();
    }
}
