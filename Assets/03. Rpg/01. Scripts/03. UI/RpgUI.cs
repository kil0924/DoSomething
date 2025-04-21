using TMPro;
using UnityEngine;

public class RpgUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _curStateText;
    
    public void SetCurStateText(string text)
    {
        _curStateText.text = text;
    }
    
}
