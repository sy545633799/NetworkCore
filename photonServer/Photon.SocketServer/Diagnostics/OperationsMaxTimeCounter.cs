using System;
using ExitGames.Diagnostics.Counter;

namespace Photon.SocketServer.Diagnostics
{
    /// <summary>
    /// An <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> for the max operation execution time.
    /// The only supported method is <see cref="M:Photon.SocketServer.Diagnostics.OperationsMaxTimeCounter.GetNextValue"/>.
    /// </summary>
    public sealed class OperationsMaxTimeCounter : ICounter
    {
        /// <summary>
        /// The singleton.
        /// </summary>
        public static readonly OperationsMaxTimeCounter Instance = new OperationsMaxTimeCounter();

        /// <summary>
        /// Prevents a default instance of the <see cref="T:Photon.SocketServer.Diagnostics.OperationsMaxTimeCounter"/> class from being created.
        /// </summary>
        private OperationsMaxTimeCounter()
        {
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <returns>
        ///  Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///This is a read only counter.
        ///</exception>
        public long Decrement()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the next value and resets <see cref="F:Photon.SocketServer.Diagnostics.PhotonCounter.OperationsMaxTime"/> to 0.
        /// </summary>
        /// <returns>
        ///  The next value.
        /// </returns>
        public float GetNextValue()
        {
            float nextValue = PhotonCounter.OperationsMaxTime.GetNextValue();
            PhotonCounter.OperationsMaxTime.RawValue = 0L;
            return nextValue;
        }

        /// <summary>
        ///  This method is not supported.
        /// </summary>
        /// <returns>
        ///  Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This is a read only counter.
        ///</exception>
        public long Increment()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="value">
        ///   The value to increment by.
        /// </param>
        /// <returns>
        /// Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This is a read only counter.
        ///</exception>
        public long IncrementBy(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the counter type.
        /// </summary>
        public CounterType CounterType
        {
            get
            {
                return PhotonCounter.OperationsMaxTime.CounterType;
            }
        }

        /// <summary>
        ///  Gets the counter name.
        /// </summary>
        public string Name
        {
            get
            {
                return PhotonCounter.OperationsMaxTime.Name;
            }
        }
    }
}
