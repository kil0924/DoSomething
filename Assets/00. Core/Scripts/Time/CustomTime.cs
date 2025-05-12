using UnityEngine;

namespace Core.Time
{
    public class CustomTime
    {
        private double _updateTime = 0;
        private double _fixedUpdateTime = 0;
        private double _lastFrameTime = 0;
        private double _lastFrameDeltaTime = 0;

        public double deltaTime => _lastFrameDeltaTime;

        public CustomTime()
        {
            Reset();
        }

        public void Reset()
        {
            _updateTime = 0;
            _fixedUpdateTime = 0;
            _lastFrameTime = 0;
            _lastFrameDeltaTime = 0;
        }
        
        public void CalcLastFrameDelta(float deltaTime, bool isFixedUpdate)
        {
            if (isFixedUpdate)
            {
                _fixedUpdateTime += deltaTime;
                _lastFrameDeltaTime = _fixedUpdateTime - _lastFrameTime;
                if (_lastFrameDeltaTime < 0)
                {
                    _fixedUpdateTime = _lastFrameTime;
                    _lastFrameDeltaTime = 0;
                }
                _lastFrameTime = _fixedUpdateTime;
            }
            else
            {
                _updateTime += deltaTime;
                _lastFrameDeltaTime = _updateTime - _lastFrameTime;
                if (_lastFrameDeltaTime < 0)
                {
                    _updateTime = _lastFrameTime;
                    _lastFrameDeltaTime = 0;
                }
                _lastFrameTime = _updateTime;
            }
        }
    }
}
