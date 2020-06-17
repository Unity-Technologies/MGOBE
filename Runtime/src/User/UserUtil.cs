using System;

using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;
using com.unity.mgobe.src.EventUploader;


namespace com.unity.mgobe.src.User
{
    public static class UserUtil
    {
        private static readonly Action<ResponseEvent> LogoutRsp = (ResponseEvent eve) =>
        {
            if (eve.Code == ErrCode.EcOk)
            {
                Core.Pinger1.Stop();
            }
        };
        public static void Login(Action<ResponseEvent> callback)
        {
            var para = new LoginPara();

            void Cb(ResponseEvent e)
            {
                callback?.Invoke(e);
                UserUtil.LoginRsp(e);
            }

            // 使用签名函数的方式
            if (GameInfo.CreateSignature != null && Core.User != null)
            {
                GameInfo.CreateSignature = (Signature signature) => {
                    Debugger.Log("使用签名函数方法登录");
                    Core.User.Login(para, null, signature, Cb);
                };
                Debugger.Log("创建签名函数方法成功");
            }
            if (Core.User != null)
            {
                Core.User.Login(para, GameInfo.SecretKey, null, Cb);
            }
        }

        private static void LoginRsp(ResponseEvent e)
        {
            if (!SdkStatus.IsIniting()) return;
            ResponseEvent eve;
            if (e.Code != ErrCode.EcOk)
            {
                eve = new ResponseEvent(e.Code);
                Core.SdkInitCallback(false, eve);
                return;
            }
            var pingInterval = 5000;
            var reportInterval = 10000;
            var enableUdp = false;
            ulong serverTime = 0;

            var data = (LoginRsp)e.Data;
            if (data.SdkConfig != null)
            {
                if (data.SdkConfig.PingInterval != 0) pingInterval = (int)data.SdkConfig.PingInterval;
                if (data.SdkConfig.ReportInterval != 0) reportInterval = (int)data.SdkConfig.ReportInterval;
                // enableUdp = data.SdkConfig.EnableUdp;
                if (data.SdkConfig.ServerTime != 0) serverTime = data.SdkConfig.ServerTime;
            }

            // 上报
            EventUpload.Validate(GameInfo.OpenId, RequestHeader.PlayerId, reportInterval, null);
            // 心跳间隔
            Config.PingTimeout = pingInterval;
            // 是否使用udp
            Config.EnableUdp = enableUdp;

            var initRsp = new InitRsp(serverTime);
            eve = new ResponseEvent(ErrCode.EcOk, "", "", initRsp);
            Core.SdkInitCallback(true, eve);
            Core.Pinger1.Ping(null);
        }

        public static void Logout()
        {
            if (Core.User != null)
            {
                Core.User.Logout(new LogoutReq(), UserUtil.LogoutRsp);
            }
        }

    }
}