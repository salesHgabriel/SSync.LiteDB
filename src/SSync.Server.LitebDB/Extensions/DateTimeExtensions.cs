
using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Extensions
{
    public static class DateTimeExtensions
    {
        //private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //private static readonly long MinUnixTimestamp = (long)(DateTime.MinValue - UnixEpoch).TotalSeconds;
        //private static readonly long MaxUnixTimestamp = (long)(DateTime.MaxValue - UnixEpoch).TotalSeconds;

        //public static DateTime FromUnixTimestamp(this long timestamp)
        //{
        //    if (timestamp < MinUnixTimestamp || timestamp > MaxUnixTimestamp)
        //    {
        //        throw new ArgumentOutOfRangeException(nameof(timestamp), $"Unix timestamp must be between {MinUnixTimestamp} and {MaxUnixTimestamp}.");
        //    }

        //    return UnixEpoch.AddSeconds(timestamp);
        //}

        //public static long ToUnixTimestamp(this DateTime dateTime)
        //{
        //    DateTime utcDateTime = dateTime.ToUniversalTime();

        //    if (utcDateTime < UnixEpoch)
        //    {
        //        throw new ArgumentOutOfRangeException(nameof(dateTime), "The DateTime value must be greater than or equal to Unix epoch (1970-01-01).");
        //    }

        //    return (long)(utcDateTime - UnixEpoch).TotalSeconds;
        //}

        public static long ToUnixTimestamp(this DateTime dateTime, Time? time = Time.UTC)
        {
            time ??= Time.UTC;

            var utcOrLocal = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,                       
                DateTimeKind.Local => time == Time.UTC              
                    ? dateTime.ToUniversalTime()
                    : dateTime,                                    
                _ => time == Time.UTC                              
                    ? dateTime.ToUniversalTime()                   
                    : dateTime.ToLocalTime()                       
            };

         
            DateTimeOffset dto = new(utcOrLocal);
            return dto.ToUnixTimeSeconds();
        }
        public static long? ToUnixTimestamp(this DateTime? dateTime, Time? time = Time.UTC)
        {
            if (dateTime.HasValue)
            {
                time ??= Time.UTC;
                var utcOrLocal = dateTime.Value.Kind switch
                {
                    DateTimeKind.Utc => dateTime,                       
                    DateTimeKind.Local => time == Time.UTC              
                        ? dateTime.Value.ToUniversalTime()
                        : dateTime,                                    
                    _ => time == Time.UTC                             
                        ? dateTime.Value.ToUniversalTime()                   
                        : dateTime.Value.ToLocalTime()                       
                };

                // Convert to Unix timestamp
                DateTimeOffset dto = new(utcOrLocal.Value);
                return dto.ToUnixTimeSeconds();
            }
            return null;
        }

        public static DateTime FromUnixTimestamp(this long timestamp, Time? time = Time.UTC)
        {
            time ??= Time.UTC;

            var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);

            return time == Time.UTC ? offset.UtcDateTime : offset.LocalDateTime;
        }

        public static DateTime? FromUnixTimestamp(this long? timestamp, Time? time = Time.UTC)
        {
            if (timestamp is not null)
            {                
                time ??= Time.UTC;

                var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);

                return time == Time.UTC ? offset.UtcDateTime : offset.LocalDateTime;
            }
            return null;
        }
    }
}