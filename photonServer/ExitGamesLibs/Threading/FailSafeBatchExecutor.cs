//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ExitGames.Threading
//{
//public class FailSafeBatchExecutor : IExecutor
//{
//    // Fields
//    private static readonly ILogger #a = LogManager.GetCurrentClassLogger();
//    private readonly Action<Exception> #b;
//    private bool #c;

//    // Methods
//    public FailSafeBatchExecutor() : this(new Action<Exception>(#a.Error))
//    {
//    }

//    public FailSafeBatchExecutor(Action<Exception> exceptionHandler)
//    {
//        this.#c = true;
//        this.#b = exceptionHandler;
//    }

//    public void Execute(Action action)
//    {
//        if (this.#c)
//        {
//            try
//            {
//                .~(action);
//            }
//            catch (ThreadAbortException)
//            {
//                throw;
//            }
//            catch (OutOfMemoryException)
//            {
//                throw;
//            }
//            catch (Exception exception)
//            {
//                this.#b(exception);
//            }
//        }
//    }

//    public void Execute(List<Action> actionList)
//    {
//        foreach (Action action in actionList)
//        {
//            this.Execute(action);
//        }
//    }

//    // Properties
//    public bool IsEnabled
//    {
//        get
//        {
//            return this.#c;
//        }
//        set
//        {
//            this.#c = value;
//        }
//    }
//}

//}
