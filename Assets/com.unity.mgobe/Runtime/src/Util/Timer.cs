using System;
using System.Timers;

namespace Packages.com.unity.mgobe.Runtime.src.Util
{
    public class Timer : System.Timers.Timer
    {

        private Action _timeEvent;
        public static void SetTimer(System.Timers.Timer timer, ElapsedEventHandler onTimedEvent, int interval)
        {
            timer.Interval = interval;
            timer.Elapsed += onTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void SetTimer(Action onTimedEvent, int interval){
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = true;
            this.Enabled = true;
            this.Start();
        }

        private void OnElapsedEvent(object sender, EventArgs e) {
            this._timeEvent();
        }
    }

    public class TimeEventArgs : EventArgs
    {
        public TimeEventArgs()
        {

        }

        public string Seq { get; set; }
    }
}