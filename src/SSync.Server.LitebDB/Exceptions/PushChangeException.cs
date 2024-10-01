namespace SSync.Server.LitebDB.Exceptions
{
    public class PushChangeException : Exception
    {
        public PushChangeException()
        {
        }

        public PushChangeException(string? message) : base(message)
        {
        }

        public PushChangeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}