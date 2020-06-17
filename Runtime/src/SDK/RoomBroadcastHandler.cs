using System;


namespace com.unity.mgobe.src.SDK
{
    public abstract class RoomBroadcastHandler {
        // 新玩家加入房间广播回调接口
        // 玩家退出房间广播回调接口
        // 房间被解散广播回调接口
        // 房主修改房间信息广播回调接口
        // 房间内玩家被移除
        // 收到房间内其他玩家消息广播回调接口
        // 收到自定义服务消息广播回调接口
        // 房间内玩家网络状态变化广播回调接口
        // 玩家自定义状态变化广播回调接口
        // 开始帧同步广播回调接口
        // 停止帧同步广播回调接口
        // 房间帧消息广播回调接口
        // 自动补帧失败广播回调接口
        // 匹配结束广播回调接口
        // 组队匹配超时广播

        public Action<BroadcastEvent> OnJoinRoom { get; set; }

        public Action<BroadcastEvent> OnLeaveRoom { get; set; }

        public Action<BroadcastEvent> OnDismissRoom { get; set; }

        public Action<BroadcastEvent> OnChangeRoom { get; set; }

        public Action<BroadcastEvent> OnRemovePlayer { get; set; }

        public Action<BroadcastEvent> OnRecvFromClient { get; set; }

        public Action<BroadcastEvent> OnRecvFromGameSvr { get; set; }

        public Action<BroadcastEvent> OnChangePlayerNetworkState { get; set; }

        public Action<BroadcastEvent> OnChangeCustomPlayerStatus { get; set; }

        public Action<BroadcastEvent> OnStartFrameSync { get; set; }

        public Action<BroadcastEvent> OnStopFrameSync { get; set; }

        public Action<BroadcastEvent> OnRecvFrame { get; set; }

        public Action<BroadcastEvent> OnAutoRequestFrameError { get; set; }

        public static Action<BroadcastEvent> OnMatch { get; set; }

        public static Action<BroadcastEvent> OnCancelMatch { get; set; }
    }
}