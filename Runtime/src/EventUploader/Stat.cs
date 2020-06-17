using System;

namespace com.unity.mgobe.src.EventUploader
{
    public static class StatCallbacks
    {
        public static Action<double> onPingTime = (double time) => {return;};
        public static Action<double> onFitFrameTime = (double deltaTime) => { return; };
        public static Action<double> onBstFrameRate = (double deltaTime) => { return; };
        public static Action<double> onRenderFrameRate = (double deltaTime) =>{ return; };
    }
}