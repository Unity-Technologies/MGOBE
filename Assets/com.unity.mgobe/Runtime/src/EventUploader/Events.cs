namespace Packages.com.unity.mgobe.Runtime.src.EventUploader
{
    // 基本参数
    public static class Events
    {
        // 初始化SDK
        public const string InitSdk = "e1";

        // 接口调用
        public const  string Request = "e2";
        // 心跳时延
        public const string Ping = "e3";
        // 收发帧间隔
        public const string RecFrame = "e4";
        // 帧广播间隔
        public const string FrameRate = "e5";
    }
    public class BaseEventParam
    {
        public string sv;   // sdk 版本
        public int sc;      // sdk 渠道
        public string pi;   // openID
        public string gi;   // gameID
        public BaseEventParam()
        {
            sv = "";
            sc = 0;
            pi = "";
            gi = "";
        }
    }
    // 上报接口调用拓展参数
    public class ReqEventParam : BaseEventParam
    {
        public string rqRn; // 请求名称
        public string rqSq; // seq
        public int rqCd;    // 错误码
    }

    // 上报心跳时延
    public class PingEventParam : BaseEventParam
    {
        public readonly long time;    // 心跳时延

        public PingEventParam(long time)
        {
            this.time = time;
        }
    }

    public class RecvFrameEventParam : BaseEventParam
    {
        public long sdFt;    // 发收帧间隔
    }

    public class FrameRateEventParam : BaseEventParam
    {
        public long frRt; // 画面帧率
        public long reFt; // 帧广播间隔
    }
}