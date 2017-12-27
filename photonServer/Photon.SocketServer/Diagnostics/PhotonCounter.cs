using System.Diagnostics;
using ExitGames.Diagnostics.Counter;
using ExitGames.Diagnostics.Monitoring;

namespace Photon.SocketServer.Diagnostics
{
    public static class PhotonCounter
    {
        // Fields
        [PublishCounter("AvrgOpExecTime")]
        public static readonly AverageCounter AverageOperationExecutionTime = new AverageCounter();
        [PublishCounter("EventsSentCount")]
        public static readonly NumericCounter EventSentCount = new NumericCounter();
        [PublishCounter("EventsSentPerSec")]
        public static readonly CountsPerSecondCounter EventSentPerSec = new CountsPerSecondCounter();
        [PublishCounter("InitPerSec")]
        public static readonly CountsPerSecondCounter InitPerSec = new CountsPerSecondCounter();
        [PublishCounter("OpReceiveCount")]
        public static readonly NumericCounter OperationReceiveCount = new NumericCounter();
        [PublishCounter("OpReceivePerSec")]
        public static readonly CountsPerSecondCounter OperationReceivePerSec = new CountsPerSecondCounter();
        [PublishCounter("OpResponseCount")]
        public static readonly NumericCounter OperationResponseCount = new NumericCounter();
        [PublishCounter("OpResponsePerSec")]
        public static readonly CountsPerSecondCounter OperationResponsePerSec = new CountsPerSecondCounter();
        [PublishCounter("OperationsFast")]
        public static readonly CountsPerSecondCounter OperationsFast = new CountsPerSecondCounter();
        [PublishCounter("OperationsMaxTime")]
        public static readonly NumericCounter OperationsMaxTime = new NumericCounter();
        [PublishCounter("OperationsMiddle")]
        public static readonly CountsPerSecondCounter OperationsMiddle = new CountsPerSecondCounter();
        [PublishCounter("OperationsSlow")]
        public static readonly CountsPerSecondCounter OperationsSlow = new CountsPerSecondCounter();
        [PublishCounter("SessionCount")]
        public static readonly NumericCounter SessionCount = new NumericCounter();

        // Methods
        public static long GetElapsedMilliseconds(long timestamp)
        {
            long num = (Stopwatch.GetTimestamp() - timestamp) * 0x3e8L;
            return (num / Stopwatch.Frequency);
        }

        public static long GetTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public static void OnOperationCompleted(long startTimestamp)
        {
            long elapsedMilliseconds = GetElapsedMilliseconds(startTimestamp);
            AverageOperationExecutionTime.IncrementBy(elapsedMilliseconds);
            OperationResponseCount.Increment();
            OperationResponsePerSec.Increment();
            if (elapsedMilliseconds < 50L)
            {
                OperationsFast.Increment();
            }
            else if (elapsedMilliseconds < 200L)
            {
                OperationsMiddle.Increment();
            }
            else
            {
                OperationsSlow.Increment();
            }
            if (elapsedMilliseconds > OperationsMaxTime.RawValue)
            {
                OperationsMaxTime.RawValue = elapsedMilliseconds;
            }
        }
    }
}
