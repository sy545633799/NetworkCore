using System;
using System.Management;
using System.ServiceProcess;

namespace PhotonControl.Helper
{
    public class ServiceInfo
    {
        // Fields
        public readonly ServiceController Controller;
        public readonly Guid Id = Guid.NewGuid();
        public readonly string InstanceName;
        public readonly string Path;
        public const string PhotonExeName = "PhotonSocketServer.exe";
        public const string PhotonServicePrefix = "Photon Socket Server: ";

        // Methods
        public ServiceInfo(ServiceController controller)
        {
            this.Controller = controller;
            this.InstanceName = controller.ServiceName.Substring("Photon Socket Server: ".Length);
            ManagementObject obj2 = new ManagementObject(string.Format("Win32_Service.Name='{0}'", controller.ServiceName));
            obj2.Get();
            obj2.Dispose();
            this.Path = (string)obj2.Properties["PathName"].Value;
            if (!string.IsNullOrEmpty(this.Path))
            {
                int index = this.Path.IndexOf("PhotonSocketServer.exe", StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    index += "PhotonSocketServer.exe".Length;
                    this.PhotonServerExePath = this.Path.Substring(0, index);
                }
            }
            object obj3 = obj2.Properties["ProcessId"].Value;
            if ((obj3 != null) && (obj3 is uint))
            {
                this.ProcessId = (int)((uint)obj2.Properties["ProcessId"].Value);
            }
        }

        public void Refresh()
        {
            this.Controller.Refresh();
            ManagementObject obj2 = new ManagementObject(string.Format("Win32_Service.Name='{0}'", this.Controller.ServiceName));
            obj2.Get();
            obj2.Dispose();
            this.ProcessId = (int)((uint)obj2.Properties["ProcessId"].Value);
        }

        // Properties
        public string PhotonServerExePath { get; private set; }

        public int ProcessId { get; private set; }
    }

}
