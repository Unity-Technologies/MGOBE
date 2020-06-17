using System;
using System.Collections.Generic;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Room
{
    public class Room : BaseNetUtil
    {
        private readonly ServerSendClientBstWrap2Type _joinRoomBroadcastType = ServerSendClientBstWrap2Type.EPushTypeJoinRoom;
        private readonly ServerSendClientBstWrap2Type _leaveRoomBroadcastType = ServerSendClientBstWrap2Type.EPushTypeLeaveRoom;
        private readonly ServerSendClientBstWrap2Type _dismissRoomBroadcastType = ServerSendClientBstWrap2Type.EPushTypeDismissRoom;
        private readonly ServerSendClientBstWrap2Type _changeRoomBroadcastType = ServerSendClientBstWrap2Type.EPushTypeModifyRoomProperty;
        private readonly ServerSendClientBstWrap2Type _removeUserBroadcastType = ServerSendClientBstWrap2Type.EPushTypeRemovePlayer;
        private readonly ServerSendClientBstWrap2Type _changeUserStateBroadcastType = ServerSendClientBstWrap2Type.EPushTypePlayerState;
        private readonly ServerSendClientBstWrap2Type _roomUserNetworkBroadcastType = ServerSendClientBstWrap2Type.EPushTypeNetworkState;
        private readonly ServerSendClientBstWrap2Type _testBroadcastType = ServerSendClientBstWrap2Type.EPushTypeTest;

        public Room(Responses responses) : base(responses)
        {
            // 注册广播
            this.SetBroadcastHandler(this._joinRoomBroadcastType, this.OnJoinRoom);
            this.SetBroadcastHandler(this._leaveRoomBroadcastType, this.OnLeaveRoom);
            this.SetBroadcastHandler(this._dismissRoomBroadcastType, this.OnDismissRoom);
            this.SetBroadcastHandler(this._changeRoomBroadcastType, this.OnChangeRoom);
            this.SetBroadcastHandler(this._removeUserBroadcastType, this.OnRemoveUser);
            this.SetBroadcastHandler(this._changeUserStateBroadcastType, this.OnChangeUserState);
            this.SetBroadcastHandler(this._roomUserNetworkBroadcastType, this.OnChangePlayerNetworkState);
            this.SetBroadcastHandler(this._testBroadcastType, TestBroadcast);
        }

        ///////////////////////////////// 请求 //////////////////////////////////

        // 创建房间
        public string CreateRoom(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdCreateRoomReq;
            var response = new NetResponseCallback(CreateRoomResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("CREATEROOM_Para", para, seq);
            return seq;
        }
        // 加入房间
        public string JoinRoom(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdJoinRoomReq;
            var response = new NetResponseCallback(JoinRoomResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("JOINROOM_Para", para, seq);
            return seq;
        }
        // 离开房间
        public string LeaveRoom(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdQuitRoomReq;
            var response = new NetResponseCallback(LeaveRoomResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("LEAVEROOM_Para", para, seq);
            return seq;
        }
        // 解散房间
        public string DismissRoom(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdDissmissRoomReq;
            var response = new NetResponseCallback(DismissRoomResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("DISMISSROOM_Para", para, seq);
            return seq;
        }

        // 房间变更
        public string ChangeRoom(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdChangeRoomPropertisReq;
            var response = new NetResponseCallback(ChangeRoomResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("CHANGEROOM_Para", para, seq);
            return seq;
        }
        // 移除房间内玩家
        public string RemoveUser(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdRemoveMemberReq;
            var response = new NetResponseCallback(RemoveUserResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("REMOVEUSER_Para", para, seq);
            return seq;
        }
        // 查询房间详情
        public string GetRoomByRoomId(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdGetRoomDetailReq;
            var response = new NetResponseCallback(GetRoomByRoomIdRsp);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("GET_ROOM_BY_ROOMID", para, seq);
            return seq;
        }
        // 查询房间列表
        public string GetRoomList(ByteString para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdGetRoomListV2Req;
            var response = new NetResponseCallback(GetRoomListResponse);
            var seq = this.Send(para, subcmd, response, callback);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////

        // 创建房间
        private void CreateRoomResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.CreateRoomRsp(eve);
            return;
        }

        // 加入房间
        private void JoinRoomResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.JoinRoomRsp(eve);
            return;
        }

        // 离开房间
        private void LeaveRoomResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.LeaveRoomRsp(eve);
            return;
        }
        // 解散房间
        private void DismissRoomResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.DismissRoomRsp(eve);
            return;
        }
        // 房间变更
        private void ChangeRoomResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            responses.ChangeRoomRsp(eve);
            return;
        }


        // 踢人操作
        private void RemoveUserResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.RemovePlayerRsp(eve);
            return;
        }

        // 查询房间详情
        private void GetRoomByRoomIdRsp(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.GetRoomByRoomIdRsp(eve);
            return;
        }

        // 查询房间列表
        private void GetRoomListResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            callback?.Invoke(eve);
            Responses.GetRoomListRsp(eve);
            return;
        }

        ////////////////////////////////////// 广播  /////////////////////////////////////////
        private void OnJoinRoom(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onJoinRoom bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnJoinRoom(eve);
        }

        private void OnLeaveRoom(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onLeaveRoom bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnLeaveRoom(eve);
        }

        private void OnDismissRoom(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onDismissRoom bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnDismissRoom(eve);
        }

        private void OnChangeRoom(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onChangeRoom bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnChangeRoom(eve);
        }

        private void OnRemoveUser(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onRemoveUser bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnRemovePlayer(eve);
        }

        private void OnChangeUserState(DecodeBstResult bst, string seq)
        {
            Debugger.Log("onChangeUserState bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnChangeCustomPlayerStatus(eve);
        }

        private void OnChangePlayerNetworkState(DecodeBstResult bst, string seq)
        {
            // Debugger.Log("onChangePlayerNetworkState bst: {0}", seq);
            var eve = new BroadcastEvent(bst.Body, seq);
            this.responses.OnChangePlayerNetworkState(eve);
        }

        private static void TestBroadcast(DecodeBstResult bst, string seq)
        {
            Debugger.Log("testBroadcast bst: {0}", seq);
        }

    }
}
