using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Lagame;
using Newtonsoft.Json;
using Packages.com.unity.mgobe.Runtime.src;
using Packages.com.unity.mgobe.Runtime.src.SDK;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class SDKTools
{
    public static string paramStr = "";
    public static string nameStr = "";
    public static string rspStr = "";
    public static string bstStr = "";
    public static string frameStr = "";
    
    public static string pingTime = "";
    public static string fitFrameTime = "";
    public static string bstFrameTime = "";
    public static string renderFrameTime = "";
    
    public static ulong lastFrameId = 0;
    public static string requestFrameInfoStr = "";
    
    public static Room room = new Room(null);

    public static void run()
    {
        Debug.Log(SDKTools.nameStr);
        Debug.Log(SDKTools.paramStr);

        try
        {
            //通过反射来执行类的静态方法
            MethodInfo mi = null;
            MethodInfo[] mis = typeof(SDKTools).GetMethods();

            for (var i = 0; i < mis.Length; i++)
            {
                if (mis[i].Name == SDKTools.nameStr)
                {
                    mi = mis[i];
                    break;
                }
            }

            if (mi == null)
            {
                Debug.Log("没有找到方法");
                return;
            }

            mi.Invoke(null, null);
        }
        catch (Exception err)
        {
            Debug.LogError("出错了");
            Debug.LogError(err.Message.ToString());
        }
    }

    public static Action<ResponseEvent> RspCallback = (ResponseEvent eve) =>
    {
        SDKTools.rspStr = JsonConvert.SerializeObject(eve, Formatting.Indented);
    };

    public static Action<BroadcastEvent> genBstCallback(string tag)
    {
        return (BroadcastEvent eve) =>
        {
            SDKTools.bstStr = tag + "\n" +  JsonConvert.SerializeObject(eve, Formatting.Indented);

            if (tag == "OnStartFrameSync" || tag == "OnStopFrameSync")
            {
                SDKTools.lastFrameId = 0;
                SDKTools.requestFrameInfoStr = "";
                SDKTools.frameStr = "";
                return;
            }

            if (tag == "OnRecvFrame")
            {
                ulong frameId = ((RecvFrameBst) eve.Data).Frame.Id;

                if (frameId - SDKTools.lastFrameId > 1)
                {
                    // 丢帧
                    SDKTools.requestFrameInfoStr = "丢帧：" + frameId;
                }
                
                SDKTools.lastFrameId = frameId;

                if (((RecvFrameBst) eve.Data).Frame.Items.Count > 0)
                {
                    SDKTools.frameStr = JsonConvert.SerializeObject(eve, Formatting.Indented);
                }
            }
            
        };
    }

    public static Action<BroadcastEvent> BstCallback = (BroadcastEvent eve) =>
    {
        SDKTools.bstStr = JsonConvert.SerializeObject(eve, Formatting.Indented);
    };
    
    // 设置广播
    private static void setBstCallback()
    {
        Room.OnMatch = SDKTools.genBstCallback("OnMatch");
        Room.OnCancelMatch = SDKTools.genBstCallback("OnCancelMatch");
        SDKTools.room.OnChangeRoom = SDKTools.genBstCallback("OnChangeRoom");
        SDKTools.room.OnDismissRoom = SDKTools.genBstCallback("OnDismissRoom");
        SDKTools.room.OnJoinRoom = SDKTools.genBstCallback("OnJoinRoom");
        SDKTools.room.OnLeaveRoom = SDKTools.genBstCallback("OnLeaveRoom");
        SDKTools.room.OnRecvFrame = SDKTools.genBstCallback("OnRecvFrame");
        SDKTools.room.OnRemovePlayer = SDKTools.genBstCallback("OnRemovePlayer");
        SDKTools.room.OnRecvFromClient = SDKTools.genBstCallback("OnRecvFromClient");
        SDKTools.room.OnStartFrameSync = SDKTools.genBstCallback("OnStartFrameSync");
        SDKTools.room.OnStopFrameSync = SDKTools.genBstCallback("OnStopFrameSync");
        SDKTools.room.OnAutoRequestFrameError = SDKTools.genBstCallback("OnAutoRequestFrameError");
        SDKTools.room.OnChangeCustomPlayerStatus = SDKTools.genBstCallback("OnChangeCustomPlayerStatus");
        SDKTools.room.OnChangePlayerNetworkState = SDKTools.genBstCallback("OnChangePlayerNetworkState");
        SDKTools.room.OnRecvFromGameSvr = SDKTools.genBstCallback("OnRecvFromGameSvr");
        /*
        StatCallbacks.onPingTime = (double time) => null;
        StatCallbacks.onFitFrameTime = (double time) => null;
        StatCallbacks.onBstFrameRate = (double time) => null;
        StatCallbacks.onRenderFrameRate = (double time) => null;
        */
    }

    // SDK 初始化
    public static void ListenerInit()
    {
        Utils.Config initConfig = JsonConvert.DeserializeObject<Utils.Config>(SDKTools.paramStr);

        GameInfoPara gameInfo = new GameInfoPara
        {
            GameId = initConfig.GameId,
            SecretKey = initConfig.Key,
            OpenId = initConfig.OpenId
        };

        ConfigPara config = new ConfigPara
        {
            Url = initConfig.Domain,
            ReconnectMaxTimes = 5,
            ReconnectInterval = 4000,
            ResendInterval = 2000,
            ResendTimeout = 20000,
            IsAutoRequestFrame = true,
        };
        
        SDKTools.setBstCallback();

        Listener.Init(gameInfo, config, SDKTools.RspCallback);
        Listener.Add(SDKTools.room);
    }
    
    public static void ListenerAdd()
    {
        Listener.Add(SDKTools.room);
        SDKTools.rspStr = "调用成功";
    }

    public static void ListenerRemove()
    {
        Listener.Remove(SDKTools.room);
        SDKTools.rspStr = "调用成功";
    }

    public static void ListenerClear()
    {
        Listener.Clear();
        SDKTools.rspStr = "调用成功";
    }

    public static void getRoomList()
    {
        GetRoomListPara para = JsonConvert.DeserializeObject<GetRoomListPara>(SDKTools.paramStr);
        Room.GetRoomList(para, SDKTools.RspCallback);
    }
    
    public static void getMyRoom()
    {
        Room.GetMyRoom(SDKTools.RspCallback);
    }
    
    public static void getRoomByRoomId()
    {
        GetRoomByRoomIdPara para = JsonConvert.DeserializeObject<GetRoomByRoomIdPara>(SDKTools.paramStr);
        Room.GetRoomByRoomId(para, SDKTools.RspCallback);
    }

    public static void createRoom()
    {
        CreateRoomPara para = JsonConvert.DeserializeObject<CreateRoomPara>(SDKTools.paramStr);
        SDKTools.room.CreateRoom(para, SDKTools.RspCallback);
    }

    public static void createTeamRoom()
    {
        CreateTeamRoomPara para = JsonConvert.DeserializeObject<CreateTeamRoomPara>(SDKTools.paramStr);
        SDKTools.room.CreateTeamRoom(para, SDKTools.RspCallback);
    }
    
    public static void initRoom()
    {
        RoomInfo para = JsonConvert.DeserializeObject<RoomInfo>(SDKTools.paramStr);
        SDKTools.room.InitRoom(para);
    }
    
    public static void joinRoom()
    {
        JoinRoomPara para = JsonConvert.DeserializeObject<JoinRoomPara>(SDKTools.paramStr);
        SDKTools.room.JoinRoom(para, SDKTools.RspCallback);
    }
    
    public static void joinTeamRoom()
    {
        JoinTeamRoomPara para = JsonConvert.DeserializeObject<JoinTeamRoomPara>(SDKTools.paramStr);
        SDKTools.room.JoinTeamRoom(para, SDKTools.RspCallback);
    }
    public static void getRoomDetail()
    {
        SDKTools.room.GetRoomDetail(SDKTools.RspCallback);
    }
    
    public static void leaveRoom()
    {
        SDKTools.room.LeaveRoom(SDKTools.RspCallback);
    }
    
    public static void dismissRoom()
    {
        SDKTools.room.DismissRoom(SDKTools.RspCallback);
    }
    
    public static void changeRoom()
    {
        ChangeRoomPara para = JsonConvert.DeserializeObject<ChangeRoomPara>(SDKTools.paramStr);
        SDKTools.room.ChangeRoom(para, SDKTools.RspCallback);
    }

    public static void matchRoom()
    {
        MatchRoomPara para = JsonConvert.DeserializeObject<MatchRoomPara>(SDKTools.paramStr);
        SDKTools.room.MatchRoom(para, SDKTools.RspCallback);
    }

    public static void changeCustomPlayerStatus()
    {
        ChangeCustomPlayerStatusPara para = JsonConvert.DeserializeObject<ChangeCustomPlayerStatusPara>(SDKTools.paramStr);
        SDKTools.room.ChangeCustomPlayerStatus(para, SDKTools.RspCallback);
    }

    public static void removePlayer()
    {
        RemovePlayerPara para = JsonConvert.DeserializeObject<RemovePlayerPara>(SDKTools.paramStr);
        SDKTools.room.RemovePlayer(para, SDKTools.RspCallback);
    }
    public static void startFrameSync()
    {
        SDKTools.room.StartFrameSync(SDKTools.RspCallback);
    }
    
    public static void stopFrameSync()
    {
        SDKTools.room.StopFrameSync(SDKTools.RspCallback);
    }
    
    public static void sendFrame()
    {
        SendFramePara para = JsonConvert.DeserializeObject<SendFramePara>(SDKTools.paramStr);
        Debug.LogFormat("sendFrame>>>>  {0}",  para.Data);
        SDKTools.room.SendFrame(para, SDKTools.RspCallback);
    }

    public static void requestFrame()
    {
        RequestFramePara para = JsonConvert.DeserializeObject<RequestFramePara>(SDKTools.paramStr);
        SDKTools.room.RequestFrame(para, SDKTools.RspCallback);
    }
    
    public static void sendToClient()
    {
        SendToClientPara para = JsonConvert.DeserializeObject<SendToClientPara>(SDKTools.paramStr);
        SDKTools.room.SendToClient(para, SDKTools.RspCallback);
    }
    
    public static void sendToGameSvr()
    {
        SendToGameSvrPara para = JsonConvert.DeserializeObject<SendToGameSvrPara>(SDKTools.paramStr);
        SDKTools.room.SendToGameSvr(para, SDKTools.RspCallback);
    }

    public static void matchPlayers()
    {
        MatchPlayersPara para = JsonConvert.DeserializeObject<MatchPlayersPara>(SDKTools.paramStr);
        SDKTools.room.MatchPlayers(para, SDKTools.RspCallback);
    }

    public static void matchGroup()
    {
        MatchGroupPara para = JsonConvert.DeserializeObject<MatchGroupPara>(SDKTools.paramStr);
        SDKTools.room.MatchGroup(para, SDKTools.RspCallback);
    }

    public static void cancelPlayerMatch()
    {
        CancelPlayerMatchPara para = JsonConvert.DeserializeObject<CancelPlayerMatchPara>(SDKTools.paramStr);
        SDKTools.room.CancelPlayerMatch(para, SDKTools.RspCallback);
    }
}