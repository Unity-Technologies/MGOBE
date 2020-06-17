using System;
using System.Collections.Generic;

using com.unity.mgobe.src.Broadcast;
using com.unity.mgobe.src.EventUploader;

namespace com.unity.mgobe.src.Util {
    public class Responses {
        private static Responses _instance;
        private RoomBroadcast _roomBroadcast;
        private GlobalRoomBroadcast _globalBroadcast;
        private readonly HashSet<object> _context;

        private static Action<string> HandleRsp (ResponseEvent eve, HashSet<object> context) {
            return (string funcName) => {
                if ( string.IsNullOrEmpty(funcName)) return;
            };
        }

        // TODO: Response 广播回调，响应 Broadcast 事件
        private Action<string> HandleRsp (BroadcastEvent eve, HashSet<object> context) {
            return (string funcName) => {
                foreach (var response in context) {

                };
            };
        }

        public Responses () {
            if (Responses._instance != null) return;
            Responses._instance = this;
            _context = new HashSet<object> ();
        }

        /**
         * 创建房间响应
         */
        public static void CreateRoomRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("createRoomRsp");
        }

        /**
         * 加入房间响应
         */
        public static void JoinRoomRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("joinRoomRsp");
        }

        /**
         * 退出房间响应
         */
        public static void LeaveRoomRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("leaveRoomRsp");
        }

        /**
         * 解散房间响应
         */
        public static void DismissRoomRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("dismissRoomRsp");
        }
        /**
         * 修改房间响应
         */
        public void ChangeRoomRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("changeRoomRsp");
        }
        /**
         * 踢人响应
         */
        public static void RemovePlayerRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("removePlayerRsp");
        }

        /**
         * 获取房间详情响应
         */
        public static void GetRoomByRoomIdRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("getRoomByRoomIdRsp");
        }

        /**
         * 获取房间列表响应
         */
        public static void GetRoomListRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("getRoomListRsp");
        }

        /**
         * 多人匹配响应
         */
        public void MatchPlayersSimpleRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("matchPlayersSimpleRsp");
        }

        /**
         * 多人复杂匹配响应
         */
        public static void MatchPlayersRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("matchPlayersRsp");
        }

        /**
         * 组队匹配响应
         */
        public static void MatchGroupRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("matchGroupRsp");
        }

        /**
         * 房间匹配响应
         */
        public static void MatchRoomSimpleRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("matchRoomSimpleRsp");
        }

        /**
         * 取消匹配响应
         */
        public static void CancelPlayerMatchRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("cancelPlayerMatchRsp");
        }

        /**
         * 开始帧同步响应
         */
        public static void StartFrameSyncRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("startFrameSyncRsp");
        }

        /**
         * 停止帧同步响应
         */
        public static void StopFrameSyncRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("stopFrameSyncRsp");
        }

        /**
         * 发送帧同步数据响应
         */
        public static void SendFrameRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("sendFrameRsp");
        }

        /**
         * 绑定响应回调处理方法
         * context 应该实现除 bindResponse 外全部 Response 公共方法
         * @param context 响应回调处理
         */
        public static void BindResponse (RoomBroadcast roomBroadcast) {
            Responses._instance._roomBroadcast = roomBroadcast;
        }

        public static void BindResponse (GlobalRoomBroadcast roomBroadcast) {
            Responses._instance._globalBroadcast = roomBroadcast;
        }

        public static void UnbindResponse (RoomBroadcast roomBroadcast) {
            Responses._instance._roomBroadcast = null;
        }

        public static void ClearResponse () {
            Responses._instance._roomBroadcast = null;
            // Responses._instance._globalBroadcast = null;
        }

        /**
         * 房间内发送消息响应
         */
        public static void SendToClientRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("sendToClientRsp");
        }

        /**
         * 修改玩家状态响应
         */
        public static void ChangeCustomPlayerStatusRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("changeCustomPlayerStatusRsp");
        }

        /**
         * 收到帧同步消息
         */
        public static void RequestFrameRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("requestFrameRsp");
        }

        /**
         * 发自定义服务消息
         */
        public static void SendToGameSvrRsp (ResponseEvent eve) {
            HandleRsp (eve, Responses._instance._context) ("sendToGameSvrRsp");
        }

        /**
         * 本地网络状态变化
         */
        public void OnNetwork (ResponseEvent eve) {
            _roomBroadcast?.OnNetwork (eve);
        }

        /**
         * 玩家加入房间广播
         */
        public void OnJoinRoom (BroadcastEvent eve) {
            _roomBroadcast?.OnJoinRoom (eve);
        }

        /**
         * 玩家退出房间广播
         */
        public void OnLeaveRoom (BroadcastEvent eve) {
            _roomBroadcast?.OnLeaveRoom (eve);
        }

        /**
         * 玩家解散房间广播
         */
        public void OnDismissRoom (BroadcastEvent eve) {
            _roomBroadcast?.OnDismissRoom (eve);
        }

        /**
         * 玩家修改房间广播
         */
        public void OnChangeRoom (BroadcastEvent eve) {
            _roomBroadcast?.OnChangeRoom (eve);
        }

        /**
         * 玩家被踢广播
         */
        public void OnRemovePlayer (BroadcastEvent eve) {
            _roomBroadcast?.OnRemovePlayer (eve);
        }

        /**
         * 收到消息广播
         */
        public void OnRecvFromClient (string roomId, BroadcastEvent eve) {
            _roomBroadcast?.OnRecvFromClient (roomId, eve);
        }

        /**
         * 自定义服务广播
         */
        public void OnRecvFromGameSvr (string roomId, BroadcastEvent eve) {
            _roomBroadcast?.OnRecvFromGameSvr (roomId, eve);
        }

        /**
         * 匹配成功广播
         */
        public void OnMatchPlayers (BroadcastEvent eve) {
            Debugger.Log("onmatch player");
            _roomBroadcast?.OnMatchPlayers (eve);
           _globalBroadcast?.OnMatchPlayers(eve);

        }

        /**
         * 匹配超时广播
         */
        public void OnMatchTimeout (BroadcastEvent eve) {
            Debugger.Log("on match timeout");
            _roomBroadcast?.OnMatchTimeout (eve);
           _globalBroadcast?.OnMatchTimeout(eve);
        }

        /**
         * 取消组队匹配广播
         */
        public void OnCancelMatch (BroadcastEvent eve) {
           _globalBroadcast?.OnCancelMatch(eve);
        }

        /**
         * 玩家网络状态变化广播
         */
        public void OnChangePlayerNetworkState (BroadcastEvent eve) {
            _roomBroadcast?.OnChangePlayerNetworkState (eve);
        }

        /**
         * 玩家修改玩家状态广播
         */
        public void OnChangeCustomPlayerStatus (BroadcastEvent eve) {
            _roomBroadcast?.OnChangeCustomPlayerStatus (eve);
        }

        /**
         * 开始游戏广播
         */
        public void OnStartFrameSync (BroadcastEvent eve) {
            _roomBroadcast?.OnStartFrameSync (eve);
        }

        /**
         * 结束游戏广播
         */
        public void OnStopFrameSync (BroadcastEvent eve) {
            _roomBroadcast?.OnStopFrameSync (eve);
        }

        /**
         * 收到帧同步消息
         */
        public void OnRecvFrame (BroadcastEvent eve) {
            _roomBroadcast?.OnRecvFrame (eve);
        }

        /**
         * 监听全部响应的错误码
         * @param {SDKType.ResponseEvent<any>} event
         */
        public void Error (BroadcastEvent eve) {
            _roomBroadcast?.Error (eve);
        }
    }
}