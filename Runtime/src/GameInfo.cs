using System;


namespace com.unity.mgobe
{
    public class GameInfo
    {
        public GameInfo()
        {
        }

        public static string SecretKey { get; set; } = "";

        public static string OpenId { get; set; } = "";

        public static string GameId { get; set; } = "";

        public static Action<Signature> CreateSignature { get; set; } = null;

        public string DeviceId { get; set; } = "";

        public static void Assign(GameInfoPara gameInfo)
        {
            OpenId = gameInfo.OpenId;
            GameId = gameInfo.GameId;
            SecretKey = gameInfo.SecretKey;
        }

    }
}