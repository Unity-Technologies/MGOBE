using System;
using System.Collections.Generic;
using Google.Protobuf;

using com.unity.mgobe.src.Net.Sockets;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Net
{
    public struct QueueRequest
    {
        public ByteString Body { get; set; }

        public int Subcmd { get; set; }

        public Action<ResponseEvent> Completed { get; set; }

        public string RequestCmd { get; set; }

        public bool Running { get; set; }

        public NetResponseCallback Response { get; set; }

        public Action BeforeRequest { get; set; }

        public Action<string> AfterRequest { get; set; }
    }

    public class BaseNetUtil
    {

        private static HashSet<ClientSendServerReqWrap2Cmd> _roomCmd;
        private static Queue<QueueRequest> _checkLoginQueue;
        private static Queue<QueueRequest> _roomQueue;

        public static void StartQueueLoop()
        {
            BaseNetUtil._checkLoginQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomCmd = new HashSet<ClientSendServerReqWrap2Cmd>
            {
                ClientSendServerReqWrap2Cmd.ECmdCreateRoomReq,
                ClientSendServerReqWrap2Cmd.ECmdJoinRoomReq,
                ClientSendServerReqWrap2Cmd.ECmdQuitRoomReq,
                ClientSendServerReqWrap2Cmd.ECmdDissmissRoomReq,
                ClientSendServerReqWrap2Cmd.ECmdChangeRoomPropertisReq,
                ClientSendServerReqWrap2Cmd.ECmdRemoveMemberReq,
                ClientSendServerReqWrap2Cmd.ECmdChangePlayerStateReq,
                ClientSendServerReqWrap2Cmd.ECmdStartFrameSyncReq,
                ClientSendServerReqWrap2Cmd.ECmdStopFrameSyncReq
            };
            // 创建房间
            // 加入房间
            // 离开房间
            // 解散房间
            // 房间变更
            // 移除房间内玩家
            // 修改用户状态
            // 开始帧同步
            // 停止帧同步
            Net.StartQueueLoop();
        }

        public static void StopQueueLoop()
        {
            BaseNetUtil._checkLoginQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomQueue = new Queue<QueueRequest>();
            BaseNetUtil._checkLoginQueue.Clear();
            BaseNetUtil._roomQueue.Clear();
            Net.StopQueueLoop();
        }

        public readonly NetClient client;
        private readonly NetServer _server;
        protected readonly Responses responses;

        public BaseNetUtil(Responses responses)
        {
            this.responses = responses;
            client = new NetClient(responses);
            _server = new NetServer();
        }

        public void BindSocket(Socket socket)
        {
            void HandleResponse(byte[] data) => client.HandleMessage(data);

            void HandleBroadcast(byte[] data) => NetServer.HandleMessage(data);

            client.BindSocket(socket, HandleResponse, HandleBroadcast);
            _server.BindSocket(socket, HandleResponse, HandleBroadcast);
        }
        public void UnbindSocket()
        {
            client.UnbindSocket();
            _server.UnbindSocket();
        }

        public void SetBroadcastHandler(ServerSendClientBstWrap2Type type, BroadcastCallback handler)
        {
            _server.SetBroadcastHandler(type, handler);
        }

        public string Send(ByteString body, int subcmd, NetResponseCallback response, Action<ResponseEvent> callback)
        {
            // 第一层 cmd：通用连接 | 帧同步连接
            var requestCmd = "comm_cmd";

            if (client.Socket.Id == (int)ConnectionType.Relay)
            {
                requestCmd = "relay_cmd";
            }

            var queRequest = new QueueRequest
            {
                Body = body,
                Subcmd = (int) subcmd,
                Completed = callback,
                RequestCmd = requestCmd,
                Running = false,
                Response = response
            };

            // CheckLogin 队列化
            if (subcmd == (int)ClientSendServerReqWrap2Cmd.ECmdCheckLoginReq)
            {
                queRequest.BeforeRequest = () =>
                {   
                    CheckLoginStatus.SetStatus(CheckLoginStatus.StatusType.Checking);
                };
                queRequest.AfterRequest = (seq) =>
                {
                    // Debugger.Log("CHECKLOGIN", seq);
                };
            };

            // 房间操作队列化
            var queue = BaseNetUtil._roomCmd.Contains((ClientSendServerReqWrap2Cmd)subcmd) ? BaseNetUtil._roomQueue : BaseNetUtil._checkLoginQueue;
            return queue.Count == 0 ? SendRequest(queRequest) : PushRequest(queRequest, queue);
        }
        private string SendRequest(QueueRequest queRequest)
        {
            queRequest.Running = true;
            queRequest.BeforeRequest?.Invoke();
            var seq = client.SendRequest(queRequest.Body, queRequest.Subcmd, queRequest.Response, queRequest.Completed, queRequest.RequestCmd, "");

            queRequest.AfterRequest?.Invoke(seq);

            return seq;
        }

        private string PushRequest(QueueRequest queRequest, Queue<QueueRequest> queue)
        {
            var callback = queRequest.Completed;
            Action<ResponseEvent> requestCompleted = (ResponseEvent seq) =>
            {
                callback(seq);
                queRequest.Running = false;
                queue.Dequeue();
                QueueLoop(queue);
            };
            queRequest.Completed = requestCompleted;
            queue.Enqueue(queRequest);
            return QueueLoop(queue);
        }

        private string QueueLoop(Queue<QueueRequest> queue)
        {
            if (queue.Count == 0 || queue.Peek().Running)
            {
                return "NO_SEQ";
            }
            var queRequest = queue.Peek();
            return SendRequest(queRequest);
        }

    }
}