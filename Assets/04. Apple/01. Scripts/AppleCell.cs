using TMPro;
using UnityEngine;

public class AppleCell : MonoBehaviour
{
    public int number;
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private GameObject _border;
    public int x;
    public int y;
    public bool isActive;
    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        
        number = Random.Range(1, 10);
        _numberText.text = number.ToString();
        ActiveBorder(false);
        isActive = true;
    }

    public void ActiveBorder(bool active)
    {
        _border.SetActive(active);
    }
}
