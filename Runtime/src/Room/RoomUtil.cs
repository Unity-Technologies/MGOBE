using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;


using com.unity.mgobe.src.Broadcast;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;
using com.unity.mgobe.src;

namespace com.unity.mgobe {
    internal class RoomUtil {
        private readonly Room _room;

        public RoomUtil (Room room) {
            this._room = room;
            this._room.RoomInfo = new RoomInfo ();
        }

        public void SetRoomInfo (RoomInfo roomInfo) {
            // if(roomInfo!=null) Debugger.Log("setRoomInfo {0}", roomInfo.PlayerList);
            if (roomInfo == null) roomInfo = new RoomInfo ();
            var oldRoomInfo = _room.RoomInfo ?? new RoomInfo ();

            _room.RoomInfo = roomInfo;

            // 更新玩家信息
            if (_room.IsInRoom ()) {
                foreach (var info in _room.RoomInfo.PlayerList.Where (info => info.Id == RequestHeader.PlayerId)) {
                    GamePlayerInfo.SetInfo ((PlayerInfo) info);
                    break;
                }
            }

            // 重置帧广播信息:
            // 1 切换房间
            // 2 不存在房间id
            // 3 不在房间中
            // 4 房间的开始帧同步时间发生变化
            if (!oldRoomInfo.Id.Equals(_room.RoomInfo.Id) || string.IsNullOrEmpty (_room.RoomInfo.Id) || !_room.IsInRoom () || oldRoomInfo.StartGameTime != this._room.RoomInfo.StartGameTime) {
                _room.RoomBroadcast?.FrameBroadcastFrameIdReset (0);
            }

            // 激活第二条链接
            if (_room.RoomInfo != null && _room.RoomInfo.PlayerList != null) {
                if (_room.RoomInfo.PlayerList.Any (info => Listener.IsMe (info.Id))) {
                    this.ActiveFrame ();
                }
            }
            _room.onUpdate (this._room);
        }

        public void SaveRoomInfo (ResponseEvent eve) {
            if (eve.Code != ErrCode.EcOk) return;
            if (eve.Data != null) {
                var roomInfo = new RoomInfo ();
                roomInfo.MergeFrom ((ByteString) eve.Data);
                this.SetRoomInfo (roomInfo);
            } else {
                this.SetRoomInfo (null);
            }
        }

        public GetRoomByRoomIdPara AddRoomPara () {
            var para = new GetRoomByRoomIdPara ();
            para.RoomId = "";
            if (this._room != null && this._room.RoomInfo != null && this._room.RoomInfo.Id != null) {
                para.RoomId = this._room.RoomInfo.Id;
            }
            return para;
        }

        public void ActiveFrame () {
            Sdk.Instance.SetFrameRoom (_room.RoomInfo);
        }

        public void InitBroadcast () {
            if (this._room.RoomBroadcast == null) {
                this._room.RoomBroadcast = new RoomBroadcast (this._room);
            }
        }
    }
}