namespace SSync.Client.LitebDB.Exceptions
{
    public class SSyncLiteDBExcepetion : Exception
    {
        public SSyncLiteDBExcepetion()
        {
        }

        public SSyncLiteDBExcepetion(string? message) : base(message)
        {
        }

        public SSyncLiteDBExcepetion(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}