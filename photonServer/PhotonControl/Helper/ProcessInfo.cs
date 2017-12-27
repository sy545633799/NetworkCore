using System;
using System.Diagnostics;
using System.Management;

namespace PhotonControl.Helper
{
    public class ProcessInfo
    {
        // Fields
        public const string PhotonExeName = "PhotonSocketServer.exe";
        public readonly Process Process;

        // Methods
        public ProcessInfo(Process process)
        {
            try
            {
                this.Process = process;
                this.Command = PhotonCommand.Unknown;
                ManagementObject obj2 = new ManagementObject(string.Format("Win32_Process.Handle='{0}'", process.Id));
                obj2.Get();
                obj2.Dispose();
                string str2 = (string)obj2["ExecutablePath"];
                string str3 = (string)obj2["CommandLine"];
                this.ExecutablePath = str2;
                int index = str3.IndexOf("/", StringComparison.Ordinal);
                this.CommandLine = (index > 0) ? str3.Substring(index) : string.Empty;
                this.ParseCommandLine();
            }
            catch (ManagementException exception)
            {
                if (exception.ErrorCode != ManagementStatus.NotFound)
                {
                    throw;
                }
            }
        }

        private void ParseCommandLine()
        {
            this.IsValid = true;
            string[] args = this.CommandLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/run":
                    case "/debug":
                        this.Command = PhotonCommand.RunAsApplication;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/service":
                        this.Command = PhotonCommand.RunAsService;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/help":
                        this.Command = PhotonCommand.Help;
                        break;

                    case "/install":
                        this.Command = PhotonCommand.InstallService;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/installcounters":
                        this.Command = PhotonCommand.InstallCounters;
                        break;

                    case "/remove":
                        this.Command = PhotonCommand.RemoveService;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/removecounters":
                        this.Command = PhotonCommand.RemoveCounters;
                        break;

                    case "/stop":
                        this.Command = PhotonCommand.StopAll;
                        break;

                    case "/stop1":
                        this.Command = PhotonCommand.StopInstance;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/pause":
                        this.Command = PhotonCommand.PauseAll;
                        break;

                    case "/pause1":
                        this.Command = PhotonCommand.PauseInstance;
                        this.TryParseInstanceName(args, ++i);
                        break;

                    case "/config":
                        this.Config = this.TryParseArgument(args, ++i);
                        break;

                    case "/configpath":
                        this.ConfigPath = this.TryParseArgument(args, ++i);
                        break;

                    case "/nomessages":
                    case "/\\photonsocketserver.exe\"":
                        break;

                    default:
                        this.IsValid = false;
                        break;
                }
            }
        }

        private string TryParseArgument(string[] args, int index)
        {
            if (args.Length <= index)
            {
                this.IsValid = false;
                return string.Empty;
            }
            return args[index];
        }

        private void TryParseInstanceName(string[] args, int index)
        {
            if (args.Length <= index)
            {
                this.IsValid = false;
            }
            else if (!string.IsNullOrEmpty(this.InstanceName))
            {
                this.IsValid = false;
            }
            else
            {
                this.InstanceName = args[index];
            }
        }

        // Properties
        public PhotonCommand Command { get; private set; }

        public string CommandLine { get; private set; }

        public string Config { get; private set; }

        public string ConfigPath { get; private set; }

        public string ExecutablePath { get; private set; }

        public string InstanceName { get; private set; }

        public bool IsValid { get; private set; }

        // Nested Types
        public enum PhotonCommand
        {
            Help = 10,
            InstallCounters = 5,
            InstallService = 4,
            PauseAll = 8,
            PauseInstance = 9,
            RemoveCounters = 7,
            RemoveService = 6,
            RunAsApplication = 0,
            RunAsService = 1,
            StopAll = 2,
            StopInstance = 3,
            Unknown = -1
        }
    }

}
