using System;
using com.unity.mgobe.src.Util.Def;
using com.unity.mgobe.src.Broadcast;
using com.unity.mgobe.src;


namespace com.unity.mgobe
{
    public class Listener
    {

        /// <summary>
        /// 初始化监听器。
        /// 该方法为静态方法。初始化 Listener 时需要传入 gameInfo 和 config 两个参数。
        /// 初始化结果在 callback 中异步返回，错误码为 0 表示初始化成功。
        /// </summary>
        /// <param name="gameInfo"> 游戏信息 </param>
        /// <param name="config"> 游戏配置 </param>
        /// <param name="callback"> 初始化回调函数 </param>
        public static void Init(GameInfoPara gameInfo, ConfigPara config, Action<ResponseEvent> callback)
        {
            Sdk.Instance = new Sdk(gameInfo, config);
            Sdk.Instance.Init(callback);
            // 绑定全局广播
		    Sdk.Instance.BindResponse(GlobalRoomBroadcast.Instance);
        }

        public static bool IsMe(string playerId)
        {
            return playerId == RequestHeader.PlayerId;
        }

        public static bool IsInited()
        {
            return Sdk.Instance.IsInited();
        }

        // Sdk Room 实例添加广播监听
        public static void Add(Room room)
        {
            room.RoomUtil.InitBroadcast();
            Sdk.Instance.BindResponse(room.RoomBroadcast);
        }

        // Sdk Room 实例移除广播监听
        public static void Remove(Room room)
        {
            Sdk.Instance.UnbindResponses(room.RoomBroadcast);
        }

        public static void Clear()
        {
            Sdk.Instance.ClearResponse();
        }
    }

}