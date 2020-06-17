using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using com.unity.mgobe.src.Util;
using UnityEngine;
using UnityEngine.Serialization;

// 灯塔 SDK Unity 1.0.7
namespace com.unity.mgobe.src.EventUploader {
    [Serializable]
    public class MsgData {
        public string id;
        public float start;
        public float status;
        public int duration;
        public List<BatchEvent> events = new List<BatchEvent> ();
    }

    [Serializable]
    public class BatchEvent {
        public int count;
        public float start;
        public string name;
        public BaseEventParam @params;
        public BatchEvent (int count, float start, string name, BaseEventParam param) {
            this.count = count;
            this.start = start;
            this.name = name;
            this.@params = param ?? new BaseEventParam ();
        }
    }

    [Serializable]
    public class Msg {
        public int type;
        public MsgData data;
    }

    [Serializable]
    public class BaseEvent {
        public string eventName;
        public BaseEventParam @params;
        public BaseEvent (string eventName) {
            this.@params = new BaseEventParam ();
            this.eventName = eventName;
        }
        public BaseEvent (BaseEventParam param, string eventName) {
            this.@params = param;
            this.eventName = eventName;
        }
    }

    [Serializable]
    public class BeaconData {
        public string deviceId;
        public string appkey = Conf.AppKey;
        public string versionCode = Conf.Version;
        public string initTime;
        public string channelId = Conf.ChannelId;
        public string sdkVersion = BeaconSdk.SdkVersion;
        public string pixel;
        public string language;
        public string model;
        public string wxVersion;
        public string system;
        public string platform;
        public string windowArea;
        public string networkType;
        public string opid = BeaconSdk.opid;
        public string unid = BeaconSdk.unid;
        public string userInfo;
        public string location;
        public List<Msg> msgs;
    }

    public static class Util {
        private static BeaconSdk _beacon = null;
        public static void Init (BeaconSdk instance) {
            _beacon = instance;
        }
        public static long GetTime () {
            long retval = 0;
            var st = new DateTime (1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime () - st);
            retval = (long) (t.TotalMilliseconds + 0.5);
            return retval;
        }

        private static string GetSystemInfo () {
            return "";
        }

        private static string GetRandom () {
            return Convert.ToString (1e6 * Util.GetTime () + Math.Floor (1e6 * new System.Random ().NextDouble ()), CultureInfo.InvariantCulture);
        }

        private static string GetUuid () {
            var uuid = (string) Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.U);
            if (uuid != null) return uuid;

            uuid = GetRandom ();
            Adapter.SetStorageSync (BeaconSdk.Prefix + BeaconSdk.U, uuid);

            return uuid;
        }

