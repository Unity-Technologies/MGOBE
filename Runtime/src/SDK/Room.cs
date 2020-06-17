using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;

using com.unity.mgobe.src.SDK;
using com.unity.mgobe.src.Broadcast;
using com.unity.mgobe.src;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;

using UnityEngine;

namespace com.unity.mgobe {
    /********************************* SDK Room对象 *********************************/
    public class Room : RoomBroadcastHandler {
        internal RoomUtil RoomUtil { get; }

        public RoomBroadcast RoomBroadcast { get; set; }

        public RoomInfo RoomInfo { get; set; }

        public Room (RoomInfo roomInfo) : base () {
            this.RoomUtil = new RoomUtil (this);
            this.RoomUtil.SetRoomInfo (roomInfo);
        }

        public static void GetRoomList (GetRoomListPara para, Action<ResponseEvent> callback) {
            Sdk.GetRoomList (para, (eve) => {
                try {
                    if (eve.Data != null) {
                        var rsp = new GetRoomListRsp ();
                        rsp.MergeFrom ((ByteString) eve.Data);
                        eve.Data = rsp;
                    }
                    callback?.Invoke (eve);
                } catch (Exception e) {
                    Debugger.Log ("{0}", e.ToString ());
                }
            });
        }

        public static void GetRoomByRoomId (GetRoomByRoomIdPara para, Action<ResponseEvent> callback) {
            Sdk.GetRoomByRoomId (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new GetRoomByRoomIdRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public static void GetMyRoom (Action<ResponseEvent> callback) {
            var para = new GetRoomByRoomIdPara {
                RoomId = ""
            };
            Sdk.GetRoomByRoomId (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new GetRoomByRoomIdRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public bool IsInRoom () {
            return this.RoomInfo.PlayerList != null && this.RoomInfo.PlayerList.Any (u => u.Id.Equals (RequestHeader.PlayerId));
        }

        /// <summary>
        /// 初始化 Room 实例的房间信息，即更新 roomInfo 属性
        /// initRoom 会更新 Room 实例的 roomInfo，接受 MGOBE.types.RoomInfo 或 { id: string; } 类型的参数。
        /// 如果不传参数，该方法将清空 Room 实例的 roomInfo 属性，此时调用 getRoomDetail 方法将查询玩家所在的房间。
        /// 当玩家需要加入指定 id 房间时，需要使用该接口初始化 Room 实例的 roomInfo 属性，然后才能通过调用 joinRoom 
        /// 方法加入该 Room 实例所代表的房间。
        /// </summary>
        /// <param name="roomInfo"></param>
        public void InitRoom (RoomInfo roomInfo) {
            this.RoomUtil.SetRoomInfo (roomInfo);
            if (roomInfo == null || string.IsNullOrEmpty (roomInfo.Id) || string.IsNullOrEmpty (roomInfo.RouteId) ||
                roomInfo.PlayerList == null) return;
            foreach (var info in roomInfo.PlayerList.Where (info => !string.IsNullOrEmpty (info.Id) && Listener.IsMe (info.Id))) {
                this.RoomUtil.ActiveFrame ();
            }
        }

        public void InitRoom (string id) {
            var roomInfo = new RoomInfo {
                Id = id
            };
            this.InitRoom (roomInfo);
        }

        /// <summary>
        /// 房间信息更新接口
        /// onUpdate 表明 Room 实例的 roomInfo 信息发生变化，这种变化原因包括各种房间操作、房间广播、本地网络状态变化等。
        /// 开发者可以在该接口中更新游戏画面，或者使用 networkState 属性判断网络状态。
        /// </summary>
        /// <param name="room"></param>
        public Action<Room> onUpdate = room => {

        };

        /// <summary>
        /// 获取客户端本地 SDK 网络状态
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool GetNetworkState (ConnectionType type) {
            var socket = Sdk.Instance.GetSocket (type);
            return socket != null && socket.IsSocketStatus ("connect");
        }

        /// <summary>
        /// 创建房间
        /// createRoom 调用结果将在 callback 中异步返回。操作成功后，roomInfo 属性将更新。
        /// 创建房间成功后，玩家自动进入该房间，因此无法继续调用 joinRoom、matchPlayers 等方法，可以利用房间ID邀请其他玩家进入该房间。
        /// </summary>
        /// <param name="SDKType.CreateRoomPara"></param>
        /// <param name="createRoomPara"></param>
        /// <param name="callback"></param>
        public void CreateRoom (CreateRoomPara createRoomPara, Action<ResponseEvent> callback) {
            Sdk.Instance.CreateRoom (createRoomPara, (eve) => {
                if (eve.Data != null) {
                    var rsp = new CreateRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /// <summary>
        /// /// 创建团队房间
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void CreateTeamRoom (CreateTeamRoomPara para, Action<ResponseEvent> callback) {
            Sdk.CreateTeamRoom (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new CreateRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /// <summary>
        /// 加入团队房间
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void JoinRoom (JoinRoomPara para, Action<ResponseEvent> callback) {
            Sdk.JoinRoom (para, this.RoomInfo.Id, (eve) => {
                if (eve.Data != null) {
                    var rsp = new JoinRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /// <summary>
        /// 加入团队房间
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void JoinTeamRoom (JoinTeamRoomPara para, Action<ResponseEvent> callback) {
            Sdk.JoinTeamRoom (para, this.RoomInfo.Id, (eve) => {
                if (eve.Data != null) {
                    var rsp = new JoinRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void LeaveRoom (Action<ResponseEvent> callback) {
            Sdk.LeaveRoom ((eve) => {
                if (eve.Data != null) {
                    var rsp = new LeaveRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /// <summary>
        /// 解散房间
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void DismissRoom (Action<ResponseEvent> callback) {
            void Eve (ResponseEvent e) {
                this.RoomUtil.SetRoomInfo (null);
                callback?.Invoke (e);
            }

            Sdk.DismissRoom (Eve);
        }

        /// <summary>
        /// 修改房间信息
        /// </summary>
        /// <param name="para"></param>
        /// <param name="callback"></param>
        public void ChangeRoom (ChangeRoomPara para, Action<ResponseEvent> callback) {
            Sdk.ChangeRoom (para, this.RoomInfo, (eve) => {
                if (eve.Data != null) {
                    var rsp = new ChangeRoomRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public void ChangeCustomPlayerStatus (ChangeCustomPlayerStatusPara para, Action<ResponseEvent> callback) {
            Sdk.ChangeCustomPlayerStatus (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new ChangeCustomPlayerStatusRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public void RemovePlayer (RemovePlayerPara para, Action<ResponseEvent> callback) {
            Sdk.RemovePlayer (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new RemovePlayerRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public void GetRoomDetail (Action<ResponseEvent> callback) {
            Sdk.GetRoomByRoomId (this.RoomUtil.AddRoomPara (), (eve) => {
                if (eve.Data != null) {
                    var rsp = new GetRoomByRoomIdRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                    Debugger.Log("Get room detail: {0}", eve.Code);
                }
                callback?.Invoke (eve);
            });
        }

        public void MatchPlayers (MatchPlayersPara para, Action<ResponseEvent> callback) {
            if (Room.OnMatch != null) {
                Sdk.MatchPlayers (para, eve => {
                    if (eve.Data != null) {
                        var rsp = new MatchPlayersRsp ();
                        rsp.MergeFrom ((ByteString) eve.Data);
                        eve.Data = rsp;
                    }
                    callback?.Invoke (eve);
                });
                return;
            }

            BroadcastOnce.Push (BroadcastOnce.PlayerComplex, callback);

            void Eve (ResponseEvent e) {
                if (e.Code == (int) QAppProtoErrCode.EcOk) return;
                e.Data = null;
                BroadcastOnce.Once (BroadcastOnce.PlayerComplex, e);
            }

            Sdk.MatchPlayers (para, Eve);
        }

        public void MatchRoom (MatchRoomPara para, Action<ResponseEvent> callback) {
            Sdk.MatchRoom (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new MatchRoomSimpleRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp.RoomInfo?.ToByteString ();
                    this.RoomUtil.SaveRoomInfo (eve);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public void MatchGroup (MatchGroupPara para, Action<ResponseEvent> callback) {
            Sdk.MatchGroup (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new MatchGroupRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        public void CancelPlayerMatch (CancelPlayerMatchPara para, Action<ResponseEvent> callback) {
            void Eve (ResponseEvent e) {
                if (e.Code == ErrCode.EcOk) {
                    BroadcastOnce.RemoveCallbacksByTag (BroadcastOnce.PlayerComplex);
                    BroadcastOnce.RemoveCallbacksByTag (BroadcastOnce.PlayerSimple);
                }

                callback?.Invoke (e);
            }
            Sdk.CancelMatch (para, Eve);
        }

        public void StartFrameSync (Action<ResponseEvent> callback) {
            this.RoomUtil.ActiveFrame ();
            Sdk.Instance.StartFrameSync (callback);
        }

        public void StopFrameSync (Action<ResponseEvent> callback) {
            this.RoomUtil.ActiveFrame ();
            Sdk.Instance.StopFrameSync (callback);
        }

        public void SendFrame (SendFramePara para, Action<ResponseEvent> callback) {
            this.RoomUtil.ActiveFrame ();
            Sdk.Instance.SendFrame (para, (eve) => {
                if (eve.Data != null) {
                    var rsp = new SendFrameRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            });
        }

        /**
         * @doc Room.requestFrame
         * @name 请求补帧
         * @description 调用结果将在 callback 中异步返回。
         * @param {SDKType.RequestFramePara} requestFramePara  请求补帧参数
         * @param {SDKType.ReqCallback<SDKType.RequestFrameRsp>} callback  响应回调函数
         * @returns {void}
         */
        public void RequestFrame (RequestFramePara para, Action<ResponseEvent> callback) {
            this.RoomUtil.ActiveFrame ();

            void Eve (ResponseEvent eve) {
                // Debugger.Log("request frame rsp");
                if (eve.Data != null) {
                    var rsp = new RequestFrameRsp ();
                    rsp.MergeFrom ((ByteString) eve.Data);
                    var frames = new List<Frame> ();
                    foreach (var item in rsp.Frames) {
                        var frame = new Frame {
                            Id = item.Id,
                            Ext = item.Ext,
                            Time = Convert.ToInt64 (SdkUtil.GetCurrentTimeSeconds()),
                            RoomId = RoomInfo.Id,
                            IsReplay = true
                        };
                        frame.Items.AddRange (item.Items);
                        frames.Add (frame);
                    }
                    rsp.Frames.Clear ();
                    rsp.Frames.AddRange (frames);
                    eve.Data = rsp;
                }
                callback?.Invoke (eve);
            }

            Sdk.Instance.RequestFrame (para, Eve);
        }

        public void RetryAutoRequestFrame () {
            RoomBroadcast.FrameBroadcast.RetryFill (this);
        }

        public void SendToClient (SendToClientPara para, Action<ResponseEvent> callback) {
            var recvPlayerList = para.RecvPlayerList;
            switch (para.RecvType) {
                case RecvType.RoomAll:
                    {
                        // 发给所有玩家
                        recvPlayerList.AddRange (RoomInfo.PlayerList.Select (info => info.Id));
                        break;
                    }
                case RecvType.RoomOthers:
                    {
                        // 不包含自己的其他玩家
                        recvPlayerList.AddRange (from info in RoomInfo.PlayerList where!info.Id.Equals (RequestHeader.PlayerId) select info.Id);
                        break;
                    }
                case RecvType.RoomSome:
                    break;
                default:
                    {
                        callback?.Invoke (new ResponseEvent (ErrCode.EcParamsInvalid, "参数错误，消息接收者类型无效", "", null));
                        return;
                    }
            }
            var callbackPara = new SendToClientPara {
                RecvPlayerList = recvPlayerList,
                Msg = para.Msg
            };
            Sdk.SendToClient (para, RoomInfo.Id, callback);
        }

        public void SendToGameSvr (SendToGameSvrPara para, Action<ResponseEvent> callback) {
            Sdk.Instance.SendToGameSvr (para, RoomInfo.Id, callback);
        }
    }
}