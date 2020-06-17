using System;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;



namespace com.unity.mgobe.src.Matcher {
    public class Matcher : BaseNetUtil {
        private const ServerSendClientBstWrap2Type MatchTimeoutBroadcastType = ServerSendClientBstWrap2Type.EPushTypeMatchTimeout;
        private const ServerSendClientBstWrap2Type MatchUsersBroadcastType = ServerSendClientBstWrap2Type.EPushTypeMatchSuccess;
        private const ServerSendClientBstWrap2Type CancelMatchBroadcastType = ServerSendClientBstWrap2Type.EPushTypeMatchCancel;

        public Matcher (Responses responses) : base (responses) {
            // 注册广播
            // BroadcastCallback matchTimeoutBst = new BroadcastCallback(this.matchUsersTimeoutBroadcast);
            // BroadcastCallback matchUserstBst = new BroadcastCallback(this.matchUsersBroadcast);

            this.SetBroadcastHandler (MatchTimeoutBroadcastType, this.MatchUsersTimeoutBroadcast);
            this.SetBroadcastHandler (MatchUsersBroadcastType, this.MatchUsersBroadcast);
            this.SetBroadcastHandler (CancelMatchBroadcastType, this.CancelMatchBroadcast);

        }

        ///////////////////////////////// 请求 //////////////////////////////////

        // 多人复杂匹配
        public string MatchUsersComplex (ByteString para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdMatchPlayerComplexReq;
            var response = new NetResponseCallback (MatchUsersComplexResponse);
            var seq = this.Send (para, subcmd, response, callback);
            Debugger.Log ("MATCHUSERSCOMPLEX_Para", para, seq);
            return seq;
        }
        // 组队匹配
        public string MatchGroup (ByteString para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdMatchGroupReq;
            var response = new NetResponseCallback (MatchGroupResponse);
            var seq = this.Send (para, subcmd, response, callback);
            Debugger.Log ("MATCH_GROUP_Para", para, seq);
            return seq;
        }
        // 房间匹配
        public string MatchRoom (ByteString para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdMatchRoomSimpleReq;
            var response = new NetResponseCallback (MatchRoomResponse);
            var seq = this.Send (para, subcmd, response, callback);
            Debugger.Log ("MATCHROOM_Para", para, seq);
            return seq;
        }
        // 取消匹配
        public string CancelMatch (ByteString para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdMatchCancelMatchReq;
            var response = new NetResponseCallback (CancelMatchResponse);
            var seq = this.Send (para, subcmd, response, callback);
            Debugger.Log ("CANCELMATCH_Para", para, seq);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////

        // 多人复杂匹配
        private void MatchUsersComplexResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke (eve);
            Responses.MatchPlayersRsp (eve);
            return;
        }

        // 组队匹配
        private void MatchGroupResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke (eve);
            Responses.MatchGroupRsp (eve);
            return;
        }

        // 房间匹配
        private void MatchRoomResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke (eve);
            Responses.MatchRoomSimpleRsp (eve);
            return;
        }

        // 取消匹配
        private void CancelMatchResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke (eve);
            Responses.CancelPlayerMatchRsp (eve);
            return;
        }

        ////////////////////////////////////// 广播  /////////////////////////////////////////
        private void MatchUsersTimeoutBroadcast (DecodeBstResult bst, string seq) {
            var eve = new BroadcastEvent (bst.Body, seq);
            this.responses.OnMatchTimeout (eve);
        }

        private void MatchUsersBroadcast (DecodeBstResult bst, string seq) {
            var eve = new BroadcastEvent (bst.Body, seq);
            this.responses.OnMatchPlayers (eve);
        }

        private void CancelMatchBroadcast (DecodeBstResult bst, string seq) {
            var eve = new BroadcastEvent (bst.Body, seq);
            this.responses.OnCancelMatch (eve);
        }
    }
}