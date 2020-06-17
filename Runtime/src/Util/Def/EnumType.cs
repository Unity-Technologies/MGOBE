

namespace com.unity.mgobe.src.Util.Def
{
    /**
     * @doc ENUM
     * @name 操作类型枚举
     * @enum {CreateRoomType}  CreateRoomType  创建房间方式
     * @enum {MatchType}  MatchType  匹配类型
     * @enum {NetworkState}  NetworkState  网络状态
     * @enum {FrameSyncState}  FrameSyncState  房间帧同步状态
     * @enum {SDKType.RecvType}  RecvType  消息接收者范围
     */
    public static class EnumType
    {
        public static CreateRoomType CreateRoomType { get; }

        public static MatchType MatchType { get; }

        public static RecvType RecvType { get; }

        public static FrameSyncState FrameSyncState { get; }

        public static NetworkState NetworkState { get; }
    }
}