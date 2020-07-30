
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Net
{
    public class NetServer : Net
    {
        public NetServer()
        {

        }

        public byte[] BuildData(byte[] data)
        {
            const byte pre = (byte)MessageDataTag.ServerPre;
            const byte end = (byte)MessageDataTag.ServerEnd;
            return BuildData(pre, data, end);
        }

        // 处理接受的广播消息
        public static void HandleMessage(byte[] body)
        {
            var bst = Pb.DecodeBst(body);

            BroadcastCallback handler = null;
            BroadcastHandlers.TryGetValue(bst.BstWrap2.Type, out handler);

            handler?.Invoke(bst, bst.BstWrap1.Seq);
        }

        public void HandleBroadcast(byte[] data)
        {
            HandleMessage(data);
        }

        // 设置广播回调
        public void SetBroadcastHandler(ServerSendClientBstWrap2Type type, BroadcastCallback handler)
        {
            BroadcastHandlers.TryAdd(type, handler);
            bdhandlers.TryAdd(type, null);
        }

    }
}