using System;
using System.Collections;
using PhotonHostRuntimeInterfaces;

namespace PhotonHostRuntime
{
    public interface ILicensing
    {
        // Methods
        string GetHardwareId(bool cpu, bool hdd, bool mac, bool mainboard, bool bios, bool os);
        SortedList GetLicenseInformation(string licensefile);
        int GetLicenseInformation(IPhotonServerShutdown shutdown, out int maxConcurrentConnections, out string[] validIps, out bool isBootstrap, Version assemblyVersion, ILogToUnmanagedLog unmanagedLog, PhotonDomainManager photonDomainManager);
    }
}
