using System.Diagnostics;
using PhotonHostRuntimeInterfaces;

namespace PhotonHostRuntime
{
    public class WindowsPerfCounter
    {
        // Fields
        private PerformanceCounterCategory category;
        private const string CategoryName = "Photon Socket Server: Managed Application";
        private const string CounterNamePeers = "Peers";
        private const string CounterNamePolicyPeers = "Policy:Peers";
        private PerformanceCounter currentPeerCounter;
        private PerformanceCounter currentPolicyPeerCounter;
        private static object perfCounterInstallSync = new object();
        private ILogToUnmanagedLog unmanagedLog;

        // Methods
        public WindowsPerfCounter(ILogToUnmanagedLog unmanagedLog)
        {
            this.unmanagedLog = unmanagedLog;
        }

        public void Decrement(IPhotonPeer peer)
        {
        }

        public void Increment(IPhotonPeer peer)
        {
        }

        public void InitializePerformanceCounter(string instanceName)
        {
        }
    }
}
