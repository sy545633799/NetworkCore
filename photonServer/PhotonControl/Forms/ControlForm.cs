using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using PhotonControl.Helper;
using PhotonHostRuntime;

namespace PhotonControl.Forms
{
    public partial class ControlForm : Form
    {
        private const string ControlRegKey = "PhotonControl";

        private readonly string counterServiceName;

        private bool currentVersionLicenseMonitorUpdated;
        private readonly string dashBoardDirectory;
        private const string DefaultTextForIPs = "(still detecting)";
        private string gsConfiguredIp;

        private readonly string[] instanceNames;
        private readonly System.Windows.Forms.Timer intervalTimer;
        private static bool isWaiting;
        private string latestSdkVersionNumber;
        private readonly List<ToolStripMenuItem> listInstallServiceItems;
        private readonly List<ToolStripMenuItem> listRemoveServiceItems;
        private readonly List<ToolStripMenuItem> listRestartServiceItems;
        private readonly List<ToolStripMenuItem> listStartAppItems;
        private readonly List<ToolStripMenuItem> listStartServiceItems;
        private readonly List<ToolStripMenuItem> listStopAppItems;
        private readonly List<ToolStripMenuItem> listStopServiceItems;
        private string[] localIps;
        private readonly object lockObject;
        private readonly object lockObjectIcon;
        private readonly string logCmdDirectory;

        private const string PhotonDownloadUrl = "https://www.exitgames.com/Download/Photon";

        private const string PhotonServicePrefix = "Photon Socket Server: ";
        private readonly string photonWorkingDirectory;
        private readonly string procDumpCommand;
        private string publicIp;

        private static readonly Version requiredFrameworkVersion = new Version(3, 5);
        private bool restartServiceOnStop;
        private static ServiceController serviceToWaitFor;

        private int waitAnimationStep;
        private readonly System.Timers.Timer waitTimer;

