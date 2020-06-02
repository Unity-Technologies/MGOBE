using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Lagame;
using Newtonsoft.Json;
using Packages.com.unity.mgobe.Runtime;
using Packages.com.unity.mgobe.Runtime.src;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;

// [InitializeOnLoad]
public class Main : MonoBehaviour {

    // 请求名称
    public InputField NameInputField;
    // 参数表
    public InputField ParamInputField;
    // 响应
    public InputField RspInputField;
    // 广播
    public InputField BstInputField;
    // 帧广播
    public InputField FrameInputField;
    // PlayerId
    public InputField PlayerIdInputField;

    // scroll content
    public RectTransform content;

    // public static void Stop () {
    //     // Global.UnInit();
    // }

    // private static Action<PlayModeStateChange> OnUnityPlayModeChanged = (PlayModeStateChange mode) => {
    //     switch (mode) {
    //         case PlayModeStateChange.ExitingPlayMode:
    //             Stop ();
    //             break;
    //         default:
    //             break;
    //     }
    // };

    // Start is called before the first frame update
    void Start () {
        // EditorApplication.playModeStateChanged += OnUnityPlayModeChanged;
        SDKTools.nameStr = "xxx";
        SDKTools.paramStr = "xxx";
        SDKTools.rspStr = "xxx";
        SDKTools.bstStr = "xxx";
        SDKTools.frameStr = "xxx";
    }

    // Update is called once per frame
    void Update () {
        this.content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (400, this.GetInfoHeight ());

        this.RspInputField.text = SDKTools.rspStr + "\n";
        this.BstInputField.text = SDKTools.bstStr + "\n";
        this.FrameInputField.text = SDKTools.frameStr + "\n";
        this.RspInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.BstInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.FrameInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.NameInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.ParamInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();

        this.PlayerIdInputField.text = "玩家ID=" + Player.Id + "\n" + SDKTools.requestFrameInfoStr;
    }

    public void OnInputChanged () {
        this.NameInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.ParamInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.RspInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.BstInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
        this.FrameInputField.GetComponent<ContentSizeFitter> ().SetLayoutVertical ();
    }

    // 获取高度
    float GetInfoHeight () {
        var nameHeight = this.NameInputField.GetComponent<RectTransform> ().rect.height;
        var paramHeight = this.ParamInputField.GetComponent<RectTransform> ().rect.height;
        var rspHeight = this.RspInputField.GetComponent<RectTransform> ().rect.height;
        var bstHeight = this.BstInputField.GetComponent<RectTransform> ().rect.height;
        var frameHeight = this.FrameInputField.GetComponent<RectTransform> ().rect.height;

        var total = nameHeight + paramHeight + rspHeight + bstHeight + frameHeight + 20 * 5;

        return total;
    }

    // 设置入参和请求名称
    void SetRequestInput (string nameStr, string paramStr) {
        this.NameInputField.text = nameStr + "\n";
        this.ParamInputField.text = paramStr + "\n";
        SDKTools.rspStr = "";
        SDKTools.bstStr = "";
    }

    public void OnSendButtonClick () {
        Debug.Log ("发送请求");
        SDKTools.paramStr = this.ParamInputField.text;
        SDKTools.nameStr = this.NameInputField.text.Replace ("\n", "");
        SDKTools.run ();
    }

    // 点击 初始化
    public void OnInitButtonClick () {
        try {
            Utils.Config config = new Utils.Config {
                GameId = "obg-8xwt9z7a",
                Key = "f857f0dc320e77ed46081407645b3a9fcfd40983",
                Domain = "8xwt9z7a.wxlagame.com",
                // GameId = "obg-ernpm27s",
                // Key = "0d0b3d21f243ad25c47b45ef07fe586fd01a3cce",
                // Domain = "ernpm27s.wxlagame.com",
                OpenId = "" + Utils.Date.now (),
            };

            string output = JsonConvert.SerializeObject (config, Formatting.Indented);

            this.SetRequestInput ("ListenerInit", output);
        } catch (Exception e) {
            SDKTools.bstStr = e.Message.ToString ();
        }
    }

    // 点击 添加监听
    public void OnAddButtonClick () {
        this.SetRequestInput ("ListenerAdd", "");
    }

