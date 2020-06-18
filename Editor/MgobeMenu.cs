using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace com.unity.mgobe.Editor {
    internal static class MgobeMenu {
        public const string MgobeRootMenu = "Mgobe";

        // [MenuItem (MgobeRootMenu)]
        // private static void CreateBlenderSettingAsset () {
        //     // ScriptableObjectUtility.Create<CinemachineBlenderSettings>();
        // }

        [MenuItem (MgobeRootMenu + "/ Mgobe 对战引擎控制台")]
        private static void RedirectToTencentCloud () {
            var task = Task.Run (TcbClient.updateUserInfo);
            task.Wait ();
            Application.OpenURL ("https://console.cloud.tencent.com/mgobe?utm_source=unity");
        }

        [MenuItem (MgobeRootMenu + "/ 使用指引")]
        private static void RedirectToUnity () {
            var task = Task.Run (TcbClient.updateUserInfo);
            task.Wait ();
            Application.OpenURL ("https://unity.cn/mgobe");
        }
    }
}