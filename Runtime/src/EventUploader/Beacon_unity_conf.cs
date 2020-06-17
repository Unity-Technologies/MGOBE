namespace com.unity.mgobe.src.EventUploader
{
    public static class Conf
    {
        public static readonly string AppKey = "MA0NCELB39H5S6"; // 小程序appKey，从灯塔官网获取
        public static readonly string Version = "1.0.0"; // 小程序版本号
        public static readonly string ChannelId = "9"; // 小程序渠道号，可不填
        public static readonly bool GetLocation = true; // 获取当前的地理位置、速度，默认开启
        public static readonly bool GetUserInfo = false; // 获取用户信息，默认关闭
        public static readonly bool IsDebug = false; // SDK实时联调，默认关闭，发布正式环境时务必关闭
        public static readonly string LocationType = "";

        public static bool IsAppKetyValid()
        {
            return AppKey != null && AppKey.Length > 0;
        }

        public static bool IsVersionValid()
        {
            return Version != null && Version.Length > 0;
        }
    }
}