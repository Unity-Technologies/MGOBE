using System;
using System.Timers;

namespace com.unity.mgobe.src.Util {
    public class Timer : System.Timers.Timer {

        private Action _timeEvent;

        public void SetTimer (Action onTimedEvent, int interval) {
            this.Stop();
            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = true;
            this.Start();
        }

        public void SetTimeout (Action onTimedEvent, int interval) {
            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = false;
            this.Start ();
        }

        private void OnElapsedEvent (object sender, EventArgs e) {
            this._timeEvent ();
        }
    }

    public class TimeEventArgs : EventArgs {
        public TimeEventArgs () {

        }

        public string Seq { get; set; }
    }
}