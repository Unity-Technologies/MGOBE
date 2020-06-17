using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
// using Newtonsoft.Json;
using com.unity.mgobe.src.Util;
using UnityEngine;

namespace com.unity.mgobe.src.EventUploader {
    public class GetLocationObject {
        public string type;
        public bool altitude;
        public Action<GetLocationSuccessObject> success = null;
        public readonly Action<string> fail = null;
        public Action complete = null;
    }

    public class GetUserInfoObject {
        public bool withCredentials;
        public string lang;
        public Action<GetUserInfoSuccessObject> success = null;
        public Action<string> fail = null;
        public Action complete = null;
    }

    public abstract class GetUserInfoSuccessObject {
        public readonly EventUserInfo userInfo;
        public string rawData;
        public string signature;
        public string encryptedData;
        public string iv;

        protected GetUserInfoSuccessObject (EventUserInfo userInfo) {
            this.userInfo = userInfo;
        }
    }

    public abstract class EventUserInfo {
        public string nickName;
        public string avatarUrl;
        public int gender;
        public string country;
        public string province;
        public string city;
        public string language;
    }

    public abstract class GetLocationSuccessObject {
        public float altitude;
        public float horizontalAccuracy;
        public float latitude;
        public float longitude;
        public double timestamp;
        public float verticalAccuracy;
    }

    public class GetNetworkTypeObject {
        public Action<GetNetworkTypeSuccessObject> success = null;
        public readonly Action<string> fail = null;
        public Action complete = null;
    }

    public abstract class GetNetworkTypeSuccessObject {
        public readonly string networkType;

        protected GetNetworkTypeSuccessObject (string networkType) {
            this.networkType = networkType;
        }
    }

    public static class Adapter {
        private static readonly Dictionary<string, object> Storage = new Dictionary<string, object> ();
        private static readonly HttpClient Client = new HttpClient ();
        private static readonly LocationService LocationService = new LocationService ();
        public static object GetStorageSync (string key) {
            return Storage.ContainsKey (key) ? Storage[key] : null;
        }

        public static void SetStorageSync (string key, object data) {
            Storage.Add (key, data);
        }

        public static object GetSystemInfoSync () {
            return null;
        }

        public static void GetUserInfo (GetUserInfoObject obj) {
            obj.fail?.Invoke ("ERROR");
        }
        public static void GetLocation (GetLocationObject obj) {
            try {
                Debugger.Log ("GetLocation: {0}", Input.location);

                obj.fail?.Invoke ("ERROR");
                // First, check if user has location service enabled
                if (!Input.location.isEnabledByUser)
                    // Start service before querying location
                    Input.location.Start ();

                // Wait until service initializes
                int maxWait = 20;
                while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
                    maxWait--;
                }

                // Service didn't initialize in 20 seconds
                if (maxWait < 1) {
                    Debugger.Log ("Timed out");
                }

                // Connection has failed
                if (Input.location.status == LocationServiceStatus.Failed) {
                    Debugger.Log ("Unable to determine device location");
                } else {
                    // Access granted and location value could be retrieved
                    Debugger.Log ("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
                }
                // Stop service if there is no need to query location updates continuously
                Input.location.Stop ();
            } catch (System.Exception e) {
                Debugger.Log (e.ToString ());
                throw;
            }
        }

        public static void GetNetworkType (GetNetworkTypeObject obj) {
            obj.fail?.Invoke ("ERROR");
        }
        async public static void Request (string url, BeaconData data, Action success, Action fail) {
            // var json = JsonConvert.SerializeObject (data);
            // HttpContent httpContent = new StringContent (json, Encoding.UTF8, "application/json");
            // var response = await Client.PostAsync (url, httpContent);
            // var responseString = await response.Content.ReadAsStringAsync ();
        }

    }

}