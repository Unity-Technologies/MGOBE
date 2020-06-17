using System;
using System.Collections.Generic;
using System.Linq;
using com.unity.mgobe.src.Util.Def;
using com.unity.cloudbase;

namespace com.unity.mgobe.src.Util {
    public static class SdkUtil {
        private static int _seqNum = 1;
        public static string GetSequenceStr () {
            var seqStr = _seqNum.GetHashCode ().ToString ();
            _seqNum++;

            if (_seqNum >= int.MaxValue) {
                _seqNum = 1;
            }

            return seqStr;
        }

        public static void PrintBytes (IEnumerable<byte> data) {
            var str = data.Aggregate ("", (current, t) => current + (t.ToString ("x02") + " "));
            Debugger.Log (str);
        }

        public static int ErrCodeConvert (int code) {
            return code < 0 ? ErrCode.EcInnerError : code;
        }

        public static string ErrCodeConvert (int code, string msg) {
            return code < 0 ? string.Format ("服务器内部错误[{0]", msg) : msg;
        }
        public static ulong GetCurrentTimeSeconds () {
            return Convert.ToUInt64 ((DateTime.Now.ToUniversalTime () - new DateTime (1970, 1, 1)).TotalSeconds);
        }

        public static long GetCurrentTimeMilliseconds () {
            var st = new DateTime (1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime () - st);
            return Convert.ToInt64 (t.TotalMilliseconds + 0.5);
        }

        async public static void UploadMgobeUserInfo (string gameId) {
            CloudBaseApp app = CloudBaseApp.Init ("59eb4700a3c34", 3000);
            AuthState state = await app.Auth.GetAuthStateAsync ();
            if (state == null) {
                // 匿名登录
                state = await app.Auth.SignInAnonymouslyAsync ();
            }
            // 调用云函数
            FunctionResponse res = await app.Function.CallFunctionAsync ("uploadServiceUser", new Dictionary<string, dynamic> { { "gameId", gameId }, { "serviceType", "mgobe" } });
        }
    }
}