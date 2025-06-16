using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private int _frameCount;
    private int _totalFrameCount;
    private float _time;
    private float _totalTime;

    private int _lastFPS;
    private int _totalFPS;
    
    public TextMeshProUGUI lastFPSText;
    public TextMeshProUGUI totalFPSText;
    void Update()
    {
        _frameCount++;
        _totalFrameCount++;
        _time += Time.deltaTime;
        _totalTime += Time.deltaTime;

        if (_time >= 1f)
        {
            _lastFPS = (int)(_frameCount / _time);
            _frameCount = 0;
            _time = 0f;
            
            _totalFPS = (int)(_totalFrameCount / _totalTime);
     
            SetFPS(_lastFPS, lastFPSText);
            SetFPS(_totalFPS, totalFPSText);
        }
    }
    private void SetFPS(int fps, TextMeshProUGUI text)
    {
        if(text == null)
            return;
        
        text.text = fps.ToString();
        if (fps >= 50)
        {
            text.color = Color.green;
        }
        else if (fps >= 30)
        {
            text.color = Color.yellow;
        }
        else
        {
            text.color = Color.red;
        }
    }
    public void ResetFPS()
    {
        _frameCount = 0;
        _time = 0f;
        _lastFPS = 0;
        
        _totalFrameCount = 0;
        _totalTime = 0;
        _totalFPS = 0;
        
        SetFPS(_lastFPS, lastFPSText);
        SetFPS(_totalFPS, totalFPSText);
    }
}
