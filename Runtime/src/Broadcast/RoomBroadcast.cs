using System;
using System.Linq;
using Google.Protobuf;

using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Broadcast {
    public class RoomBroadcast {
        private readonly com.unity.mgobe.Room _room;
        public FrameBroadcast FrameBroadcast { get; }
        public int FrameBroadcastFrameId { get; } = 0;

        public RoomBroadcast (com.unity.mgobe.Room room) {
            this._room = room;
            var frameRate = this._room.RoomInfo.FrameRate != 0 ? 1000 / this._room.RoomInfo.FrameRate : 66;
            void Callback (BroadcastEvent eve) {
                var bst = (RecvFrameBst) eve?.Data;
                if (bst?.Frame == null || !this.MatchId (bst.Frame.RoomId)) {
                    Debugger.Log("roombst return: {} {1}", bst?.Frame == null, !this.MatchId (bst.Frame.RoomId));
                    return;
                }
                _room.OnRecvFrame?.Invoke (eve);
            }

            this.FrameBroadcast = new FrameBroadcast (frameRate, Callback);
        }

        /**
         * 本地网络状态变化
         */
        public void OnNetwork (ResponseEvent eve) {
            _room.onUpdate (this._room);
        }

        /**
         * 玩家加入房间广播
         */
        public void OnJoinRoom (BroadcastEvent eve) {
            var bst = new JoinRoomBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnJoinRoom?.Invoke (eve);
        }

        /**
         * 玩家退出房间广播
         */
        public void OnLeaveRoom (BroadcastEvent eve) {
            var bst = new LeaveRoomBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnLeaveRoom?.Invoke (eve);
        }

        /**
         * 玩家解散房间广播
         */
        public void OnDismissRoom (BroadcastEvent eve) {
            var bst = new DismissRoomBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            void DissmissRoom (BroadcastEvent e) => this._room.RoomInfo = new RoomInfo ();
            this.MatchRoomInfo (eve, bst.RoomInfo, DissmissRoom);
            eve.Data = bst;
            _room.OnDismissRoom?.Invoke (eve);
        }

        /**
         * 玩家修改房间广播
         */
        public void OnChangeRoom (BroadcastEvent eve) {
            var bst = new ChangeRoomBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnChangeRoom?.Invoke (eve);
        }

        /**
         * 玩家被踢广播
         */
        public void OnRemovePlayer (BroadcastEvent eve) {
            var bst = new RemovePlayerBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnRemovePlayer?.Invoke (eve);
        }

        /**
         * 收到消息广播
         */
        public void OnRecvFromClient (string id, BroadcastEvent eve) {

            if (!this.MatchId (id)) return;
            _room.OnRecvFromClient?.Invoke (eve);
        }

        /**
         * 自定义服务广播
         */
        public void OnRecvFromGameSvr (string id, BroadcastEvent eve) {
            if (!this.MatchId (id)) return;
            try {
                var bst = new RecvFromGameSvrBst ();
                bst.MergeFrom ((ByteString) eve.Data);
                eve.Data = bst;
                _room.OnRecvFromGameSvr?.Invoke (eve);
            } catch(Exception e) {
                Debugger.Log(e.ToString());
            }
        }

        /**
         * 匹配成功广播
         */
        public void OnMatchPlayers (BroadcastEvent eve) {
            var bst = new MatchPlayersBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = new RoomInfo (bst.RoomInfo);
            this._room.RoomUtil.SetRoomInfo (roomInfo);
            if (roomInfo.PlayerList != null) {
                if (roomInfo.PlayerList.Any (info => Listener.IsMe (info.Id))) {
                    this._room.RoomUtil.ActiveFrame ();
                }
            }
            string tag;
            if (bst.MatchType == MatchType.PlayerComplex) {
                tag = BroadcastOnce.PlayerComplex;
            } else {
                return;
            }
            eve.Data = bst;
            var e = new ResponseEvent (ErrCode.EcOk, "", "", eve.Data);

            BroadcastOnce.Once (tag, e);
        }

        /**
         * 匹配超时广播
         */
        public void OnMatchTimeout (BroadcastEvent eve) {
            var bst = new MatchTimeoutBst ();
            bst.MergeFrom ((ByteString) eve.Data);

            if (bst.MatchType == MatchType.PlayerComplex) { } else {
                return;
            }
            var matchErrCode = bst.ErrCode != 0 ? bst.ErrCode : (int) QAppProtoErrCode.EcMatchTimeout;
            var errCode = SdkUtil.ErrCodeConvert (matchErrCode);
            var errMsg = SdkUtil.ErrCodeConvert (errCode, "");
            var e = new ResponseEvent (errCode, errMsg, "", new object ());
        }

        /**
         * 玩家网络状态变化广播
         */
        public void OnChangePlayerNetworkState (BroadcastEvent eve) {
            var bst = new ChangePlayerNetworkStateBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnChangePlayerNetworkState?.Invoke (eve);
        }

        /**
         * 玩家修改玩家状态广播
         */
        public void OnChangeCustomPlayerStatus (BroadcastEvent eve) {
            var bst = new ChangeCustomPlayerStatusBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnChangeCustomPlayerStatus?.Invoke (eve);
        }

        /**
         * 开始游戏广播
         */
        public void OnStartFrameSync (BroadcastEvent eve) {
            var bst = new StartFrameSyncBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnStartFrameSync?.Invoke (eve);
        }

        /**
         * 结束游戏广播
         */
        public void OnStopFrameSync (BroadcastEvent eve) {
            this.FrameBroadcast.Reset (0);
            var bst = new StopFrameSyncBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomInfo = (RoomInfo) bst.RoomInfo;
            this.Save (eve, roomInfo);
            eve.Data = bst;
            _room.OnStopFrameSync?.Invoke (eve);
        }

        /**
         * 收到帧同步消息
         */
        public void OnRecvFrame (BroadcastEvent eve) {
            if (_room.RoomInfo.FrameSyncState == (int) FrameSyncState.Stop) {
                return;
            }
            this.FrameBroadcast.Push (eve, _room);
        }
        public void Error (BroadcastEvent eve) {
            // this.room.
        }

        public void FrameBroadcastFrameIdReset (int sentFrameId) {
            this.FrameBroadcast.Reset (sentFrameId);
        }

        private bool MatchRoomInfo (RoomInfo roomnInfo) {
            return this._room.RoomInfo.Id == roomnInfo.Id;
        }

        private void MatchRoomInfo (BroadcastEvent eve, RoomInfo roomnInfo, Action<BroadcastEvent> callback) {
            var roomInfo = new RoomInfo ();

            if (!this.MatchRoomInfo (roomInfo)) {
                return;
            }

            callback?.Invoke (eve);
        }

        private bool MatchId (string id) {
            return this._room.RoomInfo.Id.Equals (id);
        }

        private void Save (BroadcastEvent eve, RoomInfo roomInfo) {
            if (!this.MatchRoomInfo (roomInfo)) {
                return;
            }
            this._room.RoomUtil.SetRoomInfo (new RoomInfo (roomInfo));
        }

        protected void MatchFrameBroadcast (string id, BroadcastEvent eve, Action<BroadcastEvent> callback) {
            if (!this.MatchId (id)) return;
            callback?.Invoke (eve);
        }

    }

    public class GlobalRoomBroadcast {
        public static GlobalRoomBroadcast Instance { get; } = new GlobalRoomBroadcast ();

        /**
         * 匹配成功广播
         */
        public void OnMatchPlayers (BroadcastEvent eve) {
            var matchBst = (MatchPlayersBst) eve.Data;
            var roomEvent = new BroadcastEvent (
                new MatchBst {
                    RoomInfo = new RoomInfo (matchBst.RoomInfo),
                        ErrCode = ErrCode.EcOk
                },
                eve.Seq);
            Debugger.Log ("onMatch bst gloal {0}", roomEvent);
            com.unity.mgobe.Room.OnMatch?.Invoke (roomEvent);
        }

        /**
         * 匹配超时广播
         */
        public void OnMatchTimeout (BroadcastEvent eve) {
            var bst = new MatchTimeoutBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomEvent = new BroadcastEvent (
                new MatchBst { ErrCode = bst.ErrCode },
                eve.Seq
            );
            com.unity.mgobe.Room.OnMatch?.Invoke (roomEvent);
        }

        /**
         * 匹配取消广播
         */
        public void OnCancelMatch (BroadcastEvent eve) {
            var bst = new CancelMatchBst ();
            bst.MergeFrom ((ByteString) eve.Data);
            var roomEvent = new BroadcastEvent (
                new CancelMatchBst {
                    MatchCode = bst.MatchCode,
                        PlayerId = bst.PlayerId
                },
                eve.Seq);
            com.unity.mgobe.Room.OnCancelMatch?.Invoke (roomEvent);
        }
    }
}