using System;
using System.Runtime.Serialization;

namespace com.unity.mgobe.src.EventUploader {
    // 基本参数
    // public static class Events
    // {
    //     // 初始化SDK
    //     public const string InitSdk = "e1";

    //     // 接口调用
    //     public const  string Request = "e2";
    //     // 心跳时延
    //     public const string Ping = "e3";
    //     // 收发帧间隔
    //     public const string RecFrame = "e4";
    //     // 帧广播间隔
    //     public const string FrameRate = "e5";
    // }
    public static class Events {
        // 初始化SDK
        public const string InitSdk = "cs1";

        // 接口调用
        public const string Request = "cs2";
        // 心跳时延
        public const string Ping = "cs3";
        // 收发帧间隔
        public const string RecFrame = "cs4";
        // 帧广播间隔
        public const string FrameRate = "cs5";
    }

    [Serializable]
    public class BaseEventParam : ISerializable {
        public string Sv; // sdk 版本
        public int Sc; // sdk 渠道
        public string Pi; // openID
        public string Gi; // gameID
        public BaseEventParam () {
            // Sv = "";
            // Sc = 0;
            // Pi = "";
            // Gi = "";
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("sv", Sv);
            info.AddValue ("sc", Sc);
            info.AddValue ("pi", Pi);
            info.AddValue ("gi", Gi);
        }
    }
    // 上报接口调用拓展参数
    [Serializable]
    public class ReqEventParam : BaseEventParam, ISerializable {
        public int RqCmd; // 请求名称
        public string RqSq; // seq
        public int RqCd; // 错误码
        public long Time; // 时延
        public ReqEventParam () {

        }

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {
            base.GetObjectData (info, context);
            info.AddValue ("rqCmd", RqCmd);
            info.AddValue ("rqSq", RqSq);
            info.AddValue ("rqCd", RqCd);
            info.AddValue("time", Time);
        }

        protected ReqEventParam (SerializationInfo info, StreamingContext context) {
            RqCmd = info.GetInt32 ("rqCmd");
            RqSq = info.GetString ("rqSq");
            RqCd = info.GetInt32 ("rqCd");
            Time = info.GetInt64("time");
        }
    }

    // 上报心跳时延
    [Serializable]
    public class PingEventParam : BaseEventParam, ISerializable {
        public PingEventParam (long time) {
            this.Time = time;
        }

        public long Time { get; }
        public PingEventParam () {

        }
        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {
            base.GetObjectData (info, context);
            info.AddValue ("time", Time);
        }
    }

    [Serializable]
    public class RecvFrameEventParam : BaseEventParam, ISerializable {
        public long SdFt; // 发收帧间隔
        public RecvFrameEventParam () {

        }
        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {
            base.GetObjectData (info, context);
            info.AddValue ("sdFt", SdFt);
        }

    }

    [Serializable]
    public class FrameRateEventParam : BaseEventParam, ISerializable {
        public long FrRt; // 画面帧率
        public long ReFt; // 帧广播间隔
        public FrameRateEventParam () {

        }

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {
            base.GetObjectData (info, context);
            info.AddValue ("frRt", FrRt);
            info.AddValue ("reFt", ReFt);
        }

    }
}