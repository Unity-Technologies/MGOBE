using System;

namespace Utils
{
    public class Date
    {
        public static long now()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
    
    public class Config
    {
        public string GameId;
        public string Key;
        public string Domain;
        public string OpenId;
    }
}