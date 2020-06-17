

namespace com.unity.mgobe.src.Util {
    public static class UserStatus {
        public enum StatusType : int {
            Login = 1,
            Logining = 2,
            Logout = 3,
            Logouting = 4,
        }

        private static StatusType _status = StatusType.Logout;
        private static int _errCode = 0;
        private static string _errMsg = "";
        private static readonly object _lock = new object();

        public static bool IsStatus (StatusType sdkStatus) {
            lock (_lock) {
                return _status == sdkStatus;
            }
        }

        public static void SetStatus (StatusType sdkStatus) {
            lock (_lock) {
                _status = sdkStatus;
                if (sdkStatus == StatusType.Login)
                    _errCode = 0;
            }

        }

        public static void SetErrCode (int code, string msg) {
            _errCode = code;
            _errMsg = msg;
        }

        public static int GetErrCode () {
            return _errCode;
        }

        public static string GetErrMsg () {
            return _errMsg;
        }
    }

    public static class CheckLoginStatus {
        public enum StatusType : int {
            Checking = 1,
            Checked = 2,
            Offline = 3,
        }

        private static StatusType _status = StatusType.Checking;
        private static string _curRouteId;
        private static readonly object _lock = new object();
        public static bool IsChecked () {
            lock (_lock) {
                // Debugger.Log ("Get checkLoginStatus: {0}", _status);
                return _status == StatusType.Checked;
            }
        }

        public static bool IsOffline () {
            lock (_lock) {
                // Debugger.Log ("Get checkLoginStatus: {0}", _status);
                return _status == StatusType.Offline;
            }
        }

        public static void SetStatus (StatusType checkStatus) {
            lock (_lock) {
                _status = checkStatus;
                // Debugger.Log ("Set checkLoginStatus: {0}", checkStatus);
            }

        }

        public static void SetRouteId (string routeId) {
            _curRouteId = routeId;
        }

        public static string GetRouteId () {
            return _curRouteId;
        }

    }

    public static class GamePlayerInfo {
        private static PlayerInfo _playerInfo = new PlayerInfo ();
        public static PlayerInfo GetInfo () {
            return _playerInfo;
        }

        public static void SetInfo (PlayerInfo info) {
            _playerInfo = info;
        }

        public static void SetInfo (string id) {
            _playerInfo.Id = id;
        }
    }

    public abstract class SdkStatus {
        public enum StatusType : int {
            Inited = 1,
            Initing = 2,
            Uninit = 3,
        }

        private static StatusType _status = StatusType.Uninit;

        public static bool IsInited () {
            return _status == StatusType.Inited;
        }

        public static bool IsIniting () {
            return _status == StatusType.Initing;
        }

        public static bool IsUnInit () {
            return _status == StatusType.Uninit;
        }

        public static void SetStatus (StatusType type) {
            _status = type;
        }
    }

}