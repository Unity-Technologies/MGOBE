using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;

using com.unity.mgobe.src.Broadcast;
using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Net.Sockets;
using com.unity.mgobe.src.Sender;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;
using UnityEngine;
// using System.Text.Json.Serialization;

namespace com.unity.mgobe.src {
    public class SdKonfig {
        private static int _pingInterval = 5000;
        private static int _reportInterval = 10000;
        private static long _serverTime = 0;
        private static bool _enableUdp = false;
        public static string Version { get; set; } = "1.2.6.1";
    }

    public class Sdk {
        /// <summary>
        /// 唯一实例对象
        /// </summary>
        private static Sdk instance = null;

        private Socket _socket1 = null;
        private Socket _socket2 = null;
        private FrameSender _frameSender = null;

        private Action<ResponseEvent> _initRspCallback;

        // public SDKType.RoomInfo roomInfo;

        public Sdk (GameInfoPara gameInfo, ConfigPara config) {
            var task = Task.Run (() => SdkUtil.UploadMgobeUserInfo (gameInfo.GameId));
            if (Instance != null) return;
            Instance = this;

            // 合并游戏信息
            GameInfo.Assign (gameInfo);
            Config.Assign (config);
            RequestHeader.GameId = gameInfo.GameId;
        }

        /// <summary>
        /// 唯一实例对象
        /// </summary>
        public static Sdk Instance {
            get => instance;
            set => instance = value;
        }

        public static Responses Responses { get; } = new Responses ();

        public static ErrCode ErrCode { get; set; }
        public static void Uninit () {
            Sdk.Instance = null;
        }

        public static void UpdateSdk () {
            Instance._frameSender = Core.FrameSender;
            Instance._socket1 = Core.Socket1;
            Instance._socket2 = Core.Socket2;
        }

        public void BindResponse (RoomBroadcast roomBroadcast) {
            Responses.BindResponse (roomBroadcast);
        }

        public void BindResponse (GlobalRoomBroadcast roomBroadcast) {
            Responses.BindResponse (roomBroadcast);
        }

        public void UnbindResponses (RoomBroadcast broadcast) {
            Responses.UnbindResponse (broadcast);
        }

        public void ClearResponse () {
            Responses.ClearResponse ();
        }

        public Socket GetSocket (ConnectionType type) {
            switch (type) {
                case ConnectionType.Common:
                    return _socket1;
                case ConnectionType.Relay:
                    return _socket2;
                default:
                    return null;
            }
        }

        public void Init (Action<ResponseEvent> callback) {
            // 初始化成功修改playerid
            this._initRspCallback = callback;
            Core.InitSdk ();
        }

        /**
         * sdk 初始化回调
         */
        public void InitRsp (ResponseEvent eve) {
            this._initRspCallback (eve);
        }

        public bool IsInited () {
            return SdkStatus.IsInited ();
        }

        /// <summary>
        /// 修改玩家状态
        /// </summary>
        public static void ChangeCustomPlayerStatus (ChangeCustomPlayerStatusPara para, Action<ResponseEvent> callback) {
            var req = new ChangeCustomPlayerStatusReq {
                CustomPlayerStatus = para.CustomPlayerStatus
            };
            Core.User.ChangeUserState (req, callback);
        }

        /// 创建房间
        public void CreateRoom (CreateRoomPara para, Action<ResponseEvent> callback) {
            CreateTeamRoom (new CreateTeamRoomPara {
                RoomType = para.RoomType,
                    RoomName = para.RoomName,
                    MaxPlayers = para.MaxPlayers,
                    IsPrivate = para.IsPrivate,
                    CustomProperties = para.CustomProperties,
                    TeamNumber = 1,
                    PlayerInfo = para.PlayerInfo
            }, callback);
        }

