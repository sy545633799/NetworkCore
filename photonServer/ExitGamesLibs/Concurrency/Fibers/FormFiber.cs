using System.ComponentModel;
using ExitGames.Concurrency.Core;

namespace ExitGames.Concurrency.Fibers
{
    ///<summary>
    /// Allows interaction with Windows Forms.  Transparently moves actions onto the Form's thread.
    ///</summary>
    public class FormFiber : GuiFiber
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        public FormFiber(ISynchronizeInvoke invoker, IExecutor executor)
            : base(new FormAdapter(invoker), executor)
        {
        }
    }
}
