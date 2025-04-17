using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum Match3CellType
{
    Empty = 0,
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    End,
}
public class Match3Cell : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
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
            case Match3CellType.Empty:
                SetColor(new Color(0, 0, 0, 0.5f));
                break;
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

    private Vector2 _startPos;
    private bool _isDrag;
    private RectTransform rectTransform;
    private Vector2 _originPos;
    private int _siblingIndex;
    private float _threshold = 1f;
    private float _maxDelta = 50f;
    private Vector2 _dragDir;
    private float _dragValidThreshold = 5f;
    private bool _dragValid;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDrag = true;
        _startPos = eventData.position;
        rectTransform = GetComponent<RectTransform>();
        
        _originPos = rectTransform.anchoredPosition;
        _siblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        ActiveBorder(true);
        _dragDir = Vector2.zero;
        _dragValid = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_isDrag == false) return;
        
        var delta = eventData.position - _startPos;
        if (Mathf.Abs(delta.x) < _threshold && Mathf.Abs(delta.y) < _threshold) return;

        if (_dragDir == Vector2.zero)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                _dragDir = new Vector2(delta.x > 0 ? 1 : -1, 0);
            }
            else
            {
                _dragDir = new Vector2(0, delta.y > 0 ? 1 : -1);
            }
        }
        else
        {
            if (_dragDir.x > 0)
            {
                delta.x = Mathf.Clamp(delta.x, 0, _maxDelta);
                delta.y = 0;
            }

            if (_dragDir.x < 0)
            {
                delta.x = Mathf.Clamp(delta.x, -_maxDelta, 0);
                delta.y = 0;
            }

            if (_dragDir.y > 0)
            {
                delta.y = Mathf.Clamp(delta.y, 0, _maxDelta);
                delta.x = 0;
            }

            if (_dragDir.y < 0)
            {
                delta.y = Mathf.Clamp(delta.y, -_maxDelta, 0);
                delta.x = 0;
            }
        }
        
        rectTransform.anchoredPosition = delta + _originPos;
    }
    private Action<int, int, int, int> onDrop;
    public void SetOnDrop(Action<int, int, int, int> onDrop)
    {
        this.onDrop = onDrop;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        var delta = eventData.position - _startPos;
        if (Mathf.Abs(delta.x) >= _dragValidThreshold || Mathf.Abs(delta.y) >= _dragValidThreshold)
            _dragValid = true;
        else
            _dragValid = false;
        
        _isDrag = false;
        rectTransform.anchoredPosition = _originPos;
        transform.SetSiblingIndex(_siblingIndex);
        ActiveBorder(false);

        if (_dragValid)
        {
            onDrop?.Invoke(x, y, (int)_dragDir.x, (int)_dragDir.y);
        }
    }
}
