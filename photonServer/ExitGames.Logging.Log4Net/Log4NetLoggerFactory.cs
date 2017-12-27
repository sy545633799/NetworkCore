namespace ExitGames.Logging.Log4Net
{
    using System.Diagnostics;
    using LogManager = log4net.LogManager;

    public sealed class Log4NetLoggerFactory : ILoggerFactory
    {
        // Fields
        public static readonly Log4NetLoggerFactory Instance = new Log4NetLoggerFactory();

        // Methods
        private Log4NetLoggerFactory()
        {
        }

        public ILogger CreateLogger(string name)
        {
            StackFrame frame = new StackFrame(3, false);
            return new Log4NetLogger(LogManager.GetLogger(frame.GetMethod().DeclaringType.Assembly, name));
        }
    }
}
