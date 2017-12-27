namespace ExitGames.Logging.Log4Net
{
    using System;
    using log4net;

    public sealed class Log4NetLogger : ILogger
    {
        // Fields
        private readonly ILog log;

        // Methods
        public Log4NetLogger(ILog logger)
        {
            this.log = logger;
        }

        public void Debug(object message)
        {
            this.log.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            this.log.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this.log.DebugFormat(format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            this.log.DebugFormat(provider, format, args);
        }

        public void Error(object message)
        {
            this.log.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            this.log.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this.log.ErrorFormat(format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            this.log.ErrorFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            this.log.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            this.log.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this.log.FatalFormat(format, args);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            this.log.FatalFormat(provider, format, args);
        }

        public void Info(object message)
        {
            this.log.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            this.log.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this.log.InfoFormat(format, args);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            this.log.InfoFormat(provider, format, args);
        }

        public void Warn(object message)
        {
            this.log.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            this.log.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.log.WarnFormat(format, args);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            this.log.WarnFormat(provider, format, args);
        }

        // Properties
        public bool IsDebugEnabled
        {
            get
            {
                return this.log.IsDebugEnabled;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return this.log.IsErrorEnabled;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return this.log.IsFatalEnabled;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return this.log.IsInfoEnabled;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return this.log.IsWarnEnabled;
            }
        }

        public string Name
        {
            get
            {
                return this.log.Logger.Name;
            }
        }
    }
}