        public static string GetNetworkType () {
            var type = Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.Nt);
            if (type != null) {
                return (string) type;
            }
            return "4g";
        }

        private static void SetNetworkType () {
            var network = new GetNetworkTypeObject {
                success = (res) => { Adapter.SetStorageSync (BeaconSdk.Prefix + BeaconSdk.Nt, res.networkType); }
            };
            Adapter.GetNetworkType (network);
        }

        private static string GetLocation () {
            var location = Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.Lt);
            if (location != null && Conf.GetLocation) {
                return (string) location;
            }
            return "";
        }

        private static void SetLocation () {
            if (!Conf.GetLocation) return;
            var localObj = new GetLocationObject ();
            localObj.type = Conf.LocationType != "" ? Conf.LocationType : "wgs84";
            localObj.success = (res) => {
                Adapter.SetStorageSync (BeaconSdk.Prefix + BeaconSdk.Lt, res.ToString ());
            };
            // Adapter.getLocation(localObj);
        }

        private static string GetUserInfo () {
            var userInfo = Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.Ui);
            if (Conf.GetUserInfo && userInfo != null) {
                return (string) userInfo;
            }
            return "";
        }

        private static void SetUserInfo () {
            if (!Conf.GetUserInfo) return;
            var userInfo = new GetUserInfoObject {
                withCredentials = false,
                    success = (res) => {
                        var n = res.userInfo;
                        // n.nickName = encodeURIComponent(n.nickName);
                        Adapter.SetStorageSync (BeaconSdk.Prefix + BeaconSdk.Ui, n.ToString ());
                    },
                    fail = (res) => { }
            };
        }

        public static void SetOpenId (string e) {
            if (e.Length <= 0) return;
            BeaconSdk.opid = e;
            Adapter.SetStorageSync (BeaconSdk.Prefix + BeaconSdk.Oik, e);
        }

        public static void InitData () {
            BeaconSdk.serverUrl = Conf.IsDebug ? "https://jrlts.wxlagame.com/analytics/upload?tp=weapp" : "https://report.wxlagame.com/analytics/upload?tp=weapp";
            SetLocation ();
            SetUserInfo ();
            SetNetworkType ();
            BeaconSdk.opid = (string) Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.Oik);
            BeaconSdk.unid = (string) Adapter.GetStorageSync (BeaconSdk.Prefix + BeaconSdk.Uik);
        }

        public static void SendEvent (int status, float start) {
            if (start <= 0) {
                start = GetTime ();
            }
            const int duration = 0;
            var events = new List<BatchEvent> ();
            Request (status, start, duration, events);
        }

        public static void Request (int status, float start, int duration, List<BatchEvent> events, Action<bool> callback = null) {
            if (start <= 0) {
                start = GetTime ();
            }
            var msgData = new MsgData {
                id = GetRandom (),
                start = start,
                status = status,
                duration = duration,
                events = events
            };
            // if (events.Count > 0) {
            //     Debugger.Log ("Beacon data: {0}",  events[0].param);
            // }
            var msgs = new List<Msg> { new Msg { type = 2, data = msgData, } };

            var sysInfo = GetSystemInfo ();
            var deviceId = GetUuid ();

            var data = new BeaconData {
                userInfo = GetUserInfo (),
                location = GetLocation (),
                msgs = msgs
            };

            void Success () {
                callback?.Invoke (true);
            }

            void Fail () {
                callback?.Invoke (false);
            }
            var task = Task.Run (() => Adapter.Request (BeaconSdk.serverUrl, data, Success, Fail));
            task.Wait ();
        }

    }
    public class BeaconSdk {
        public static string serverUrl = null;
        public static string opid = null;
        public static string unid = null;
        public const string Prefix = "beacon_";
        public const string SdkVersion = "weapp_v1.0.7";
        public const string U = "u";
        public const string Ui = "ui";
        public const string Lt = "lt";
        public const string Nt = "nt";
        public const string Oik = "oik";
        public const string Uik = "uik";
        private float _atl = 0;
        public float ats = 0;
        public float ptl = 0;
        public float pts = 0;

        public void Init () {
            Util.Init (this);
            if (Conf.IsAppKetyValid () && Conf.IsVersionValid ()) {
                Util.InitData ();
                this._atl = Util.GetTime ();
                Util.SendEvent (1, this._atl);
            } else {
                Debugger.Log ("beacon_error:init data invalid");
            }
        }

        public void OnEvent (string eventName, BaseEventParam param, Action<bool> callback) {
            var start = Util.GetTime ();
            var events = new List<BatchEvent> ();
            if (param == null) param = new BaseEventParam ();
            events.Add (new BatchEvent (1, start, eventName, param));
            Util.Request (4, start, 0, events, callback);
        }

        public static void OnEvents (IEnumerable<BaseEvent> events, Action<bool> callback) {
            var start = Util.GetTime ();
            var eventsBatch = events.Select (eve => new BatchEvent (1, start, eve.eventName, eve.@params)).ToList ();
            if (eventsBatch.Count > 0) {
                Util.Request (4, start, 0, eventsBatch, callback);
            }
        }
    }
}