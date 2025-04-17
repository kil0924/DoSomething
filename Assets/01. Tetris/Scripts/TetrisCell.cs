using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TetrisCell : MonoBehaviour
{
    public bool isEmpty = true;
    public bool isValid = true;
    public Image cellImage;
    public Image effectImage;
    public void Init(int x, int y, bool isValid)
    {
        this.isValid = isValid;
        SetEmpty();
        #if UNITY_EDITOR
        gameObject.name = $"Cell [{x}, {y}]";
        #endif
    }
    
    public void SetEmpty()
    {
        isEmpty = true;
        if(isValid)
            SetColor(Color.white);
        else
            SetColor(Color.gray);
        effectImage.gameObject.SetActive(false);
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

    public void Fix()
    {
        isEmpty = false;
    }

    public IEnumerator Effect(Action onFinish)
    {
        effectImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        effectImage.gameObject.SetActive(false);
        onFinish?.Invoke();
    }
}