    // 点击 移除监听
    public void OnRemoveButtonClick () {
        this.SetRequestInput ("ListenerRemove", "");
    }

    // 点击 清理监听
    public void OnClearButtonClick () {
        this.SetRequestInput ("ListenerClear", "");
    }

    // 点击 查询房间列表
    public void OnGetRoomListButtonClick () {
        GetRoomListPara para = new GetRoomListPara {
            PageNo = 1,
            PageSize = 5,
            RoomType = "",
            IsDesc = false,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("getRoomList", output);
    }

    // 点击 根据ID查询房间
    public void OnGetRoomByRoomIdButtonClick () {
        Debug.Log ("根据ID查询房间");

        GetRoomByRoomIdPara para = new GetRoomByRoomIdPara {
            RoomId = "xxx",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("getRoomByRoomId", output);
    }

    // 点击 查询我的房间
    public void OnGetMyRoomButtonClick () {
        Debug.Log ("查询我的房间");
        this.SetRequestInput ("getMyRoom", "");
    }

    // 点击 创建房间
    public void OnCreateRoomButtonClick () {
        Debug.Log ("创建房间");

        PlayerInfoPara playerInfoPara = new PlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
        };

        CreateRoomPara para = new CreateRoomPara {
            RoomName = "测试房间名称",
            RoomType = "A",
            MaxPlayers = 10,
            IsPrivate = true,
            CustomProperties = "xxxxxxxx",
            PlayerInfo = playerInfoPara,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("createRoom", output);
    }

    // 点击 创建团队房间
    public void OnCreateTeamRoomButtonClick () {
        Debug.Log ("创建团队房间");

        PlayerInfoPara playerInfoPara = new PlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
        };

        CreateTeamRoomPara para = new CreateTeamRoomPara {
            RoomName = "测试房间名称",
            RoomType = "A",
            MaxPlayers = 10,
            IsPrivate = true,
            CustomProperties = "xxxxxxxx",
            TeamNumber = 10,
            PlayerInfo = playerInfoPara,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("createTeamRoom", output);
    }

    // 点击 initRoom
    public void OnInitRoomButtonClick () {
        Debug.Log ("initRoom");

        RoomInfo para = new RoomInfo {
            Id = "请填写房间ID",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("initRoom", output);
    }

    // 点击 加入房间
    public void OnJoinRoomButtonClick () {
        Debug.Log ("加入房间");

        PlayerInfoPara playerInfoPara = new PlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
        };

        JoinRoomPara para = new JoinRoomPara {
            PlayerInfo = playerInfoPara,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("joinRoom", output);
    }

    // 点击 加入团队房间
    public void OnJoinTeamRoomButtonClick () {
        Debug.Log ("加入团队房间");

        PlayerInfoPara playerInfoPara = new PlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
        };

        JoinTeamRoomPara para = new JoinTeamRoomPara {
            TeamId = "0",
            PlayerInfo = playerInfoPara,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("joinTeamRoom", output);
    }

    // 点击 查询当前房间信息
    public void OnGetRoomDetailButtonClick () {
        Debug.Log ("查询当前房间信息");
        this.SetRequestInput ("getRoomDetail", "");
    }

    // 点击 退出房间
    public void OnLeaveRoomButtonClick () {
        Debug.Log ("退出房间");
        this.SetRequestInput ("leaveRoom", "");
    }

    // 点击 修改房间
    public void OnChangeRoomButtonClick () {
        Debug.Log ("修改房间");

        ChangeRoomPara para = new ChangeRoomPara {
            RoomName = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
            IsPrivate = true,
            IsForbidJoin = true,
            CustomProperties = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("changeRoom", output);
    }

    // 点击 解散房间
    public void OnDismissRoomButtonClick () {
        Debug.Log ("解散房间");
        this.SetRequestInput ("dismissRoom", "");
    }

    // 点击 房间匹配
    public void OnMatchRoomButtonClick () {
        Debug.Log ("房间匹配");
        PlayerInfoPara playerInfoPara = new PlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
        };

        MatchRoomPara para = new MatchRoomPara {
            RoomType = "Xx",
            MaxPlayers = 2,
            PlayerInfo = playerInfoPara,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("matchRoom", output);
    }

    // 点击 修改玩家状态
    public void OnChangePlayerCustomStatusButtonClick () {
        Debug.Log ("修改玩家状态");

        ChangeCustomPlayerStatusPara para = new ChangeCustomPlayerStatusPara {
            CustomPlayerStatus = 9876,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("changeCustomPlayerStatus", output);
    }

    // 点击 移除玩家
    public void OnRemovePlayerButtonClick () {
        Debug.Log ("移除玩家");
        RemovePlayerPara para = new RemovePlayerPara {
            RemovePlayerId = "请填写玩家ID",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("removePlayer", output);
    }

    // 点击 开始帧同步
    public void OnStartFrameSyncButtonClick () {
        Debug.Log ("开始帧同步");
        this.SetRequestInput ("startFrameSync", "");
    }

    // 点击 停止帧同步
    public void OnStopButtonClick () {
        Debug.Log ("停止帧同步");
        this.SetRequestInput ("stopFrameSync", "");
    }

    // 点击 发帧
    public void OnSendFrameButtonClick () {
        Debug.Log ("发帧");

        SendFramePara para = new SendFramePara {
            Data = "3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333"
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("sendFrame", output);
    }

    // 点击 补帧
    public void OnRequestFrameButtonClick () {
        Debug.Log ("补帧");

        RequestFramePara para = new RequestFramePara {
            BeginFrameId = 1,
            EndFrameId = 2,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("requestFrame", output);
    }

    // 点击 发送房间消息
    public void OnSendToClientButtonClick () {
        Debug.Log ("发送房间消息");

        SendToClientPara para = new SendToClientPara {
            RecvType = RecvType.RoomAll,
            RecvPlayerList = new List<string> { "请填写玩家ID" },
            Msg = "hello",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("sendToClient", output);
    }

    // 点击 发送实时服务器消息
    public void OnSendToGameSvrButtonClick () {
        Debug.Log ("发送实时服务器消息");

        Object data = new { cmd = 1, };

        SendToGameSvrPara para = new SendToGameSvrPara {
            Data = data,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("sendToGameSvr", output);
    }

    // 点击 玩家匹配
    public void OnMatchPlayersButtonClick () {
        Debug.Log ("玩家匹配");

        MatchAttribute matchAttribute = new MatchAttribute ();

        matchAttribute.Name = "name";
        matchAttribute.Value = 99;

        MatchPlayerInfoPara playerInfoPara = new MatchPlayerInfoPara {
            Name = "测试人员1",
            CustomProfile = "测试人员xxxxxxxx",
            CustomPlayerStatus = 12345,
            MatchAttributes = new List<MatchAttribute> { matchAttribute },
        };

        MatchPlayersPara para = new MatchPlayersPara {
            PlayerInfoPara = playerInfoPara,
            MatchCode = "match-hel6rt0j",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("matchPlayers", output);
    }

    // 点击 组队匹配
    public void OnMatchGroupButtonClick () {
        Debug.Log ("组队匹配");

        MatchAttribute matchAttribute = new MatchAttribute ();

        matchAttribute.Name = "skill";
        matchAttribute.Value = 5;

        MatchGroupPlayerInfoPara p1 = new MatchGroupPlayerInfoPara () {
            Id = Player.Id == "" ? "ID" : Player.Id,
            Name = "name",
            CustomPlayerStatus = 1,
            CustomProfile = "xxx",
            MatchAttributes = new List<MatchAttribute> { matchAttribute },
        };

        MatchGroupPlayerInfoPara p2 = new MatchGroupPlayerInfoPara () {
            Id = "xppo6neo",
            Name = "name",
            CustomPlayerStatus = 1,
            CustomProfile = "xxx",
            MatchAttributes = new List<MatchAttribute> { matchAttribute },
        };

        MatchGroupPara para = new MatchGroupPara {
            PlayerInfoList = new List<MatchGroupPlayerInfoPara> { p1, p2 },
            MatchCode = "match-evtp3fdv",
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("matchGroup", output);
    }

    // 点击 取消匹配
    public void OnCancelMatchButtonClick () {
        Debug.Log ("取消匹配");

        CancelPlayerMatchPara para = new CancelPlayerMatchPara {
            MatchType = MatchType.PlayerComplex,
        };

        string output = JsonConvert.SerializeObject (para, Formatting.Indented);

        this.SetRequestInput ("cancelPlayerMatch", output);
    }
}