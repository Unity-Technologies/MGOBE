using System;
using UnityEngine;

namespace Packages.com.unity.mgobe.Runtime.src.Util {
    public static class Debugger {
        private static bool _enable = true;
        private static Action _callback = null;
        public static void Log (string format, params object[] args) {
            if (!_enable)
                return;
            // Console.WriteLine(String.Format(format, args));
            var str = String.Format (format, args);
            Debug.Log (str);
            _callback?.Invoke ();
        }
    }
}