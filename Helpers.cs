using System;

namespace nsreporter
{
    public static class Helpers
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(string unixTime)
        {
            return Helpers.FromUnixTime(Convert.ToInt64(unixTime));
        }
        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
    }
}