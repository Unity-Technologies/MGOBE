using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using com.unity.mgobe.src.Net.Sockets;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def; // using System.Timers;

namespace com.unity.mgobe.src.Net {
    public struct MessageWrapper {
        public byte Pre { get; set; }

        public byte End { get; set; }

        public byte[] Body { get; set; }
    };

    public class SendQueueValue {
        public Action<int, string> sendFail = (int errCode, string errMsg) => { };

        public Action sendSuccess = () => { };
        public Action resend = () => { };
        public Action remove = () => { };
        public Action<DecodeRspResult> response;

        public DateTime Time { get; set; }

        public bool IsSocketSend { get; set; }

        public int Cmd { get; set; }
    };

    public enum MessageDataTag : byte {
        ClientPre = 0x02,
        ClientEnd = 0x03,

        // 接收请求 - 发送响应
        ServerPre = 0x28,
        ServerEnd = 0x29,
    }

    public delegate void NetResponseCallback (bool send, DecodeRspResult result, Action<ResponseEvent> callback);

    public delegate void BroadcastCallback (DecodeBstResult bstResult, string seq);
    // public delegate void EventHandler(object sender, SocketEvent e);
    // public delegate void EventHandler(object sender, SocketEvent e, Action<byte[]> handleResponse);

    public class Net : IDisposable {
        protected static readonly ConcurrentDictionary<string, SendQueueValue> SendQueue = new ConcurrentDictionary<string, SendQueueValue> ();
        protected static readonly ConcurrentDictionary<ServerSendClientBstWrap2Type, BroadcastCallback> BroadcastHandlers = new ConcurrentDictionary<ServerSendClientBstWrap2Type, BroadcastCallback> ();
        private static readonly Timer ResendTimer = new Timer ();
        private static readonly Timer TimeoutTimer = new Timer ();

        protected readonly ConcurrentDictionary<ServerSendClientBstWrap2Type, Object> bdhandlers = new ConcurrentDictionary<ServerSendClientBstWrap2Type, Object> ();
        // 该实例对象的发送队列
        private readonly ConcurrentDictionary<string, Object> _queue;
        // public KCPSocket socket;
        // private QAppProtoErrCode ErrCode;
        private Action<byte[]> _handleResponse;
        private Action<byte[]> _handleBroadcast;

        public Socket Socket { get; set; }

        // 循环检测 sendQueue 中的消息发送
        public static void StartQueueLoop () {
            ResendTimer.SetTimer (CheckSendQueue, Config.ResendInterval);
        }

        private static readonly Action CheckSendQueue = () => {
            foreach (var val in SendQueue.Select (kv => kv.Value)) {
                if (!val.IsSocketSend && DateTime.Now.Subtract (val.Time).TotalMilliseconds > Config.ResendInterval) {
                    val.resend ();
                }
            }
        };

        private static readonly Action CheckSendQueueTimeout = () => {
            foreach (var val in SendQueue.Select (kv => kv.Value)) {
                if (DateTime.Now.Subtract (val.Time).TotalMilliseconds > Config.ResendTimeout) {
                    int code;
                    var msg = "";
                    if (UserStatus.IsStatus (UserStatus.StatusType.Login)) {
                        code = (int) QAppProtoErrCode.EcSdkResTimeout;
                    } else {
                        if (UserStatus.GetErrCode () == (int) QAppProtoErrCode.EcOk) {
                            code = (int) QAppProtoErrCode.EcSdkNoLogin;
                            msg = "登录失败";
                        } else {
                            code = UserStatus.GetErrCode ();
                            msg = "登录失败，" + UserStatus.GetErrMsg ();
                        }
                    }
                    val.sendFail (code, msg);
                }
            }
        };

        // 停止检测消息发送, 清空全部消息
        public static void StopQueueLoop () {
            TimeoutTimer.Stop ();
            ResendTimer.Stop ();
            foreach (var val in SendQueue.Select (kv => kv.Value)) {
                val.remove ();
            }
            SendQueue.Clear ();
        }

        protected Net () {
            Socket = null;
            _queue = new ConcurrentDictionary<string, Object> ();
        }

        // 绑定 socket 对象
        public bool BindSocket (Socket socket, Action<byte[]> handleResponse, Action<byte[]> handleBroadcast) {
            if (this.Socket != null || socket == null) return false;
            this.Socket = socket;

            this._handleResponse = handleResponse;
            this._handleBroadcast = handleBroadcast;

            if (this.Socket.IsMsgBind == false) {
                this.Socket.OnEvent ("message", OnMessageEvent);
            }
            return true;
        }

        private void OnMessageEvent (SocketEvent socketEvent) {
            if (socketEvent.Data.Length == 0) return;
            var resData = socketEvent.Data;
            var msgWrap = UnpackBody (socketEvent.Data);
            switch (msgWrap.Pre) {
                case (byte) MessageDataTag.ClientPre when (byte) MessageDataTag.ClientEnd == msgWrap.End:
                    _handleResponse (msgWrap.Body);
                    break;
                case (byte) MessageDataTag.ServerPre when (byte) MessageDataTag.ServerEnd == msgWrap.End:
                    _handleBroadcast (msgWrap.Body);
                    break;
            }
        }

        public void UnbindSocket () {
            Socket = null;
            this.ClearQueue ();
            this.ClearBdHandlers ();
        }

