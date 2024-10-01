namespace SSync.Client.LitebDB.Exceptions
{
    public class PullChangesException : Exception
    {
        public PullChangesException()
        {
        }

        public PullChangesException(string? message) : base(message)
        {
        }

        public PullChangesException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}