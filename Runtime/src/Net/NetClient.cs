using System;
using System.Collections.Generic;
using Google.Protobuf;

using com.unity.mgobe.src.EventUploader;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;

namespace com.unity.mgobe.src.Net {
    public class NetClient : Net {

        private readonly int _maxDataLength = Convert.ToInt32 (Math.Pow (2, 12));
        private readonly Responses _responses;
        private static Dictionary<int, Action<byte[]>> _requestMap;

        public static void InitRequestMap () {
            _requestMap.Add ((int) ClientSendServerReqWrap2Cmd.ECmdLoginReq, (byte[] data) => {

            });
        }
        public NetClient (Responses responses) {
            this._responses = responses;
        }

        // 发送消息请求
        public string SendRequest (ByteString body, int subcmd, NetResponseCallback response, Action<ResponseEvent> callback, string cmd, string seq) {
            if (seq.Length == 0) {
                seq = Guid.NewGuid ().ToString ();
                var sendQueueVal = new SendQueueValue {
                    Time = DateTime.Now,
                        IsSocketSend = false,
                        Cmd = (int) subcmd,
                        resend = () => this.SendRequest (body, subcmd, response, callback, cmd, seq),
                        response = msg => {
                            response (true, msg, callback);
                            DeleteSendQueue (seq);
                        }
                };
                sendQueueVal.sendSuccess = () => {
                    // if(Socket.Id == 1) Debugger.Log("handle send success {0}", seq);
                    sendQueueVal.IsSocketSend = true;
                };
                sendQueueVal.remove = () => {
                    DeleteSendQueue (seq);
                };
                sendQueueVal.sendFail = (errCode, errMsg) => {
                    var errMessage = "消息发送失败，" + errMsg + "[" + errCode + "]";
                    var rspWrap1 = new ClientSendServerRspWrap1 {
                        Seq = seq,
                        ErrCode = errCode,
                        ErrMsg = errMessage
                    };
                    response (false, new DecodeRspResult {
                        RspWrap1 = rspWrap1,
                    }, callback);
                    DeleteSendQueue (seq);
                };
                AddSendQueue (seq, sendQueueVal);
            }

            // PB request = new PB();

            var qAppRequest = new ClientSendServerReqWrap1 {
                Version = RequestHeader.Version,
                AppName = RequestHeader.AppName,
                ClientIp = RequestHeader.ClientIp,
                ServiceIp = RequestHeader.ServiceIp,
                Business = RequestHeader.Business,
                AuthKey = RequestHeader.AuthKey,
                AuthType = RequestHeader.AuthType,
                AuthIp = RequestHeader.AuthIp,
                GameId = RequestHeader.GameId,
                Uid = RequestHeader.Uid,
                PlayerId = RequestHeader.PlayerId,
                Cmd = cmd,
                Seq = seq
            };
            var accessReq = new ClientSendServerReqWrap2 ();
            accessReq.Cmd = (ProtoCmd) subcmd;
            var data = Pb.EncodeReq (qAppRequest, accessReq, body);

            if (data.Length > _maxDataLength) {
                var val = SendQueue.ContainsKey (seq) ? SendQueue[seq] : null;
                var timer = new Timer ();
                timer.SetTimeout (() => {
                    if (val != null) val.sendFail ((int) QAppProtoErrCode.EcSdkSendFail, "数据长度超限");
                }, 0);
                return seq;
            }

            var reqData = BuildData (data);

            return this.Send (reqData, seq, (ProtoCmd) subcmd);
        }

        private static byte[] BuildData (byte[] data) {
            return BuildData ((byte) MessageDataTag.ClientPre, data, (byte) MessageDataTag.ClientEnd);
        }

        // 接收响应并处理
        public void HandleMessage (byte[] body) {
            try {
                var rsp = Pb.DecodeRsp (body);
                var seq = rsp.RspWrap1.Seq;

                var val = SendQueue.ContainsKey (seq) ? SendQueue[seq] : null;

                var callback = val?.response;

                if (val == null) return;
                // 处理错误码，并拦截 value.response

                EventUpload.PushRequestEvent (new ReqEventParam { RqCmd = val.Cmd, RqSq = rsp.RspWrap1.Seq, RqCd = rsp.RspWrap1.ErrCode, Time = Convert.ToInt64 ((DateTime.Now - val.Time).TotalMilliseconds) });

                // 心跳不拦截
                if (val.Cmd != (int) ProtoCmd.ECmdHeartBeatReq && HandleErrCode (rsp.RspWrap1)) {
                    return;
                }

                callback?.Invoke (rsp);
                return;
            } catch (Exception e) {
                Debugger.Log (e.ToString ());
            }
        }

        // 处理登录失败
        private void HandleTokenErr () {
            // 重登录
            UserStatus.SetStatus (UserStatus.StatusType.Logout);
            this.Socket.Emit ("autoAuth", null);
        }

        // 处理checklogin connect失败
        private void HandleRelayConnectErr () {
            Debugger.Log ("handle relay connect err");
            // 重checklogin
            CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
            this.Socket.Emit ("autoAuth", null);

        }

        // 处理异常错误码
        // 返回 true 会拦截 responses 回调
        private bool HandleErrCode (ClientSendServerRspWrap1 res) {
            // Debugger.Log("handle errcode {0}", res.ErrCode);
            if (IsTokenError (res.ErrCode)) {
                this.HandleTokenErr ();
                Debugger.Log ("TOKEN_ERROR", res);
                return true;
            }

            if (IsRelayConnectError (res.ErrCode) && this.Socket.Id == (int) ConnectionType.Relay) {
                this.HandleRelayConnectErr ();
                Debugger.Log ("RELAY_CONNECT_ERROR", res);
                return true;
            }

            if (res.ErrCode != ErrCode.EcOk) {
                this._responses.Error (null);
            }

            return false;
        }

        private static bool IsTokenError (int errCode) {
            var res = errCode == ErrCode.EcAccessCmdGetTokenErr ||
                errCode == ErrCode.EcAccessCmdTokenPreExpire ||
                errCode == ErrCode.EcAccessCmdInvalidToken ||
                errCode == ErrCode.EcAccessGetCommConnectErr;

            return res;
        }

        private static bool IsRelayConnectError (int errCode) {
            var res = errCode == ErrCode.EcAccessGetRelayConnectErr;
            return res;
        }

        // 如果返回码正确
        public static void HandleSuccess (int code, Action callback) {
            if (code == (int) QAppProtoErrCode.EcOk) {
                callback ();
            }
        }
    }
}