        // 构建请求数据
        protected static byte[] BuildData (byte pre, byte[] body, byte end) {
            var uintValue = (uint) (body.Length + 6);
            // Debugger.Log("Build data body length: {0} {1}", body.Length, uintValue);

            var uintBytes = BitConverter.GetBytes (uintValue);
            Array.Reverse (uintBytes);
            using (var memory = new MemoryStream ())
            using (var writer = new BinaryWriter (memory)) {
                writer.Write (pre);
                writer.Write (uintBytes);
                writer.Write (body);
                writer.Write (end);
                return memory.ToArray ();
            }
        }

        // 解析消息数据
        private static MessageWrapper UnpackBody (byte[] data) {
            using (var memory = new MemoryStream (data))
            using (var reader = new BinaryReader (memory)) {
                var msg = new MessageWrapper { Pre = reader.ReadByte () };
                var pkgLenBytes = reader.ReadBytes (4);
                msg.Body = reader.ReadBytes (data.Length - 6);
                msg.End = reader.ReadByte ();
                // Debugger.Log("{0}, {1}", msg.pre, msg.end);
                return msg;
            }
        }

        // 清空该实例对象的消息队列
        public void ClearQueue () {
            var keys = this._queue.Keys;

            foreach (var seq in keys) {
                SendQueue.TryRemove (seq, out SendQueueValue s);
            }

            this._queue.Clear ();
        }

        // 清空该实例对象的广播回调
        private void ClearBdHandlers () {
            var keys = this.bdhandlers.Keys;

            foreach (var type in keys) {
                BroadcastHandlers.TryRemove (type, out BroadcastCallback s);
            }

            bdhandlers.Clear ();
        }

        // 向请求队列中添加记录
        protected void AddSendQueue (string seq, SendQueueValue value) {
            SendQueue.TryAdd (seq, value);
            this._queue.TryAdd (seq, null);
        }

        // 在请求队列中删除记录
        public void DeleteSendQueue (string seq) {
            SendQueue.TryRemove (seq, out SendQueueValue s);
            this._queue.TryRemove (seq, out Object q);
        }

        // 设置广播回调
        private void SetBroadcastHandler (ServerSendClientBstWrap2Type type, BroadcastCallback callback) {
            Net.BroadcastHandlers.TryAdd (type, callback);
            bdhandlers.TryAdd (type, null);
        }

        // 处理请求的响应错误码
        private bool HandleErrCode () {
            return false;
        }

        // 调用 Socket 发送消息
        protected string Send (byte[] data, string seq, ProtoCmd subcmd) {
            var readyCode = GetReadyCode (subcmd);
            if (readyCode != 0) {
                HandleSendFail (seq, readyCode);
            } else if (data.Length > 1016 && Socket.Id == 1) {
                HandleSendFail (seq, ErrCode.EcRelayDataExceedLimited);
            } else {

                Socket.Send (data,
                    (code) => HandleSendFail (seq, code),
                    () => HandleSendSuccess (seq)
                );
            }
            return seq;
        }

        // 发送失败 Callback
        private void HandleSendFail (string seq, int code) {
            SendQueueValue val = null;
            SendQueue.TryGetValue (seq + "", out val);
            if (val == null) return;

            // 处理 wssocket 帧长度超过 856B
            if (code == ErrCode.EcRelayDataExceedLimited || DateTime.Now.Subtract (val.Time).TotalMilliseconds > Config.ResendTimeout) {
                var sendCode = UserStatus.GetErrCode () != 0 ? UserStatus.GetErrCode () : code;
                val.sendFail (sendCode, null);
                return;
            }
            switch (code) {
                case (int) QAppProtoErrCode.EcSdkUninit:
                    // 没有初始化
                    val.sendFail (code, null);
                    break;
                case (int) QAppProtoErrCode.EcSdkNoLogin:
                    // 没登录
                    Socket.Emit ("autoAuth", new SocketEvent ());
                    return;
                case (int) QAppProtoErrCode.EcSdkNoCheckLogin:
                    {
                        // 没checklogin
                        Socket.Emit ("autoAuth", new SocketEvent ());
                        return;
                    }
            }
            return;
        }

        // 发送成功 Callback
        private static void HandleSendSuccess (string seq) {
            SendQueueValue val = null;
            SendQueue.TryGetValue (seq + "", out val);
            if (seq == "" || val == null) return;
            val.sendSuccess ();
        }

        private int GetReadyCode (ProtoCmd subcmd) {
            if (!SdkStatus.IsInited () && subcmd != ProtoCmd.ECmdLoginReq) {
                // 发送失败: 没有初始化 (login不需要初始化)
                var info = new PlayerInfo {
                Id = ""
                };
                GamePlayerInfo.SetInfo (info);
                UserStatus.SetStatus (UserStatus.StatusType.Logout);
                return ErrCode.EcSdkUninit;
            }

            // 检测 socket
            if (Socket == null || string.IsNullOrEmpty (Socket.Url))
                return (int) QAppProtoErrCode.EcSdkSendFail;

            if (!UserStatus.IsStatus (UserStatus.StatusType.Login) && subcmd != ProtoCmd.ECmdLoginReq && subcmd != ProtoCmd.ECmdLogoutReq)
                return (int) QAppProtoErrCode.EcSdkNoLogin;

            if (Socket.Id == (int) ConnectionType.Relay && !CheckLoginStatus.IsChecked () &&
                (subcmd == ProtoCmd.ECmdRelaySendFrameReq || subcmd == ProtoCmd.ECmdRelayRequestFrameReq ||
                    subcmd == ProtoCmd.ECmdHeartBeatReq || subcmd == ProtoCmd.ECmdRelayClientSendtoGamesvrReq))
                return (int) QAppProtoErrCode.EcSdkNoCheckLogin;

            // 发送消息
            return 0;
        }

        public void Dispose () {

        }
    }
}