using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using com.unity.cloudbase;

namespace com.unity.mgobe.Editor {
    public class UserInfo {
        public string organizationId;
        public string organizationName;
        public string projectId;
        public string projectName;
        public string userId;
        public string userName;
    }

    [ExecuteInEditMode, InitializeOnLoad]
    internal class TcbClient {
        // private static UserInfo _userInfo;
        private static Dictionary<string, dynamic> _userInfo;
        static TcbClient () {
            EditorApplication.update += InitUserInfo;
        }

        static void InitUserInfo () {
            if (_userInfo == null) {
                _userInfo = new Dictionary<string, dynamic> { { "organizationId", CloudProjectSettings.organizationId },
                { "organizationName", CloudProjectSettings.organizationName },
                { "projectId", CloudProjectSettings.projectId },
                { "projectName", CloudProjectSettings.projectName },
                { "userId", CloudProjectSettings.userId },
                { "userName", CloudProjectSettings.userName },
                { "serviceType", "mgobe" }
                };
            }
        }

        async public static void updateUserInfo () {
            CloudBaseApp app = CloudBaseApp.Init ("59eb4700a3c34", 3000);
            AuthState state = await app.Auth.GetAuthStateAsync ();

            if (state == null) {
                // 匿名登录
                state = await app.Auth.SignInAnonymouslyAsync ();
            }

            // 调用云函数
            FunctionResponse res = await app.Function.CallFunctionAsync ("updateUserInfo", _userInfo);
        }
    }
}