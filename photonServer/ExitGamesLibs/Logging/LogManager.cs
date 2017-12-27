using System;
using System.Diagnostics;
using System.Threading;

namespace ExitGames.Logging
{
    /// <summary>
    /// The log manager provides methods to get instances of <see cref="T:ExitGames.Logging.ILogger"/> using a <see cref="T:ExitGames.Logging.ILoggerFactory"/>.
    /// Any logging framework of choice can be used by assigining a new <see cref="T:ExitGames.Logging.ILoggerFactory"/> with <see cref="M:ExitGames.Logging.LogManager.SetLoggerFactory(ExitGames.Logging.ILoggerFactory)"/>.
    /// The default logger factory creates <see cref="T:ExitGames.Logging.ILogger"/> that do not log
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// The number of loggers created.
        /// </summary>
        private static int createLoggerCount;

        /// <summary>
        /// The used logger factory.
        /// </summary>
        private static ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes static members of the <see cref="T:ExitGames.Logging.LogManager"/> class.
        ///  Sets the default logger factory.
        /// </summary>
        static LogManager()
        {
            SetLoggerFactory(null);
        }

        /// <summary>
        /// Gets an <see cref="T:ExitGames.Logging.ILogger"/> for the calling class type.
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Logging.ILogger"/> for the calling class type.</returns>
        public static ILogger GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);
            return GetLogger(frame.GetMethod().DeclaringType.Name);
        }

        /// <summary>
        /// Gets an <see cref="T:ExitGames.Logging.ILogger"/> for the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A new <see cref="T:ExitGames.Logging.ILogger"/> for the specified <paramref name="name"/>.</returns>
        public static ILogger GetLogger(string name)
        {
            return new EmptyLogger(name);
        }

        /// <summary>
        ///  Assigns a new <see cref="T:ExitGames.Logging.ILoggerFactory"/> to create <see cref="T:ExitGames.Logging.ILogger"/> instances.
        /// </summary>
        /// <param name="factory">The new factory. Set null to disable logging.</param>
        public static void SetLoggerFactory(ILoggerFactory factory)
        {
            loggerFactory = factory ?? EmptyLoggerFactory.Instance;
            int num = Interlocked.Exchange(ref createLoggerCount, 0);
            if (num != 0)
            {
                GetCurrentClassLogger().WarnFormat((num == 1) ? "LogManager.SetLoggerFactory: 1 ILogger instance created with previous factory!" : "LogManager.SetLoggerFactory: {0} ILogger instances created with previous factory!", new object[] { num });
            }
        }

        /// <summary>
        /// A logger that does nothing.
        /// </summary>
        private sealed class EmptyLogger : ILogger
        {
            /// <summary>
            /// The name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Initializes a new instance of the <see 
            /// cref="T:ExitGames.Logging.LogManager.EmptyLogger"/> class.
            /// </summary>
            /// <param name="name"></param>
            public EmptyLogger(string name)
            {
                this.name = name;
            }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message</param>
            public void Debug(object message) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Debug(object message, Exception exception) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void DebugFormat(string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void DebugFormat(IFormatProvider provider, string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message</param>
            public void Info(object message) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Info(object message, Exception exception) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void InfoFormat(string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void InfoFormat(IFormatProvider provider, string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message</param>
            public void Warn(object message) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Warn(object message, Exception exception) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void WarnFormat(string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void WarnFormat(IFormatProvider provider, string foramt, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Error(object message) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Error(object message, Exception exception) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void ErrorFormat(string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="provider"> The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void ErrorFormat(IFormatProvider provider, string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Fatal(object message) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Fatal(object message, Exception exception) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void FatalFormat(string format, params object[] args) { }

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void FatalFormat(IFormatProvider provider, string format, params object[] args) { }

            /// <summary>
            /// Gets a value indicating whether IsDebugEnabled.
            /// </summary>
            /// <value>Always false.</value>
            public bool IsDebugEnabled { get { return false; } }

            /// <summary>
            /// Gets a value indicating whether IsErrorEnabled.
            /// </summary>
            /// <value>Always false.</value>
            public bool IsErrorEnabled { get { return false; } }

            /// <summary>
            /// Gets a value indicating whether IsFatalEnabled.
            /// </summary>
            /// <value>Always false.</value>
            public bool IsFatalEnabled { get { return false; } }

            /// <summary>
            /// Gets a value indicating whether IsInfoEnabled.
            /// </summary>
            /// <value>Always false.</value>
            public bool IsInfoEnabled { get { return false; } }

            /// <summary>
            /// Gets a value indicating whether IsWarnEnabled.
            /// </summary>
            /// <value>Always false.</value>
            public bool IsWarnEnabled { get { return false; } }

            /// <summary>
            /// Gets the logger name.
            /// </summary>
            public string Name { get { return this.name; } }
        }

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILoggerFactory"/> that creates <see cref="T:ExitGames.Logging.LogManager.EmptyLogger"/> instances.
        /// Assigning this factory disables logging.
        /// </summary>
        private sealed class EmptyLoggerFactory : ILoggerFactory
        {
            /// <summary>
            /// The singleton instance.
            /// </summary>
            public static readonly EmptyLoggerFactory Instance = new EmptyLoggerFactory();

            /// <summary>
            /// Creates a new <see cref="T:ExitGames.Logging.LogManager.EmptyLogger"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns>A new <see cref="T:ExitGames.Logging.LogManager.EmptyLogger"/></returns>
            public ILogger CreateLogger(string name)
            {
                return new LogManager.EmptyLogger(name);
            }
        }

        /// <summary>
        /// A logger wrapper for lazy logger initialization. This fixes a problem where loggers are created before assigning a custom factory.
        /// </summary>
        private sealed class LazyLoggerWrapper : ILogger
        {
            /// <summary>
            ///  The logger name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// A getter funcation for the logger.
            /// Initially it is mapped to <see cref="M:ExitGames.Logging.LogManager.LazyLoggerWrapper.CreateLogger"/>.
            /// </summary>
            private Func<ILogger> getLogger;

            /// <summary>
            /// The used logger.
            /// </summary>
            private ILogger logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:ExitGames.Logging.LogManager.LazyLoggerWrapper"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public LazyLoggerWrapper(string name)
            {
                this.name = name;
                this.getLogger = new Func<ILogger>(this.CreateLogger);
            }

            /// <summary>
            ///  Creates a new <see cref="T:ExitGames.Logging.ILogger"/> with the current logger factory.
            /// It then switches the <see 
            /// cref="F:ExitGames.Logging.LogManager.LazyLoggerWrapper.getLogger"/> function to <see 
            /// cref="M:ExitGames.Logging.LogManager.LazyLoggerWrapper.GetLogger"/>.
            /// </summary>
            /// <returns>A new <see cref="T:ExitGames.Logging.ILogger"/>.</returns>
            private ILogger CreateLogger()
            {
                LogManager.LazyLoggerWrapper a;

                try
                {
                    if (this.logger == null)
                    {
                        Interlocked.Increment(ref LogManager.createLoggerCount);
                        this.logger = LogManager.loggerFactory.CreateLogger(this.name);
                        Interlocked.Exchange<Func<ILogger>>(ref this.getLogger, new Func<ILogger>(this.GetLogger));
                    }
                }
                finally
                {

                }
                return this.logger;
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Debug(object message)
            {
                this.getLogger().Debug(message);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Debug(object message, Exception exception)
            {
                this.getLogger().Debug(message, exception);
            }

            /// <summary>
            ///Log a message.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void DebugFormat(string format, object[] args)
            {
                this.getLogger().DebugFormat(format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void DebugFormat(IFormatProvider provider, string format, object[] args)
            {
                this.getLogger().DebugFormat(provider, format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Info(object message)
            {
                this.getLogger().Info(message);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Info(object message, Exception exception)
            {
                this.getLogger().Info(message, exception);
            }

            /// <summary>
            ///Log a message.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void InfoFormat(string format, object[] args)
            {
                this.getLogger().InfoFormat(format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void InfoFormat(IFormatProvider provider, string format, object[] args)
            {
                this.getLogger().InfoFormat(provider, format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Warn(object message)
            {
                this.getLogger().Warn(message);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Warn(object message, Exception exception)
            {
                this.getLogger().Warn(message, exception);
            }

            /// <summary>
            ///Log a message.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void WarnFormat(string format, object[] args)
            {
                this.getLogger().WarnFormat(format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void WarnFormat(IFormatProvider provider, string format, object[] args)
            {
                this.getLogger().WarnFormat(provider, format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Error(object message)
            {
                this.getLogger().Error(message);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Error(object message, Exception exception)
            {
                this.getLogger().Error(message, exception);
            }

            /// <summary>
            ///Log a message.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void ErrorFormat(string format, object[] args)
            {
                this.getLogger().ErrorFormat(format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void ErrorFormat(IFormatProvider provider, string format, object[] args)
            {
                this.getLogger().ErrorFormat(provider, format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            public void Fatal(object message)
            {
                this.getLogger().Fatal(message);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public void Fatal(object message, Exception exception)
            {
                this.getLogger().Fatal(message, exception);
            }

            /// <summary>
            ///Log a message.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void FatalFormat(string format, object[] args)
            {
                this.getLogger().FatalFormat(format, args);
            }

            /// <summary>
            /// Log a message.
            /// </summary>
            /// <param name="provider">The provider.</param>
            /// <param name="format">The format.</param>
            /// <param name="args">The args.</param>
            public void FatalFormat(IFormatProvider provider, string format, object[] args)
            {
                this.getLogger().FatalFormat(provider, format, args);
            }

            /// <summary>
            /// Gets a value indicating whether IsDebugEnabled.
            /// </summary>
            public bool IsDebugEnabled
            {
                get
                {
                    return this.getLogger().IsDebugEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether IsErrorEnabled.
            /// </summary>
            public bool IsErrorEnabled
            {
                get
                {
                    return this.getLogger().IsErrorEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether IsFatalEnabled.
            /// </summary>
            public bool IsFatalEnabled
            {
                get
                {
                    return this.getLogger().IsFatalEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether IsInfoEnabled.
            /// </summary>
            public bool IsInfoEnabled
            {
                get
                {
                    return this.getLogger().IsInfoEnabled;
                }
            }

            /// <summary>
            /// Gets a value indicating whether IsWarnEnabled.
            /// </summary>
            public bool IsWarnEnabled
            {
                get
                {
                    return this.getLogger().IsWarnEnabled;
                }
            }

            /// <summary>
            /// Gets Name.
            /// </summary>
            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            /// <summary>
            /// Returns the logger that was created with the factory.
            /// </summary>
            /// <returns>The logger that was created with the factory.</returns>
            private ILogger GetLogger()
            {
                return this.logger;
            }
        }
    }
}
