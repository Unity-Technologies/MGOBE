using System;
using Google.Protobuf;
using Lagame;
using Lagame;
using Packages.com.unity.mgobe.Runtime.src.EventUploader;
using Packages.com.unity.mgobe.Runtime.src.Net;
using Packages.com.unity.mgobe.Runtime.src.Sender;
using Packages.com.unity.mgobe.Runtime.src.Util;
using Packages.com.unity.mgobe.Runtime.src.Util.Def;

namespace Packages.com.unity.mgobe.Runtime.src.Ping {
    public class Pinger : BaseNetUtil {
        private const int _maxPingRetry = 2;

        private int Timeout {
            get {
                if (this.Id == (int) ConnectionType.Relay && Config.EnableUdp) return Config.PingTimeout / 2;
                return Config.PingTimeout;
            }
        }

        public Timer PingTimer { get; set; } = new Util.Timer ();

        public Timer PongTimer { get; set; } = new Util.Timer ();

        public string CurrentSeq { get; set; } = "";

        public int Id { get; }

        public FrameSender FrameSender { get; }

        public static int MaxPingRetry => _maxPingRetry;

        public int Retry { get; set; } = MaxPingRetry;

        public Pinger (Responses responses, int id, FrameSender frameSender) : base (responses) {
            this.Id = id;
            this.FrameSender = frameSender;
        }

        ///////////////////////////////// PONG //////////////////////////////////
        public void Ping (Action<ResponseEvent> callback) {
            PingTimer.Close ();
            PingTimer = new Timer ();
            if (string.IsNullOrEmpty (RequestHeader.AuthKey)) {
                return;
            }
            var startTime = DateTime.Now;
            var routeId = FrameSender?.RoomInfo?.RouteId ?? "";
            var conType = this.Id == 1 ? ConnectionType.Relay : ConnectionType.Common;
            var body = new HeartBeatReq {
                ConType = conType,
                RouteId = routeId
            };

            void PongResposne (bool send, DecodeRspResult result, Action<ResponseEvent> cb) {
                this.HandlePong (send, result, startTime);
            }

            var seq = this.Send (body.ToByteString (), (int) ProtoCmd.ECmdHeartBeatReq, PongResposne, callback);
            if(this.Id == 1) Debugger.Log("send heartBeat: {0}", seq);
            CurrentSeq = seq;
            this.PongTimer.SetTimer (() => HandlePongTimeout (seq), this.Timeout);
        }

        public void Stop () {
            PingTimer.Close ();
            PongTimer.Close ();
        }

        ///////////////////////////////// PONG //////////////////////////////////
        private void HandlePong (bool send, DecodeRspResult res, DateTime startTime) {
            EventUpload.PushRequestEvent (new ReqEventParam { rqRn = "pong" + this.Id, rqSq = res.RspWrap1.Seq, rqCd = res.RspWrap1.ErrCode });
            PongTimer.Close ();
            PongTimer = new Timer ();

            if (!send) {
                this.HandlePongTimeout (res.RspWrap1.Seq);
            }

            this.Retry = MaxPingRetry;
            // 清空发送队列
            this.client.ClearQueue ();

            // 心跳的错误码单独处理
            var errCode = res.RspWrap1.ErrCode;

            // 上报心跳时延
            if (this.Id == 1 && errCode == ErrCode.EcOk) {
                EventUpload.PushPingEvent (new PingEventParam (Convert.ToInt64 ((DateTime.Now - startTime).TotalMilliseconds)));
            }

            if (IsTokenError (errCode)) {
                UserStatus.SetStatus (UserStatus.StatusType.Logout);
                this.client.Socket.Emit ("autoAuth", new SocketEvent ());
            }

            if (IsRelayConnectError (errCode) && this.client.Socket.Id == (int) ConnectionType.Relay) {
                Debugger.Log("ping relay connect error, set offline");
                CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
                this.client.Socket.Emit ("autoAuth", new SocketEvent ());
            }

            this.PingTimer.SetTimer (() => this.Ping (null), this.Timeout);
        }

        //////////////////////////////// TIMEOUT ////////////////////////////////
        private void HandlePongTimeout (string seq) {
            this.PongTimer.Close ();
            this.PongTimer = new Timer ();
            this.client.DeleteSendQueue (seq);
            this.Retry--;
            if (!seq.Equals (this.CurrentSeq)) return;
            if (this.client.Socket == null) return;

            // 针对 KCP 的逻辑
            if (this.Id == (int) ConnectionType.Relay && Config.EnableUdp) {
                if (this.Retry >= 0) {
                    // 重试
                    this.PingTimer.SetTimer (() => this.Ping (null), this.Timeout);
                    return;
                } else {
                    this.Retry = MaxPingRetry;
                }
            }
            this.client.Socket.ConnectNewSocketTask (this.client.Socket.Url);
            this.client.ClearQueue ();
        }

        private static bool IsTokenError (int code) {
            return code == ErrCode.EcAccessCmdGetTokenErr ||
                code == ErrCode.EcAccessCmdTokenPreExpire ||
                code == ErrCode.EcAccessCmdInvalidToken ||
                code == ErrCode.EcAccessGetCommConnectErr;
        }

        private static bool IsRelayConnectError (int code) {
            return code == ErrCode.EcAccessGetCommConnectErr;
        }
    }
}