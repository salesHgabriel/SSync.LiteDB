namespace SSync.Client.LitebDB.Abstractions.Exceptions
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