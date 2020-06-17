using System;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Sender
{
    public class Sender : BaseNetUtil
    {
        private const ServerSendClientBstWrap2Type _messageBroadcastType = ServerSendClientBstWrap2Type.EPushTypeRoomChat;

        public Sender(Responses responses) : base(responses)
        {
            var bst = new BroadcastCallback(OnRecvFromClient);
            SetBroadcastHandler(_messageBroadcastType, bst);
        }

        ///////////////////////////////// 请求 //////////////////////////////////
        // 发送消息
        public string SendMessage(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdRoomChatReq;
            var response = new NetResponseCallback(SendMessageResponse);
            var seq = Send(para, subcmd, SendMessageResponse, callback);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////
        // 发送消息
        private void SendMessageResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {

            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.SendToClientRsp(eve);
            return;
        }

        ///////////////////////////////// 广播 //////////////////////////////////
        // 收到普通消息
        private void OnRecvFromClient(DecodeBstResult res, string seq)
        {
            var bst = new RecvFromClientBst();
            bst.MergeFrom(res.Body);
            var eve = new BroadcastEvent(bst, seq);
            var roomId = bst.RoomId;
            this.responses.OnRecvFromClient(roomId, eve);
            return;
        }
    }
}