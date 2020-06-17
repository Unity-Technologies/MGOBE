using System;
using System.Collections.Generic;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;
using com.unity.mgobe.src.EventUploader;



namespace com.unity.mgobe.src.Broadcast
{
    public class FrameCache
    {
        public int EndFrameId { get; }

        public List<Frame> Frames { get; } = new List<Frame>();

        public void Add(Frame frame)
        {
            Frames.Add(frame);
        }
        public FrameCache(int endFrameId)
        {
            this.EndFrameId = endFrameId;
        }
    };

    public class FrameBroadcast
    {
        private readonly FrameBroadcastTimer _timer;
        private readonly Action<BroadcastEvent> _callback;
        private int _frameIdFill = 0;
        private int _frameIdSent = 0;

        // 自动补帧失败次数
        private int _autoReqFrameErrTimes = 0;
        // 上一帧时间
        private long _lastFrameTime = 0;
        // 补帧参数
        private int _beginFrameId = -1;
        private int _endFrameId = -1;

        private readonly Dictionary<int, FrameCache> _fillCache = new Dictionary<int, FrameCache>();

        public FrameBroadcast(long frameTime, Action<BroadcastEvent> callback)
        {
            _timer = new FrameBroadcastTimer(frameTime);
            this._callback = callback;
        }

        public void Reset(int sentFrameId)
        {
            this._timer.Init();
            this._frameIdSent = sentFrameId;
            this._frameIdFill = sentFrameId;
            // 初始化帧时间
            this._lastFrameTime = 0;
        }

        public void Push(BroadcastEvent eve, com.unity.mgobe.Room room)
        {
            var bst = (RecvFrameBst)eve.Data;
            var frameId = Convert.ToInt32(bst.Frame.Id);

            if (frameId == 1)
            {
                this.Reset(0);
            }

            this._timer.Push(frameId, SdkUtil.GetCurrentTimeMilliseconds());

            var frameIdSent = this._frameIdSent;
            var frameIdFill = this._frameIdFill;
            this._frameIdFill = frameId;

            // 不自动补帧就直接发出去
            if (!Config.IsAutoRequestFrame)
            {
                this.Send(eve);
                return;
            }

            if (frameId <= frameIdSent + 1)
            {
                this.Send(eve);
                return;
            }

            this._fillCache.Add(frameId, new FrameCache(frameId));
            this._fillCache[frameId].Add(bst.Frame);

            if (frameId > frameIdFill + 1)
            {
                this.Fill(frameIdFill + 1, frameId - 1, room);
            }
        }

        public void RetryFill(com.unity.mgobe.Room room)
        {
            if (this._beginFrameId == this._endFrameId && this._beginFrameId < 0)
            {
                return;
            }
            this._autoReqFrameErrTimes = 0;

            this.Fill(this._beginFrameId, this._endFrameId, room);
        }

        private void Send(BroadcastEvent eve)
        {
            var bst = (RecvFrameBst)eve.Data;

            var frameId = 0;
            if (bst.Frame != null)
            {
                frameId = (int) bst.Frame.Id;
            }
            if (frameId <= this._frameIdSent)
            {
                Debugger.Log("FrameId <= frameId Send {0}", frameId);
                return;
            }

            if (bst.Frame == null) return;
            try {
                bst.Frame.Time = this._timer.Time(frameId);
            }  catch ( Exception e) {
                Debugger.Log(e.ToString());
            }
            this._frameIdSent = frameId;
            this._callback(eve);

            // 修正帧时间
            if (Math.Abs(this._lastFrameTime) > 0)
            {
                this._lastFrameTime = bst.Frame.Time;
            }


            var dTime = bst.Frame.Time - this._lastFrameTime;

            if (Math.Abs(dTime) > 0)
            {
                StatCallbacks.onFitFrameTime?.Invoke(dTime);
            }

            if (Math.Abs(dTime) > 300)
            {
                Debugger.Log("重置");
                this.Reset(frameId);
            }

            this._lastFrameTime = bst.Frame.Time;
        }

        private void FillSend(int beginFrameId)
        {
            while (true)
            {
                if (beginFrameId > this._frameIdSent + 1 || !this._fillCache.ContainsKey(beginFrameId)) return;
                var cache = this._fillCache[beginFrameId];
                _fillCache.Remove(beginFrameId);
                foreach (var item in cache.Frames)
                {
                    this.Send(new BroadcastEvent(item, ""));
                }

                beginFrameId = cache.EndFrameId + 1;
            }
        }

        private void Fill(int beginFrameId, int endFrameId, com.unity.mgobe.Room room)
        {
            if (!room.IsInRoom())
            {
                return;
            }
            Action<ResponseEvent> callback = (eve) =>
            {
                this._beginFrameId = beginFrameId;
                this._endFrameId = endFrameId;
                if (eve.Code != ErrCode.EcOk)
                {
                    this._autoReqFrameErrTimes++;
                    if (this._autoReqFrameErrTimes <= 5)
                    {
                        this.Fill(beginFrameId, endFrameId, room);
                    }
                    else
                    {
                        room.OnAutoRequestFrameError?.Invoke(new BroadcastEvent(eve, ""));
                    }
                }
                else
                {
                    this._beginFrameId = -1;
                    this._endFrameId = -1;

                    this._autoReqFrameErrTimes = 0;
                    var rsp = (RequestFrameRsp)eve.Data;
                    var cache = new FrameCache(endFrameId);
                    foreach (var item in rsp.Frames)
                    {
                        var frame = new Frame
                        {
                            Id = item.Id,
                            Ext = item.Ext,
                            Time = item.Time,
                            RoomId = room.RoomInfo.Id,
                            IsReplay = true
                        };
                        frame.Items.AddRange(item.Items);
                        cache.Add(frame);
                    }
                    this._fillCache.Add(beginFrameId, cache);
                    this.FillSend(beginFrameId);
                }
            };
        }
    }

    public class FrameBroadcastTimer
    {
        private int _n;
        private readonly double _a1;
        private long _sXi;
        private long _sYi;
        private long _sXiyi;
        private long _sXixi;

        public FrameBroadcastTimer(double a1)
        {
            this._a1 = a1;
            this.Init();
        }

        public void Init()
        {
            this._n = 0;
            this._sXi = 0;
            this._sYi = 0;
            this._sXiyi = 0;
            this._sXixi = 0;
        }
        public void Push(int xi, long yi)
        {
            this._n++;
            this._sXi += xi;
            this._sYi += yi;
            this._sXiyi += xi * yi;
            this._sXixi += xi * xi;
        }

        private double A0(double a1)
        {
            if (Math.Abs(a1) < 0) a1 = this.A1();
            return (this._sYi / this._n) - this.A1() * this._sXi / this._n;
        }

        private double A1()
        {
            return (this._n * this._sXiyi - this._sXi * this._sYi) / (this._n * this._sXixi - this._sXi * this._sXi);
        }

        private void Ap(ref double a0, ref double a1)
        {
            a1 = this.A1();
            a0 = this.A0(a1);
        }
        public long Time(int xi)
        {
            if (Math.Abs(this._n) < 0.00000001)
            {
                return 0;
            }
            var a0 = new double();
            var a1 = new double();
            if (this._n >= 2)
            {
                this.Ap(ref a0, ref a1);
            }
            else
            {
                a0 = this._sYi - this._sXi * this._a1;
                a1 = this._a1;
            };
            return Convert.ToInt64(a0 + xi * a1);
        }

    }
}