        public ControlForm()
        {
            string str;
            string[] strArray;
            this.listInstallServiceItems = new List<ToolStripMenuItem>();
            this.listRemoveServiceItems = new List<ToolStripMenuItem>();
            this.listRestartServiceItems = new List<ToolStripMenuItem>();
            this.listStartAppItems = new List<ToolStripMenuItem>();
            this.listStartServiceItems = new List<ToolStripMenuItem>();
            this.listStopAppItems = new List<ToolStripMenuItem>();
            this.listStopServiceItems = new List<ToolStripMenuItem>();
            this.lockObject = new object();
            this.lockObjectIcon = new object();
            this.gsConfiguredIp = "(still detecting)";
            this.publicIp = "(still detecting)";
            this.InitializeComponent();
            this.InitializeLocalizedFormItems();
            WebRequest.DefaultWebProxy = null;
            if (PhotonControlSettings.Default.Compact)
            {
                this.miDashboard.Enabled = false;
                this.miCounters.Enabled = false;
                this.miFreeLicense.Visible = true;
            }
            if (!FrameworkVersionChecker.CheckRequiredFrameworkVersion(requiredFrameworkVersion, out str))
            {
                MessageBox.Show(str);
            }
            this.counterServiceName = PhotonControlSettings.Default.CounterServiceName;
            if (string.IsNullOrEmpty(PhotonControlSettings.Default.PhotonWorkingDirectory))
            {
                this.photonWorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + @"\";
            }
            else
            {
                this.photonWorkingDirectory = PhotonControlSettings.Default.PhotonWorkingDirectory;
            }
            this.dashBoardDirectory = Path.Combine(this.photonWorkingDirectory, @"..\bin_Tools\dashboard");
            if (!Directory.Exists(this.dashBoardDirectory))
            {
                this.miDashboard.Enabled = false;
            }
            this.logCmdDirectory = Path.Combine(this.photonWorkingDirectory, @"..\bin_Tools\baretail");
            if (!Directory.Exists(this.logCmdDirectory))
            {
                this.miOpenLogs.Enabled = false;
            }
            string path = Path.Combine(this.photonWorkingDirectory, "PhotonServer.config");
            PhotonConfiguration configuration = null;
            if (File.Exists(path))
            {
                configuration = PhotonConfiguration.LoadConfiguration(path);
            }
            if (configuration != null)
            {
                this.instanceNames = new string[configuration.Instances.Count];
                strArray = new string[configuration.Instances.Count];
                for (int j = 0; j < configuration.Instances.Count; j++)
                {
                    this.instanceNames[j] = configuration.Instances[j].Name;
                    strArray[j] = configuration.Instances[j].DisplayName;
                }
            }
            else
            {
                strArray = PhotonControlSettings.Default.Instances.Split(new char[] { ',' });
                this.instanceNames = new string[strArray.Length];
                for (int k = 0; k < strArray.Length; k++)
                {
                    this.instanceNames[k] = strArray[k].Trim().Split(new char[] { ' ' })[0];
                }
            }
            string testClientPaths = PhotonControlSettings.Default.TestClientPaths;
            string testClientArguments = PhotonControlSettings.Default.TestClientArguments;
            string[] strArray2 = testClientPaths.Split(new char[] { ',' });
            string[] strArray3 = testClientArguments.Split(new char[] { ',' });
            int index = 5;
            for (int i = 0; i < this.instanceNames.Length; i++)
            {
                string str7 = strArray[i];
                string str8 = this.instanceNames[i];
                string str9 = (strArray3.Length > i) ? strArray3[i] : null;
                string str10 = (strArray2.Length > i) ? Path.Combine(this.photonWorkingDirectory, strArray2[i]) : null;
                ToolStripMenuItem item = new ToolStripMenuItem();
                this.trayMenuStrip.Items.Insert(index, item);
                index++;
                ToolStripMenuItem item2 = new ToolStripMenuItem();
                ToolStripMenuItem item3 = new ToolStripMenuItem();
                ToolStripSeparator separator = new ToolStripSeparator();
                ToolStripMenuItem item4 = new ToolStripMenuItem();
                ToolStripMenuItem item5 = new ToolStripMenuItem();
                ToolStripMenuItem item6 = new ToolStripMenuItem();
                ToolStripSeparator separator2 = new ToolStripSeparator();
                ToolStripMenuItem item7 = new ToolStripMenuItem();
                ToolStripMenuItem item8 = new ToolStripMenuItem();
                ToolStripSeparator separator3 = new ToolStripSeparator();
                ToolStripMenuItem item9 = new ToolStripMenuItem();
                item.DropDownItems.AddRange(new ToolStripItem[] { item2, item3, separator, item4, item5, item6, separator2, item7, item8, separator3, item9 });
                item.Name = "miService" + str8;
                item.Tag = str8;
                item.Size = new Size(0xbb, 0x16);
                item.Text = str7;
                this.listStopServiceItems.Add(item6);
                this.listStartServiceItems.Add(item4);
                this.listRestartServiceItems.Add(item5);
                this.listStopAppItems.Add(item3);
                this.listStartAppItems.Add(item2);
                this.listInstallServiceItems.Add(item7);
                this.listRemoveServiceItems.Add(item8);
                item2.Name = "miStartApp" + str8;
                item2.Tag = str8;
                item2.Size = new Size(0xae, 0x16);
                item2.Text = Program.ResourceManager.GetString("miStartAppText");
                item2.Click += new EventHandler(this.OnClickPhotonAppStart);
                item3.Name = "miStopApp" + str8;
                item3.Tag = str8;
                item3.Size = new Size(0xae, 0x16);
                item3.Text = Program.ResourceManager.GetString("miStopAppText");
                item3.Click += new EventHandler(this.OnClickPhotonAppStop);
                separator.Name = "toolStripSeparator2" + str8;
                separator.Size = new Size(0xab, 6);
                item4.Name = "miStartService" + str8;
                item4.Tag = str8;
                item4.Size = new Size(0xae, 0x16);
                item4.Text = Program.ResourceManager.GetString("miStartServiceText");
                item4.Click += new EventHandler(this.OnClickPhotonServiceStart);
                item5.Name = "miRestartService" + str8;
                item5.Tag = str8;
                item5.Size = new Size(0xae, 0x16);
                item5.Text = Program.ResourceManager.GetString("miRestartServiceText");
                item5.Click += new EventHandler(this.OnClickPhotonServiceRestart);
                item6.Name = "miStopService" + str8;
                item6.Tag = str8;
                item6.Size = new Size(0xae, 0x16);
                item6.Text = Program.ResourceManager.GetString("miStopServiceText");
                item6.Click += new EventHandler(this.OnClickPhotonServiceStop);
                separator2.Name = "toolStripSeparator7" + str8;
                separator2.Size = new Size(0xab, 6);
                item7.Name = "miInstallService" + str8;
                item7.Tag = str8;
                item7.Size = new Size(0xae, 0x16);
                item7.Text = Program.ResourceManager.GetString("miInstallServiceText");
                item7.Click += new EventHandler(this.OnClickPhotonServiceInstall);
                item8.Name = "miRemoveService" + str8;
                item8.Tag = str8;
                item8.Size = new Size(0xae, 0x16);
                item8.Text = Program.ResourceManager.GetString("miRemoveServiceText");
                item8.Click += new EventHandler(this.OnClickPhotonServiceRemove);
                separator3.Name = "toolStripSeparator12" + str8;
                separator3.Size = new Size(0xab, 6);
                separator3.Visible = !PhotonControlSettings.Default.Compact;
                item9.Visible = !PhotonControlSettings.Default.Compact;
                item9.Enabled = File.Exists(str10) && !PhotonControlSettings.Default.Compact;
                item9.Name = "miRunTestclient" + str8;
                TestClientConfig config = new TestClientConfig
                {
                    Path = str10,
                    Arguments = str9,
                    Instance = str8
                };
                item9.Tag = config;
                item9.Size = new Size(0xae, 0x16);
                item9.Text = string.Format("{0} ({1}", Program.ResourceManager.GetString("miRunTestclientText"), str8);
                item9.Click += new EventHandler(this.OnClickRunTestclient);
            }
            this.gsConfiguredIp = GameServerConfigChanger.GetCurrentConfigIp(PhotonControlSettings.Default.GameServerConfigPaths, out str);
            GameServerConfigChanger.GetPublicIpCompleted += new GameServerConfigChanger.GetPublicIpCompletedHandler(this.OnGetPublicIpCompletec);
            GameServerConfigChanger.GetPublicIPAsync();
            this.CheckVersionOnLicenseMonitorAsync();
            GameServerConfigChanger.GetLocalIPs(out this.localIps);
            this.iconStopped = (Icon)PhotonControl.Properties.Resources.ResourceManager.GetObject("Stopped");
            this.iconRunning = (Icon)PhotonControl.Properties.Resources.ResourceManager.GetObject("Running");
            this.miSdkVersion.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miSdkVersionText"), Application.ProductVersion);
            this.procDumpCommand = Path.Combine(this.photonWorkingDirectory, "procdump.cmd");
            if (!File.Exists(this.procDumpCommand))
            {
                this.miRunProcDump.Enabled = false;
                this.miRunProcDump.Visible = false;
            }
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 500.0
            };
            this.waitTimer = timer;
            this.waitTimer.Elapsed += new ElapsedEventHandler(this.HandleWaitingForInstanceServices);
            this.waitTimer.Start();
            isWaiting = true;
            base.Enabled = false;
            this.trayIcon.Visible = true;
            this.UpdateLicenseInfo();
            base.Enabled = true;
            isWaiting = false;
            PhotonHandler.AddPhotonToFirewall(this.photonWorkingDirectory);
            System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer
            {
                Interval = 0x3e8
            };
            this.intervalTimer = timer2;
            this.intervalTimer.Tick += new EventHandler(this.TimerTick);
            this.intervalTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                this.intervalTimer.Enabled = false;
                this.SetEnabledMenuItems();
            }
            finally
            {
                this.intervalTimer.Enabled = true;
            }
        }

        private void SetEnabledMenuItems()
        {
            lock (this.lockObject)
            {
                int num;
                int num2;
                long num3;
                Dictionary<string, ServiceInfo> photonServices = PhotonHandler.GetPhotonServices("Photon Socket Server: ");
                List<ProcessInfo> list = PhotonHandler.GetActiveProcesses(out num2, out num, out num3);
                string[] instanceNames = this.instanceNames;
                for (int j = 0; j < instanceNames.Length; j++)
                {
                    ServiceInfo info;
                    Predicate<ProcessInfo> match = null;
                    Predicate<ToolStripMenuItem> predicate2 = null;
                    Predicate<ToolStripMenuItem> predicate3 = null;
                    Predicate<ToolStripMenuItem> predicate4 = null;
                    Predicate<ToolStripMenuItem> predicate5 = null;
                    Predicate<ToolStripMenuItem> predicate6 = null;
                    Predicate<ToolStripMenuItem> predicate7 = null;
                    Predicate<ToolStripMenuItem> predicate8 = null;
                    Predicate<ToolStripMenuItem> predicate9 = null;
                    Predicate<ToolStripMenuItem> predicate10 = null;
                    string instanceName = instanceNames[j];
                    bool flag = false;
                    bool flag2 = false;
                    if (photonServices.TryGetValue(instanceName, out info))
                    {
                        flag = true;
                        if (info.Controller.Status != ServiceControllerStatus.Stopped)
                        {
                            flag2 = true;
                        }
                    }
                    if (match == null)
                    {
                        match = p => (p.InstanceName == instanceName) && (p.Command == ProcessInfo.PhotonCommand.RunAsApplication);
                    }
                    ProcessInfo info2 = list.Find(match);
                    bool flag3 = (info2 != null) && !info2.Process.HasExited;
                    if (flag)
                    {
                        if (predicate2 == null)
                        {
                            predicate2 = i => ((string)i.Tag) == instanceName;
                        }
                        this.listStartAppItems.Find(predicate2).Enabled = false;
                        if (predicate3 == null)
                        {
                            predicate3 = i => ((string)i.Tag) == instanceName;
                        }
                        this.listStopAppItems.Find(predicate3).Enabled = false;
                    }
                    else
                    {
                        if (predicate4 == null)
                        {
                            predicate4 = i => ((string)i.Tag) == instanceName;
                        }
                        this.listStartAppItems.Find(predicate4).Enabled = !flag3 && (num < 1);
                        if (predicate5 == null)
                        {
                            predicate5 = i => ((string)i.Tag) == instanceName;
                        }
                        this.listStopAppItems.Find(predicate5).Enabled = flag3;
                    }
                    if (predicate6 == null)
                    {
                        predicate6 = i => ((string)i.Tag) == instanceName;
                    }
                    this.listStartServiceItems.Find(predicate6).Enabled = flag && !flag2;
                    if (predicate7 == null)
                    {
                        predicate7 = i => ((string)i.Tag) == instanceName;
                    }
                    this.listStopServiceItems.Find(predicate7).Enabled = flag2;
                    if (predicate8 == null)
                    {
                        predicate8 = i => ((string)i.Tag) == instanceName;
                    }
                    this.listRestartServiceItems.Find(predicate8).Enabled = flag2;
                    if (predicate9 == null)
                    {
                        predicate9 = i => ((string)i.Tag) == instanceName;
                    }
                    this.listInstallServiceItems.Find(predicate9).Enabled = !flag && !flag3;
                    if (predicate10 == null)
                    {
                        predicate10 = i => ((string)i.Tag) == instanceName;
                    }
                    this.listRemoveServiceItems.Find(predicate10).Enabled = flag;
                    ToolStripMenuItem item = (ToolStripMenuItem)this.trayMenuStrip.Items["miService" + instanceName];
                    if (item != null)
                    {
                        item.Enabled = !isWaiting;
                    }
                }
                if (!isWaiting)
                {
                    lock (this.lockObjectIcon)
                    {
                        if ((num2 + num) > 0)
                        {
                            this.trayIcon.Text = string.Format("{0}. {1} MB", Program.ResourceManager.GetString("trayIconTextRunning"), num3);
                            this.trayIcon.Icon = this.iconRunning;
                        }
                        else
                        {
                            this.trayIcon.Icon = this.iconStopped;
                            this.trayIcon.Text = Program.ResourceManager.GetString("trayIconTextStopped");
                        }
                    }
                }
                if (this.trayMenuStrip.Visible)
                {
                    bool flag5;
                    bool flag6;
                    if (this.currentVersionLicenseMonitorUpdated)
                    {
                        this.currentVersionLicenseMonitorUpdated = false;
                        if (Application.ProductVersion.Equals(this.latestSdkVersionNumber))
                        {
                            this.miSdkVersion.ToolTipText = Program.ResourceManager.GetString("miSdkVersionLatestVersionText");
                            this.miSdkVersion.Checked = true;
                            this.miLatestVersion.Visible = false;
                        }
                        else
                        {
                            this.miSdkVersion.ToolTipText = Program.ResourceManager.GetString("miSdkVersionNewerVersionText");
                            this.miSdkVersion.Checked = false;
                            this.miLatestVersion.Visible = true;
                            this.miLatestVersion.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miLatestVersionText"), this.latestSdkVersionNumber);
                        }
                        if (this.miLicenseMonitorAvailable.Checked)
                        {
                            this.miLicenseMonitorAvailable.Text = Program.ResourceManager.GetString("miLicenseMonitorAvailableIsAvailableText");
                        }
                        else
                        {
                            this.miLicenseMonitorAvailable.Text = Program.ResourceManager.GetString("miLicenseMonitorAvailableIsNotAvailableText");
                        }
                    }
                    this.miLoadBalancingIpMenu.Visible = !string.IsNullOrEmpty(PhotonControlSettings.Default.GameServerConfigPaths);
                    if (this.miLoadBalancingIpMenu.Visible)
                    {
                        string gsConfiguredIp;
                        if (string.IsNullOrEmpty(this.gsConfiguredIp))
                        {
                            gsConfiguredIp = string.Format("\"\" ({0})", Program.ResourceManager.GetString("miCurrentGameServerIPAutodetectText"));
                        }
                        else
                        {
                            gsConfiguredIp = this.gsConfiguredIp;
                        }
                        this.miCurrentGameServerIp.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miCurrentGameServerIpText"), gsConfiguredIp);
                        ToolStripItemCollection dropDownItems = this.miLoadBalancingIpMenu.DropDownItems;
                        if ((this.localIps == null) || (this.localIps.Length == 0))
                        {
                            this.miSetLocalIp.Visible = true;
                            this.miSetLocalIp.Text = "(still detecting)";
                            this.miSetLocalIp.Enabled = true;
                            this.miSetLocalIp.Checked = false;
                        }
                        else
                        {
                            this.miSetLocalIp.Visible = false;
                            this.miSetLocalIp.Text = "(still detecting)";
                            this.miSetLocalIp.Enabled = false;
                            this.miSetLocalIp.Checked = false;
                            int index = dropDownItems.IndexOfKey(this.miSetLocalIp.Name);
                            foreach (string str2 in this.localIps)
                            {
                                ToolStripMenuItem item2;
                                string key = "miLocalIp" + str2;
                                ToolStripItem[] itemArray = dropDownItems.Find(key, false);
                                if (itemArray.Length == 0)
                                {
                                    item2 = new ToolStripMenuItem
                                    {
                                        Name = key,
                                        Size = this.miSetLocalIp.Size,
                                        ToolTipText = this.miSetLocalIp.ToolTipText
                                    };
                                    item2.Click += new EventHandler(this.OnClickGameServerToPrivateIp);
                                    item2.Tag = str2;
                                    item2.Enabled = true;
                                    item2.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miSetLocalIpText"), str2);
                                    dropDownItems.Insert(index, item2);
                                }
                                else
                                {
                                    item2 = (ToolStripMenuItem)itemArray[0];
                                }
                                item2.Checked = str2.Equals(this.gsConfiguredIp);
                            }
                        }
                        this.miSetPublicIp.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miSetPublicIpText"), this.publicIp);
                        this.miSetPublicIp.Enabled = !"(still detecting)".Equals(this.publicIp);
                        this.miSetPublicIp.Checked = this.publicIp.Equals(this.gsConfiguredIp);
                        this.miSetAutodetectIp.Checked = string.IsNullOrEmpty(this.gsConfiguredIp);
                    }
                    bool flag4 = AutoStartHelper.IsAutoStartEnabled("PhotonControl", Assembly.GetExecutingAssembly().Location);
                    this.miAutostartEnable.Checked = flag4;
                    GetServiceInfo(this.counterServiceName, out flag5, out flag6);
                    bool flag7 = File.Exists(Path.Combine(this.dashBoardDirectory, "PhotonDashboard.exe"));
                    this.miDashboard.Enabled = flag7 || flag5;
                    this.miDashboardOpen.Enabled = flag5 && flag6;
                    this.miDashboardStart.Enabled = flag5 && !flag6;
                    this.miDashboardStop.Enabled = flag5 && flag6;
                    this.miDashboardRestart.Enabled = flag6;
                    this.miDashboardInstall.Enabled = !flag5 && flag7;
                    this.miDashboardRemove.Enabled = flag5 && flag7;
                }
            }
        }

        private void GetServiceInfo(string serviceName, out bool installed, out bool running)
        {
            installed = false;
            running = false;
            if (!string.IsNullOrEmpty(serviceName))
            {
                foreach (ServiceController controller in ServiceController.GetServices())
                {
                    if (controller.ServiceName.Equals(serviceName))
                    {
                        installed = true;
                        if (controller.Status != ServiceControllerStatus.Stopped)
                        {
                            running = true;
                        }
                    }
                }
            }
        }

        private void OnClickGameServerToPrivateIp(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            this.SetGameServerIp(item.Tag.ToString());
        }

        private void SetGameServerIp(string ip)
        {
            string str;
            if (!GameServerConfigChanger.EditConfigFiles(PhotonControlSettings.Default.GameServerConfigPaths, ip, out str))
            {
                MessageBox.Show(str, Program.ResourceManager.GetString("gameServerConfigChangeErrorCaption"));
            }
            this.gsConfiguredIp = GameServerConfigChanger.GetCurrentConfigIp(PhotonControlSettings.Default.GameServerConfigPaths, out str);
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str);
            }
        }

        private void UpdateLicenseInfo()
        {
            SortedList licenseInformation = PhotonDomainManager.GetLicenseInformation();
            if (licenseInformation != null)
            {
                DateTime time;
                string str = licenseInformation["Company"] as string;
                if (string.IsNullOrEmpty(str))
                {
                    str = licenseInformation["FirstName"] + " " + licenseInformation["LastName"];
                }
                string str2 = licenseInformation["ConcurrentConnections"] as string;
                if (string.IsNullOrEmpty(str2))
                {
                    str2 = Program.ResourceManager.GetString("licenseBootstrapCCUText");
                    str = Program.ResourceManager.GetString("licenseBootstrapLicenseeText");
                    this.miLicenseeMail.Visible = false;
                    this.miLicensedIPs.Visible = false;
                    this.trayIcon.ShowBalloonTip(0xbb8, Program.ResourceManager.GetString("licenseNotFoundTipTitle"), Program.ResourceManager.GetString("licenseNotFoundTipText") ?? string.Empty, ToolTipIcon.Info);
                    this.miFreeLicense.Visible = true;
                }
                else if (str2.Equals("0"))
                {
                    str2 = Program.ResourceManager.GetString("licenseUnlimitedCCUText");
                }
                string s = licenseInformation["ExpireDate"] as string;
                if (DateTime.TryParse(s, out time) && (DateTime.Now.Date > time.Date))
                {
                    this.trayIcon.ShowBalloonTip(0xbb8, Program.ResourceManager.GetString("licenseExpiredTipTitle"), string.Format(Program.ResourceManager.GetString("licenseExpiredTipText") ?? string.Empty, time.Date.ToShortDateString()), ToolTipIcon.Info);
                }
                this.miLicenseCompany.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miLicenseCompanyText"), str);
                this.miLicenseeMail.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miLicenseeMailText"), licenseInformation["LicenseeMail"]);
                this.miMaxConnections.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miMaxConnectionsText"), str2);
                this.miExpires.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miExpiresText"), licenseInformation["ExpireDate"]);
                this.miFloatingLicense.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miFloatingLicenseText"), string.IsNullOrEmpty(licenseInformation["FloatingLicenseServer"] as string) ? Program.ResourceManager.GetString("no") : Program.ResourceManager.GetString("yes"));
                this.miLicensedIPs.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miLicensedIPsText"), licenseInformation["IP"]);
                this.miHardwareID.Text = string.Format("{0}: {1}", Program.ResourceManager.GetString("miHardwareIDText"), PhotonDomainManager.GetHardwareID(true, false, true, true, true, false));
            }
        }

        private void HandleWaitingForInstanceServices(object sender, ElapsedEventArgs e)
        {
            string format = Program.ResourceManager.GetString("waitText") ?? string.Empty;
            if (serviceToWaitFor != null)
            {
                serviceToWaitFor.Refresh();
                format = string.Format("{0}: {1}", Program.ResourceManager.GetString("waitingForServiceText"), serviceToWaitFor.DisplayName);
                if ((serviceToWaitFor.Status == ServiceControllerStatus.Running) || (serviceToWaitFor.Status == ServiceControllerStatus.Stopped))
                {
                    if (this.restartServiceOnStop)
                    {
                        this.restartServiceOnStop = false;
                        isWaiting = true;
                        serviceToWaitFor.Start();
                    }
                    else
                    {
                        isWaiting = false;
                        serviceToWaitFor = null;
                    }
                }
            }
            if (isWaiting)
            {
                lock (this.lockObjectIcon)
                {
                    switch (this.waitAnimationStep)
                    {
                        case 0:
                            this.trayIcon.Icon = this.iconRunning;
                            break;

                        case 1:
                            this.trayIcon.Icon = this.iconStopped;
                            break;
                    }
                    this.trayIcon.Text = string.Format(format, new object[0]);
                    this.waitAnimationStep = (this.waitAnimationStep + 1) % 2;
                }
            }
        }

        private void OnGetPublicIpCompletec(string publicIpLookupResult)
        {
            this.publicIp = publicIpLookupResult;
        }

        private void CheckVersionOnLicenseMonitorAsync()
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.CheckVersionOnLicenseMonitorAsyncCompleted);
            client.DownloadStringAsync(new Uri("https://licensesp.exitgames.com/verify/PhotonVersionCurrent?versionIn=" + Application.ProductVersion));
        }

        private void CheckVersionOnLicenseMonitorAsyncCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            this.currentVersionLicenseMonitorUpdated = true;
            if ((e.Error == null) && !string.IsNullOrEmpty(e.Result))
            {
                this.miLicenseMonitorAvailable.Checked = true;
                if (Regex.IsMatch(e.Result, @"\d+\.\d+\.\d+\.\d+"))
                {
                    this.latestSdkVersionNumber = e.Result;
                }
            }
        }

        private void OnClickRunTestclient(object sender, EventArgs e)
        {
            TestClientConfig tag = (TestClientConfig)((ToolStripMenuItem)sender).Tag;
            if ((tag == null) || string.IsNullOrEmpty(tag.Path))
            {
                MessageBox.Show(Program.ResourceManager.GetString("testClientConfigErrorMsg"), Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                object obj2;
                Monitor.Enter(obj2 = this.lockObject);
                try
                {
                    string directoryName = Path.GetDirectoryName(tag.Path);
                    Process process = new Process
                    {
                        StartInfo = { FileName = tag.Path }
                    };
                    if (!string.IsNullOrEmpty(tag.Arguments))
                    {
                        process.StartInfo.Arguments = tag.Arguments;
                    }
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        process.StartInfo.WorkingDirectory = directoryName;
                    }
                    process.Start();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Format("{0} \"{1}\":{2}{3}", new object[] { Program.ResourceManager.GetString("testClientFailedMsg"), tag.Instance, Environment.NewLine, exception.Message }), Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                finally
                {
                    Monitor.Exit(obj2);
                }
            }
        }

        private void OnClickPhotonServiceRemove(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                string tag = (string)((ToolStripMenuItem)sender).Tag;
                PhotonHandler.RemoveService(tag, this.photonWorkingDirectory);
            }
        }

        private void OnClickPhotonServiceInstall(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                string tag = (string)((ToolStripMenuItem)sender).Tag;
                PhotonHandler.InstallService(tag, this.photonWorkingDirectory);
            }
        }

        private void OnClickPhotonServiceStop(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                StopServiceByName("Photon Socket Server: " + ((string)((ToolStripMenuItem)sender).Tag));
            }
        }

        private bool StopServiceByName(string name)
        {
            try
            {
                serviceToWaitFor = new ServiceController(name);
                isWaiting = true;
                serviceToWaitFor.Stop();
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format(Program.ResourceManager.GetString("serviceStoppingFailedMsg") ?? string.Empty, name) + exception, Program.ResourceManager.GetString("photonControlErrorCaption"));
                return false;
            }
            return true;
        }

        private void OnClickPhotonServiceRestart(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                this.restartServiceOnStop = true;
                StopServiceByName("Photon Socket Server: " + ((string)((ToolStripMenuItem)sender).Tag));
            }
        }

        private void OnClickPhotonServiceStart(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                StartServiceByName("Photon Socket Server: " + ((string)((ToolStripMenuItem)sender).Tag));
            }
        }

        private void StartServiceByName(string name)
        {
            try
            {
                serviceToWaitFor = new ServiceController(name);
                isWaiting = true;
                serviceToWaitFor.Refresh();
                serviceToWaitFor.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format(Program.ResourceManager.GetString("serviceStartingFailedMsg") ?? string.Empty, name) + exception, Program.ResourceManager.GetString("photonControlErrorCaption"));
            }
        }

        private void OnClickPhotonAppStop(object sender, EventArgs e)
        {
            string str;
            lock (this.lockObject)
            {
                string tag = (string)((ToolStripMenuItem)sender).Tag;
                PhotonHandler.StopPhotonApplication(tag, this.photonWorkingDirectory, out str);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str);
            }
        }

        private void OnClickPhotonAppStart(object sender, EventArgs e)
        {
            string str;
            lock (this.lockObject)
            {
                string tag = (string)((ToolStripMenuItem)sender).Tag;
                PhotonHandler.StartPhotonApplication(tag, this.photonWorkingDirectory, out str);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str);
            }
        }

        private void OnClickHardwareIdToClipboard(object sender, EventArgs e)
        {
            Clipboard.SetText(this.miHardwareID.Text);
        }

        private void OnClickTrayIcon(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.trayIcon, null);
            }
        }

        private void OnMenuVisibilityChange(object sender, EventArgs e)
        {
            this.SetEnabledMenuItems();
        }

        private void OnClickAutostartToggle(object sender, EventArgs e)
        {
            AutoStartHelper.ToggleAutostarting("PhotonControl");
        }

        private void OnClickGetFreeLicense(object sender, EventArgs e)
        {
            Process.Start("https://www.exitgames.com/Download/Photon");
        }

        private void OnClickGameServerToPublicIp(object sender, EventArgs e)
        {
            this.SetGameServerIp(this.publicIp);
        }

        private void OnClickGameServerToAutodetectPublicIP(object sender, EventArgs e)
        {
            this.SetGameServerIp(string.Empty);
        }

        private void OnClickDashboardOpen(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://localhost:8088");
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToOpenDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickDashboardStart(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    StartServiceByName(this.counterServiceName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToStartDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickDashboardRestart(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    StopServiceByName(this.counterServiceName);
                    StartServiceByName(this.counterServiceName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToRestartDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickDashboardStop(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    StopServiceByName(this.counterServiceName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToStopDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickDashboardInstall(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    new Process { StartInfo = { FileName = "PhotonDashboard.exe", Arguments = "-i -s", WorkingDirectory = this.dashBoardDirectory } }.Start();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToInstallDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickDashboardRemove(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    new Process { StartInfo = { FileName = "PhotonDashboard.exe", Arguments = "-u -s", WorkingDirectory = this.dashBoardDirectory } }.Start();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedToRemoveDashboardMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickPerfmonStart(object sender, EventArgs e)
        {
            using (new Wow64RedirectionDisabler())
            {
                Process.Start("perfmon.exe");
            }
        }

        private void OnClickCountersInstall(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                PhotonPerformanceCounter.Install(this.photonWorkingDirectory);
            }
        }

        private void OnClickCountersRemove(object sender, EventArgs e)
        {
            lock (this.lockObject)
            {
                PhotonPerformanceCounter.Remove(this.photonWorkingDirectory);
            }
        }

        private void OnClickPerfmonLoggingCreate(object sender, EventArgs e)
        {
            PerfmonLogging.Create(this.photonWorkingDirectory);
        }

        private void OnClickPerfmonLoggingStart(object sender, EventArgs e)
        {
            PerfmonLogging.Start();
        }

        private void OnClickPerfmonLoggingStop(object sender, EventArgs e)
        {
            PerfmonLogging.Stop();
        }

        private void OnClickPerfmonLoggingDelete(object sender, EventArgs e)
        {
            PerfmonLogging.Delete();
        }

        private void OnClickOpenLogs(object sender, EventArgs e)
        {
            string str;
            bool flag;
            lock (this.lockObject)
            {
                flag = PhotonHandler.OpenLogs(this.photonWorkingDirectory, this.logCmdDirectory, out str);
            }
            if (!flag)
            {
                this.trayIcon.ShowBalloonTip(0xbb8, Program.ResourceManager.GetString("trayIconNoLogFoundTipTitle"), Program.ResourceManager.GetString("trayIconNoLogFoundTipText") ?? string.Empty, ToolTipIcon.Info);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickExplore(object sender, EventArgs e)
        {
            PhotonHandler.ExploreWorkingDirectory(this.photonWorkingDirectory);
        }

        private void OnClickRunProcDump(object sender, EventArgs e)
        {
            try
            {
                lock (this.lockObject)
                {
                    new Process { StartInfo = { FileName = this.procDumpCommand, WorkingDirectory = this.photonWorkingDirectory } }.Start();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(Program.ResourceManager.GetString("failedOnCallMsg") + Environment.NewLine + exception.Message, Program.ResourceManager.GetString("photonControlErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void OnClickExitPhotonControl(object sender, EventArgs e)
        {
            base.Close();
            Application.Exit();
        }

        private void InitializeLocalizedFormItems()
        {
            this.trayIcon.Text = Program.ResourceManager.GetString("trayIconText");
            this.miPhotonTitle.Text = Program.ResourceManager.GetString("miPhotonTitleText");
            this.miSdkVersion.Text = Program.ResourceManager.GetString("miSdkVersionText");
            this.miLatestVersion.Text = Program.ResourceManager.GetString("miLatestVersionText");
            this.miAutostartEnable.Text = Program.ResourceManager.GetString("miAutostartEnableText");
            this.miLicense.Text = Program.ResourceManager.GetString("miLicenseText");
            this.miLicenseCompany.Text = Program.ResourceManager.GetString("miLicenseCompanyText");
            this.miLicenseeMail.Text = Program.ResourceManager.GetString("miLicenseeMailText");
            this.miMaxConnections.Text = Program.ResourceManager.GetString("miMaxConnectionsText");
            this.miExpires.Text = Program.ResourceManager.GetString("miExpiresText");
            this.miFloatingLicense.Text = Program.ResourceManager.GetString("miFloatingLicenseText");
            this.miLicensedIPs.Text = Program.ResourceManager.GetString("miLicensedIPsText");
            this.miLicenseMonitorAvailable.Text = Program.ResourceManager.GetString("miLicenseMonitorAvailableText");
            this.miHardwareID.Text = Program.ResourceManager.GetString("miHardwareIDText");
            this.copyToClipboardToolStripMenuItem.Text = Program.ResourceManager.GetString("copyToClipboardToolStripMenuItemText");
            this.miFreeLicense.Text = Program.ResourceManager.GetString("miFreeLicenseText");
            this.photonInstancesToolStripMenuItem.Text = Program.ResourceManager.GetString("photonInstancesToolStripMenuItemText");
            this.miLoadBalancingIpMenu.Text = Program.ResourceManager.GetString("miLoadBalancingIpMenuText");
            this.miCurrentGameServerIp.Text = Program.ResourceManager.GetString("miCurrentGameServerIpText");
            this.miSetLocalIp.Text = Program.ResourceManager.GetString("miSetLocalIpText");
            this.miSetPublicIp.Text = Program.ResourceManager.GetString("miSetPublicIpText");
            this.miSetAutodetectIp.Text = Program.ResourceManager.GetString("miSetAutodetectIpText");
            this.miDashboard.Text = Program.ResourceManager.GetString("miDashboardText");
            this.miDashboardOpen.Text = Program.ResourceManager.GetString("miDashboardOpenText");
            this.miDashboardStart.Text = Program.ResourceManager.GetString("miDashboardStartText");
            this.miDashboardRestart.Text = Program.ResourceManager.GetString("miDashboardRestartText");
            this.miDashboardStop.Text = Program.ResourceManager.GetString("miDashboardStopText");
            this.miDashboardInstall.Text = Program.ResourceManager.GetString("miDashboardInstallText");
            this.miDashboardRemove.Text = Program.ResourceManager.GetString("miDashboardRemoveText");
            this.miCounters.Text = Program.ResourceManager.GetString("miCountersText");
            this.miStartPerfmon.Text = Program.ResourceManager.GetString("miStartPerfmonText");
            this.miInstallCounters.Text = Program.ResourceManager.GetString("miInstallCountersText");
            this.miRemoveCounters.Text = Program.ResourceManager.GetString("miRemoveCountersText");
            this.createLoggingSetToolStripMenuItem.Text = Program.ResourceManager.GetString("createLoggingSetToolStripMenuItemText");
            this.startLoggingToolStripMenuItem.Text = Program.ResourceManager.GetString("startLoggingToolStripMenuItemText");
            this.stopLoggingToolStripMenuItem.Text = Program.ResourceManager.GetString("stopLoggingToolStripMenuItemText");
            this.removeLoggingSetToolStripMenuItem.Text = Program.ResourceManager.GetString("removeLoggingSetToolStripMenuItemText");
            this.miOpenLogs.Text = Program.ResourceManager.GetString("miOpenLogsText");
            this.miExplore.Text = Program.ResourceManager.GetString("miExploreText");
            this.miRunProcDump.Text = Program.ResourceManager.GetString("miRunProcDumpText");
            this.miExitControl.Text = Program.ResourceManager.GetString("miExitControlText");

        }
    }
}
