using System;

namespace Packages.com.unity.mgobe.Runtime.src.EventUploader
{
    public static class FrameBst
    {
        private static DateTime _lastFrameTime;
        public static double deltaTime = 0;
        private static bool _isInit = false;
        public static void Trigger()
        {
            var now = DateTime.Now;
            if (!_isInit)
            {
                _lastFrameTime = now;
                return;
            }

            deltaTime = (now - _lastFrameTime).TotalSeconds;
            StatCallbacks.onBstFrameRate?.Invoke(deltaTime);
            _lastFrameTime = now;
        }

        public static void Clear()
        {
            _isInit = true;
        }
    }
}