        /// <summary>
        /// 创建团队房间
        /// </summary>
        public static void CreateTeamRoom (CreateTeamRoomPara para, Action<ResponseEvent> callback) {
            if (para == null) para = new CreateTeamRoomPara ();
            var maxUsers = para.MaxPlayers;
            var teamNumber = para.TeamNumber;

            if (teamNumber != 0 && maxUsers % teamNumber != 0) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcParamsInvalid, "参数错误，最大玩家数无法被队伍数量整除", "", null));
                return;
            }

            var info = new PlayerInfo {
                Id = RequestHeader.PlayerId,
                CustomPlayerStatus = para.PlayerInfo.CustomPlayerStatus,
                CustomProfile = para.PlayerInfo.CustomProfile,
                Name = para.PlayerInfo.Name
            };
            var req = new CreateRoomReq {
                PlayerInfo = info,
            };

            var teamList = new TeamInfo[teamNumber];
            if (maxUsers >= teamNumber && teamNumber >= 1) {
                var playerNum = maxUsers / teamNumber;
                for (var i = 0; i < teamNumber; i++) {
                    var teamInfo = new TeamInfo {
                        Id = i + "",
                        MaxPlayers = playerNum,
                        MinPlayers = 1,
                        Name = ""
                    };

                    if (info.TeamId != null) info.TeamId = teamInfo.Id;

                    if (i == teamNumber - 1) {
                        teamInfo.MaxPlayers = maxUsers - (teamNumber - 1) * playerNum;
                    }

                    teamList[i] = teamInfo;
                }

            } else {
                if (callback != null) {
                    callback (new ResponseEvent (ErrCode.EcParamsInvalid, "参数错误，请检查最大玩家数量和队伍数量", "", null));
                    return;
                }
            }

            var createRoomReq = new CreateRoomReq {
                RoomType = para.RoomType,
                RoomName = para.RoomName,
                MaxPlayers = para.MaxPlayers,
                IsPrivate = para.IsPrivate,
                CustomProperties = para.CustomProperties,
                CreateType = CreateRoomType.CommonCreate,
                PlayerInfo = info
            };
            createRoomReq.TeamList.AddRange (teamList);
            Core.Room.CreateRoom (createRoomReq.ToByteString (), callback);
        }

        /// <summary>
        /// 加入房间 
        /// </summary>
        public static void JoinRoom (JoinRoomPara para, string roomId, Action<ResponseEvent> callback) {
            var req = new JoinRoomReq {
                PlayerInfo = new PlayerInfo {
                Name = para.PlayerInfo.Name,
                CustomProfile = para.PlayerInfo.CustomProfile,
                CustomPlayerStatus = para.PlayerInfo.CustomPlayerStatus,
                Id = RequestHeader.PlayerId,
                },
                TeamId = "0",
                JoinType = JoinRoomType.CommonJoin,
                RoomId = roomId ?? ""
            };
            Core.Room.JoinRoom (req.ToByteString (), callback);
        }

        // 加入团队房间
        public static void JoinTeamRoom (JoinTeamRoomPara para, string roomId, Action<ResponseEvent> callback) {
            // Debugger.Log ("join team room para: {0} {1} {2}", para.teamId, JoinRoomType.CommonJoin, para.roomId);
            var req = new JoinRoomReq {
                PlayerInfo = new PlayerInfo {
                Name = para.PlayerInfo.Name,
                CustomProfile = para.PlayerInfo.CustomProfile,
                CustomPlayerStatus = para.PlayerInfo.CustomPlayerStatus,
                Id = RequestHeader.PlayerId,
                },
                TeamId = para.TeamId,
                JoinType = JoinRoomType.CommonJoin,
                RoomId = roomId ?? ""
            };
            Core.Room.JoinRoom (req.ToByteString (), callback);
        }

        // 退出房间
        public static void LeaveRoom (Action<ResponseEvent> callback) {
            var req = new LeaveRoomReq ();
            Core.Room.LeaveRoom (req.ToByteString (), callback);
        }

        // 解散房间
        public static void DismissRoom (Action<ResponseEvent> callback) {
            var req = new DismissRoomReq ();
            Core.Room.DismissRoom (req.ToByteString (), callback);
        }

        public static void ChangeRoom (ChangeRoomPara roomPara, RoomInfo roomInfo, Action<ResponseEvent> callback) {
            var req = new ChangeRoomReq {
                RoomName = roomPara.RoomName ?? "",
                Owner = roomPara.Owner ?? "",
                IsPrivate = roomPara.IsPrivate,
                CustomProperties = roomPara.CustomProperties ?? "",
                IsForbidJoin = roomPara.IsForbidJoin
            };
            if (req.RoomName != "") req.ChangeRoomOptionList.Add (ChangeRoomOption.RoomName);
            if (req.Owner != "") req.ChangeRoomOptionList.Add (ChangeRoomOption.Owner);
            if (req.CustomProperties != "") req.ChangeRoomOptionList.Add (ChangeRoomOption.CustomProperties);
            req.ChangeRoomOptionList.Add (ChangeRoomOption.IsForbidJoin);
            req.ChangeRoomOptionList.Add (ChangeRoomOption.IsPrivate);
            Core.Room.ChangeRoom (req.ToByteString (), callback);
        }

        // 移除房间内玩家
        public static void RemovePlayer (RemovePlayerPara para, Action<ResponseEvent> callback) {
            var req = new RemovePlayerReq {
                RemovePlayerId = para.RemovePlayerId
            };
            Core.Room.RemoveUser (req.ToByteString (), callback);
        }

        // 多人复杂匹配
        public static void MatchPlayers (MatchPlayersPara para, Action<ResponseEvent> callback) {
            var req = new MatchPlayersReq {
                PlayerInfo = new MatchPlayerInfo {
                TeamId = "",
                Region = "",
                TeamLeader = "",
                Id = RequestHeader.PlayerId ?? "",
                Name = para.PlayerInfoPara.Name ?? "",
                CustomProfile = para.PlayerInfoPara.CustomProfile ?? "",
                CustomPlayerStatus = para.PlayerInfoPara.CustomPlayerStatus,
                },
                MatchCode = para.MatchCode ?? ""
            };
            foreach (var item in para.PlayerInfoPara.MatchAttributes) {
                req.PlayerInfo.MatchAttributes.Add (new MatchAttribute {
                    Name = item.Name,
                        Value = item.Value
                });
            }

            Core.Matcher.MatchUsersComplex (req.ToByteString (), callback);
        }

        // 组队匹配
        public static void MatchGroup (MatchGroupPara matchGroupPara, Action<ResponseEvent> callback) {
            var req = new MatchGroupReq {
                MatchCode = matchGroupPara.MatchCode,
            };
            foreach (var item in matchGroupPara.PlayerInfoList) {
                var matchPlayerInfoPara = new MatchGroupPlayerInfo {
                    Id = item.Id,
                    Name = item.Name,
                    CustomPlayerStatus = item.CustomPlayerStatus,
                    CustomProfile = item.CustomProfile
                };
                foreach (var attr in item.MatchAttributes) {
                    matchPlayerInfoPara.MatchAttributes.Add (new MatchAttribute {
                        Name = attr.Name,
                            Value = attr.Value
                    });
                }
                req.PlayerInfoList.Add (matchPlayerInfoPara);
            }
            Core.Matcher.MatchGroup (req.ToByteString (), callback);
        }

        // 房间匹配
        public static void MatchRoom (MatchRoomPara para, Action<ResponseEvent> callback) {
            var req = new MatchRoomSimpleReq {
                PlayerInfo = new PlayerInfo {
                Id = RequestHeader.PlayerId,
                Name = para.PlayerInfo.Name,
                CustomProfile = para.PlayerInfo.CustomProfile,
                CustomPlayerStatus = para.PlayerInfo.CustomPlayerStatus,
                },
                RoomType = para.RoomType,
                MaxPlayers = para.MaxPlayers
            };
            Core.Matcher.MatchRoom (req.ToByteString (), callback);
        }

        // 取消匹配
        public static void CancelMatch (CancelPlayerMatchPara para, Action<ResponseEvent> callback) {
            var req = new CancelPlayerMatchReq {
                MatchType = para.MatchType
            };
            Core.Matcher.CancelMatch (req.ToByteString (), callback);
        }

        /// <summary>
        /// 获取房间列表
        /// </summary>
        public static void GetRoomList (GetRoomListPara getRoomListPara, Action<ResponseEvent> callback) {
            if (getRoomListPara.PageNo < 0) {
                callback?.Invoke (new ResponseEvent (61017, "PageNo 参数不合法，请确认", "", null));
                return;
            } else if (getRoomListPara.PageSize < 0) {
                callback?.Invoke (new ResponseEvent (61018, "PageSize 参数不合法，请确认", "", null));
            }
            var para = new GetRoomListReq {
                GameId = GameInfo.GameId,
                PageNo = Convert.ToUInt32 (getRoomListPara.PageNo),
                PageSize = Convert.ToUInt32 (getRoomListPara.PageSize),
                IsDesc = getRoomListPara.IsDesc,
                RoomType = getRoomListPara.RoomType
            };
            Core.Room.GetRoomList (para.ToByteString (), callback);
        }

        /// <summary>
        /// 获取房间信息
        /// </summary>
        public static void GetRoomByRoomId (GetRoomByRoomIdPara getRoomByRoomIdPara, Action<ResponseEvent> callback) {
            var para = new GetRoomByRoomIdReq {
                RoomId = getRoomByRoomIdPara.RoomId
            };
            Core.Room.GetRoomByRoomId (para.ToByteString (), callback);
        }

        // 设置帧同步房间
        public bool SetFrameRoom (RoomInfo roomInfo) {
            if (roomInfo?.PlayerList == null) return false;
            if (roomInfo.PlayerList.All (info => !info.Id.Equals (RequestHeader.PlayerId))) return false;
            _frameSender.SetFrameRoom (roomInfo);
            return true;
        }

        // 开始帧同步
        public void StartFrameSync (Action<ResponseEvent> callback) {
            var roomInfo = _frameSender?.RoomInfo;
            if (roomInfo == null) {
                StartFrameSyncFailRsp (new ResponseEvent (ErrCode.EcRoomPlayerNotInRoom), callback);
                return;
            }

            void StartFrame () {
                _frameSender.CheckLogin (eve => {
                    if (eve.Code == ErrCode.EcOk) {
                        Debugger.Log ("STARTFRAMESYNC start {0}", GameInfo.GameId);
                        var req = new StartFrameSyncReq { RoomId = roomInfo.Id, GameId = GameInfo.GameId };
                        _frameSender.StartFrameSync (req, callback);
                    } else {
                        Debugger.Log ("STARTFRAMESYNC fail at CheckLogin, seq= {0}, code={1} {2}", eve.Seq, eve.Code,
                            roomInfo);
                        StartFrameSyncFailRsp (
                            new ResponseEvent (ErrCode.EcSdkNoCheckLogin, "CheckLogin失败, seq=" + eve.Seq, null, null),
                            callback);
                    }
                }, "sdk startFrame");
            }

            _socket2.Url = Config.Url + ":" + Port.GetRelayPort ();
            if (!_socket2.IsSocketStatus ("connect")) {
                _socket2.ConnectSocketTask ("SDK startFrameSync");
                _socket2.EventOnceHandlers.Clear ();
                _socket2.OnceEvent ("connect", (SocketEvent e) => {
                    // 清空事件列表
                    _socket2.EventOnceHandlers.Clear ();
                    StartFrame ();
                });
                _socket2.OnceEvent ("connectClose", (SocketEvent e) => {
                    Debugger.Log ("STARTFRAMESYNC fail at SocketEventType.connectClose");
                    _socket2.EventOnceHandlers.Clear ();
                    StartFrameSyncFailRsp (new ResponseEvent (ErrCode.EcSdkSocketError, "Socket错误", null, null),
                        callback);

                });
                _socket2.OnceEvent ("connectError", (SocketEvent e) => {
                    Debugger.Log ("STARTFRAMESYNC fail at SocketEventType.connectError");
                    _socket2.EventOnceHandlers.Clear ();
                    StartFrameSyncFailRsp (new ResponseEvent (ErrCode.EcSdkSocketError, "Socket错误", null, null),
                        callback);
                });
            } else {
                StartFrame ();
            }
        }

        private static void StartFrameSyncFailRsp (ResponseEvent eve, Action<ResponseEvent> callback) {
            callback?.Invoke (eve);
            Responses.StartFrameSyncRsp (eve);
        }

        // 禁止帧同步
        public void StopFrameSync (Action<ResponseEvent> callback) {
            var roomInfo = _frameSender.RoomInfo;
            if (roomInfo == null) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcRoomPlayerNotInRoom, "未找到帧同步房间，请确认", "", null));
                return;
            }
            var req = new StopFrameSyncReq {
                RoomId = roomInfo.Id,
                GameId = GameInfo.GameId
            };
            _frameSender.StopFrameSync (req, (eve) => {
                if (eve.Code == ErrCode.EcOk) callback?.Invoke (eve);
            });
        }

        // 发送帧同步数据
        public void SendFrame (SendFramePara para, Action<ResponseEvent> callback) {
            var roomInfo = _frameSender?.RoomInfo;
            if (roomInfo == null) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcRoomPlayerNotInRoom, "未找到帧同步房间，请确认", "", null));
                return;
            }

            var req = new SendFrameReq {
                RoomId = roomInfo.Id,
                Item = new FrameItem {
                PlayerId = RequestHeader.PlayerId,
                Data = para.Data.ToString (),
                Timestamp = Convert.ToUInt64 (SdkUtil.GetCurrentTimeMilliseconds ())
                }
            };
            _frameSender.SendFrame (req, callback);
        }

        // 请求补帧
        public void RequestFrame (RequestFramePara para, Action<ResponseEvent> callback) {
            var roomInfo = _frameSender?.RoomInfo;
            if (roomInfo == null) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcRoomPlayerNotInRoom, "未找到帧同步房间，请确认", "", null));
                return;
            }
            if (para.BeginFrameId < 0 || para.EndFrameId < 0) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcParamsInvalid, "非法参数，请确认", "", null));
                return;
            }

            var req = new RequestFrameReq {
                RoomId = roomInfo.Id,
                BeginFrameId = Convert.ToUInt64 (para.BeginFrameId),
                EndFrameId = Convert.ToUInt64 (para.EndFrameId),
            };
            _frameSender.RequestFrame (req, callback);
        }

        // 房间内发送信息
        public static void SendToClient (SendToClientPara para, string roomId, Action<ResponseEvent> callback) {
            // 如果玩家列表为空，直接回调成功
            if (para.RecvPlayerList.Count == 0) {
                callback?.Invoke (new ResponseEvent (ErrCode.EcOk, "", "", null));
                return;
            }

            var req = new SendToClientReq {
                RoomId = roomId,
                Msg = para.Msg,
            };
            req.RecvPlayerList.AddRange (para.RecvPlayerList);
            Core.Sender.SendMessage (req.ToByteString (), callback);
        }

        // 发自定义服务消息
        public void SendToGameSvr (SendToGameSvrPara para, string roomId, Action<ResponseEvent> callback) {
            var req = new SendToGameSvrReq {
                PlayerId = RequestHeader.PlayerId,
                RoomId = roomId,
                Data = JsonUtility.ToJson (para.Data)
            };
            _frameSender.SendMessageExt (req, callback);
        }
    }
}