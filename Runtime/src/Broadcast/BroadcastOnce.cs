using System;
using System.Collections.Generic;
using System.Linq;


namespace com.unity.mgobe.src.Broadcast
{
    public static class BroadcastOnce
    {
        public const string PlayerSimple = "PLAYER_SIMPLE";
        public const string PlayerComplex = "PLAYER_COMPLEX";

        private static readonly Dictionary<string, List<Action<ResponseEvent>>> Callbacks = new Dictionary<string, List<Action<ResponseEvent>>>();

        public static void Push(string tag, Action<ResponseEvent> callback)
        {
            if (!BroadcastOnce.Callbacks.ContainsKey(tag))
            {
                BroadcastOnce.Callbacks.Add(tag, new List<Action<ResponseEvent>>());
            }
            BroadcastOnce.Callbacks[tag].Add(callback);
        }

        public static void Once(string tag, ResponseEvent eve)
        {
            var cbs = Callbacks.ContainsKey(tag) ? Callbacks[tag] : new List<Action<ResponseEvent>>();
            foreach (var cb in cbs.Where(cb => cb != null))
                cb(eve);
            BroadcastOnce.RemoveCallbacksByTag(tag);
        }

        public static void RemoveCallbacksByTag(string tag)
        {
            if (Callbacks.ContainsKey(tag))
            {
                Callbacks.Remove(tag);
            }
        }
    }
}