using System;
using Google.Protobuf;


namespace com.unity.mgobe.src.Util {
    public struct DecodeBstResult {
        public ServerSendClientBstWrap1 BstWrap1 { get; set; }

        public ServerSendClientBstWrap2 BstWrap2 { get; set; }

        public ByteString Body { get; set; }
    }

    public struct DecodeRspResult {
        // public byte[] body;
        public DecodeRspResult (ClientSendServerRspWrap1 wrap1, ClientSendServerRspWrap2 wrap2, ByteString data) : this()
        {
            RspWrap1 = wrap1;
            RspWrap2 = wrap2;
            Body = data;
        }

        public ClientSendServerRspWrap1 RspWrap1 { get; set; }

        public ClientSendServerRspWrap2 RspWrap2 { get; set; }

        public ByteString Body { get; set; }
    }

    public class Pb {
        private readonly ClientSendServerReqWrap1 _appReq;
        private ClientSendServerReqWrap2 _clientSendServerReqWrap2;

        public Pb () {
            _appReq = new ClientSendServerReqWrap1 ();
            _clientSendServerReqWrap2 = new ClientSendServerReqWrap2 ();
        }

        public ClientSendServerRspWrap1 AppRes { get; set; }

        public ClientSendServerRspWrap2 ClientSendServerRspWrap2 { get; set; }

        public ByteString Body { get; set; }

        public Action<string> Response { get; set; }

        public byte[] GetRequestByteArray () {
            var payloadBytes = _appReq.ToByteArray ();
            var bytes = new byte[payloadBytes.Length + 6];
            bytes[0] = 0x02;

            var uintValue = (uint) (payloadBytes.Length + 6);
            var uintBytes = BitConverter.GetBytes (uintValue);
            Array.Reverse (uintBytes);

            uintBytes.CopyTo (bytes, 1);
            payloadBytes.CopyTo (bytes, 5);
            bytes[bytes.Length - 1] = 0x03;

            return bytes;
        }

        public static byte[] EncodeReq (ClientSendServerReqWrap1 wrap1, ClientSendServerReqWrap2 wrap2, ByteString data) {
            wrap2.Body = data;
            wrap1.Body = wrap2.ToByteString ();
            return wrap1.ToByteArray ();
        }

        public static DecodeRspResult DecodeRsp (byte[] data) {
            var wrap1 = new ClientSendServerRspWrap1 ();
            wrap1.MergeFrom (data);
            var wrap2 = new ClientSendServerRspWrap2 ();
            wrap2.MergeFrom (wrap1.Body);
            var rsp = wrap2.Body;
            var rspResult = new DecodeRspResult {
                RspWrap1 = new ClientSendServerRspWrap1 (wrap1),
                RspWrap2 = new ClientSendServerRspWrap2 (wrap2),
                Body = rsp
            };
            return rspResult;
        }

        public static DecodeBstResult DecodeBst (byte[] data) {
            // SDKUtil.PrintBytes(data);
            var wrap1 = new ServerSendClientBstWrap1 ();
            wrap1.MergeFrom (data);
            var wrap2 = new ServerSendClientBstWrap2 ();
            wrap2.MergeFrom (wrap1.Body);

            var rsp = wrap2.Msg;

            return new DecodeBstResult {
                BstWrap1 = wrap1,
                    BstWrap2 = wrap2,
                    Body = rsp
            };
        }

    }
}