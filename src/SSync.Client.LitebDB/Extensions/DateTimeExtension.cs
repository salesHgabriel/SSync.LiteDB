using SSync.Client.LitebDB.Enums;

namespace SSync.Client.LitebDB.Extensions
{
    public static class DateTimeExtension
    {
        public static bool IsFirstPull(this DateTime dateTime) => dateTime == DateTime.MinValue;
        public static DateTime GetDaTimeFromConfig(this DateTime dateTime, Time? time)
        {
            time ??= Time.UTC;

            return time == Time.UTC ? DateTime.UtcNow : DateTime.Now;
        }

        public static DateTime ParseDaTimeFromConfig(DateTime dateTime, Time? time)
        {
            time ??= Time.UTC;

            return time == Time.UTC ? DateTime.UtcNow : DateTime.Now;
        }
    }
}
