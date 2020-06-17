using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe {
    /** Properties of a Frame. */
    [Serializable]
    public partial class Frame {
        public string RoomId { get; set; }
        public long Time { get; set; }
        public bool IsReplay { get; set; }
        public Frame (Frame frame, string id) {
            RoomId = id;
            Ext.Seed = frame.Ext.Seed;
            Id = frame.Id;
            Items.AddRange (frame.Items);
            Time = 0;
            IsReplay = false;
        }
    }
    /** Represents a Frame. */

    /** Properties of a PlayerInfo. */
    public partial class PlayerInfo {
        public static PlayerInfo CreateInstance (PlayerInfo info) {
            var playerInfo = new PlayerInfo {
                Id = info.Id,
                Name = info.Name,
                TeamId = info.TeamId,
                CustomPlayerStatus = info.CustomPlayerStatus,
                IsRobot = info.IsRobot

            };
            switch (info.RelayNetworkState) {
                case NetworkState.RelayOffline:
                    playerInfo.CommonNetworkState = NetworkState.RelayOffline;
                    break;
                case NetworkState.RelayOnline:
                    playerInfo.CommonNetworkState = NetworkState.RelayOnline;
                    break;
            }
            switch (info.CommonNetworkState) {
                case NetworkState.CommonOffline:
                    playerInfo.CommonNetworkState = NetworkState.CommonOffline;
                    break;
                case NetworkState.CommonOnline:
                    playerInfo.CommonNetworkState = NetworkState.CommonOnline;
                    break;
            }
            foreach (var attr in info.MatchAttributes) {
                var matchAttribute = new MatchAttribute (attr);
                playerInfo.MatchAttributes.Add (matchAttribute);
            };
            return playerInfo;
        }
    }
    /** 玩家信息meta */
    /** Properties of a TeamInfo. */
    public partial class TeamInfo {
        public static TeamInfo CreateInstance (TeamInfo info) {
            return new TeamInfo {
                Id = info.Id,
                    Name = info.Name,
                    MinPlayers = info.MinPlayers,
                    MaxPlayers = info.MaxPlayers
            };
        }
    }
    /** 队伍信息meta *
    /** Properties of a RoomInfo. */
    public partial class RoomInfo {
        public static RoomInfo CreateInstance (RoomInfo roomInfo) {
            var _roomInfo = new RoomInfo {
                Id = roomInfo.Id,
                Name = roomInfo.Name,
                Type = roomInfo.Type,
                CreateType = roomInfo.CreateType == CreateRoomType.CommonCreate ?
                CreateRoomType.CommonCreate :
                CreateRoomType.MatchCreate,
                MaxPlayers = roomInfo.MaxPlayers,
                Owner = roomInfo.Owner,
                IsPrivate = roomInfo.IsPrivate,
                CustomProperties = roomInfo.CustomProperties,
                FrameSyncState = roomInfo.FrameSyncState == FrameSyncState.Start ?
                FrameSyncState.Start :
                FrameSyncState.Stop,
                FrameRate = roomInfo.FrameRate,
                RouteId = roomInfo.RouteId,
                CreateTime = roomInfo.CreateTime,
                StartGameTime = roomInfo.StartGameTime,
                IsForbidJoin = roomInfo.IsForbidJoin
            };
            foreach (var info in roomInfo.PlayerList) {
                var playerInfo = new PlayerInfo (info);
                _roomInfo.PlayerList.Add (playerInfo);
            }

            foreach (var info in roomInfo.TeamList) {
                var teamInfo = new TeamInfo (info);
                _roomInfo.TeamList.Add (teamInfo);
            }

            return _roomInfo;
        }
    }
    /** 房间信息meta 

    /** 帧同步消息广播 */
    public class Signature {
        public Signature () {
            Timestamp = SdkUtil.GetCurrentTimeSeconds();
        }

        public string Sign { get; set; }

        public ulong Nonce { get; set; }

        public ulong Timestamp { get; set; }
    }
    public class BroadcastEvent {
        public BroadcastEvent (Object data, string seq) {
            this.Data = data;
            this.Seq = seq;
        }

        public object Data { get; set; }

        public string Seq { get; set; }
    }

    public class ResponseEvent {
        public ResponseEvent (int code) {
            this.Code = code;
            this.Msg = "";
            this.Seq = "";
            this.Data = null;
        }
        public ResponseEvent (int code, string msg, string seq, Object data) {
            this.Code = code;
            this.Msg = msg;
            this.Seq = seq;
            this.Data = data;
        }

        public int Code { get; set; }

        public string Msg { get; set; }

        public string Seq { get; set; }

        public object Data { get; set; }
    }

    public class InitRsp {
        public InitRsp (ulong serverTime) {
            this.ServerTime = serverTime;
        }

        public ulong ServerTime { get; set; }
    }

    // public delegate Action CreateSignature(Signature signature);

    public class ConfigPara {
        public int ReconnectMaxTimes { get; set; }

        public int ReconnectInterval { get; set; }

        public string Url { get; set; }

        public int ResendInterval { get; set; }

        public int ResendTimeout { get; set; }

        public bool IsAutoRequestFrame { get; set; }

        public string CacertNativeUrl { get; set; }
    }

    public class ChangeCustomPlayerStatusPara {
        public ulong CustomPlayerStatus { get; set; }
    }

    public class RequestFramePara {
        public long BeginFrameId { get; set; }

        public long EndFrameId { get; set; }
    }

    /**
     * @doc types.RecvType
     * @name 消息接收者类型
     * @field {1} ROOM_ALL 全部玩家
     * @field {2} ROOM_OTHERS 除自己外的其他玩家
     * @field {3} ROOM_SOME 房间中部分玩家
     */
    public enum RecvType {
        RoomAll = 1,
        RoomOthers = 2,
        RoomSome = 3
    }

    public class GameInfoPara {
        public string GameId { get; set; }

        public string OpenId { get; set; }

        public string SecretKey { get; set; }

        public Action CreateSignature { get; set; }
    }

    public class PlayerInfoPara {
        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }
    }

    public class CreateRoomPara {
        public string RoomName { get; set; }

        public string RoomType { get; set; }

        public uint MaxPlayers { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public PlayerInfoPara PlayerInfo { get; set; }
    }

    public class CreateTeamRoomPara {
        public string RoomName { get; set; }

        public string RoomType { get; set; }

        public uint MaxPlayers { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public PlayerInfoPara PlayerInfo { get; set; }

        public uint TeamNumber { get; set; }
    }

    public class JoinRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }
    }
    public class JoinTeamRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }

        public string TeamId { get; set; }

    }

    public class ChangeRoomPara {
        public string RoomName { get; set; }

        public string Owner { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public bool IsForbidJoin { get; set; }
    }

    public class RemovePlayerPara {
        public string RemovePlayerId { get; set; }
    }

    /// <summary>
    ///  获取房间列表参数
    /// </summary>
    public class GetRoomListPara {
        public int PageNo { get; set; }

        public int PageSize { get; set; }

        public string RoomType { get; set; } = "";

        public bool IsDesc { get; set; } = false;
    }

    /// <summary>
    ///  获取房间参数
    /// </summary>
    public class GetRoomByRoomIdPara {
        public string RoomId { get; set; }
    }

    public class MatchPlayersPara {
        /** 匹配的玩家信息 */
        private MatchPlayerInfoPara playerInfoPara;

        public string MatchCode { get; set; }

        /** 匹配的玩家信息 */
        public MatchPlayerInfoPara PlayerInfoPara {
            get => playerInfoPara;
            set => playerInfoPara = value;
        }
    }

    public class MatchPlayerInfoPara {
        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }

        public List<MatchAttribute> MatchAttributes { get; set; }
    }

    public class MatchRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }

        public ulong MaxPlayers { get; set; }

        public string RoomType { get; set; }
    }

    public class MatchGroupPlayerInfoPara {
        public string Id { get; set; }

        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }

        public List<MatchAttribute> MatchAttributes { get; set; }
    }

    public class MatchGroupPara {
        public List<MatchGroupPlayerInfoPara> PlayerInfoList { get; set; }

        public string MatchCode { get; set; }
    }

    public class MatchBst {
        public int ErrCode { get; set; }

        public RoomInfo RoomInfo { get; set; }
    }

    public partial class RecvFrameBst {
        public RecvFrameBst (Frame frame, string id) {
            Frame = new Frame (frame, id);
        }
    }

    public class CancelPlayerMatchPara {
        public MatchType MatchType { get; set; }
    }

    public class SendFramePara {
        public object Data { get; set; }
    }

    public class SendToClientPara {
        public List<string> RecvPlayerList { get; set; }

        public string Msg { get; set; }

        public RecvType RecvType { get; set; }
    }

    [Serializable]
    public class SendToGameSvrPara {
        public object Data { get; set; }
    }

}