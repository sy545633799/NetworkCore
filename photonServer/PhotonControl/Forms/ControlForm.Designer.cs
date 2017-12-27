using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace PhotonControl.Forms
{
    partial class ControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ControlForm));
            this.trayIcon = new NotifyIcon(this.components);
            this.trayMenuStrip = new ContextMenuStrip(this.components);
            this.miPhotonTitle = new ToolStripMenuItem();
            this.miSdkVersion = new ToolStripMenuItem();
            this.miLatestVersion = new ToolStripMenuItem();
            this.toolStripSeparator11 = new ToolStripSeparator();
            this.miAutostartEnable = new ToolStripMenuItem();
            this.miLicense = new ToolStripMenuItem();
            this.miLicenseCompany = new ToolStripMenuItem();
            this.miLicenseeMail = new ToolStripMenuItem();
            this.toolStripSeparator6 = new ToolStripSeparator();
            this.miMaxConnections = new ToolStripMenuItem();
            this.miExpires = new ToolStripMenuItem();
            this.miFloatingLicense = new ToolStripMenuItem();
            this.miLicensedIPs = new ToolStripMenuItem();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.miLicenseMonitorAvailable = new ToolStripMenuItem();
            this.miHardwareID = new ToolStripMenuItem();
            this.copyToClipboardToolStripMenuItem = new ToolStripMenuItem();
            this.miFreeLicense = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.photonInstancesToolStripMenuItem = new ToolStripMenuItem();
            this.miLoadBalancingIpMenu = new ToolStripMenuItem();
            this.miCurrentGameServerIp = new ToolStripMenuItem();
            this.miSetLocalIp = new ToolStripMenuItem();
            this.miSetPublicIp = new ToolStripMenuItem();
            this.miSetAutodetectIp = new ToolStripMenuItem();
            this.toolStripSeparator5 = new ToolStripSeparator();
            this.miDashboard = new ToolStripMenuItem();
            this.miDashboardOpen = new ToolStripMenuItem();
            this.toolStripSeparator8 = new ToolStripSeparator();
            this.miDashboardStart = new ToolStripMenuItem();
            this.miDashboardRestart = new ToolStripMenuItem();
            this.miDashboardStop = new ToolStripMenuItem();
            this.toolStripSeparator9 = new ToolStripSeparator();
            this.miDashboardInstall = new ToolStripMenuItem();
            this.miDashboardRemove = new ToolStripMenuItem();
            this.miCounters = new ToolStripMenuItem();
            this.miStartPerfmon = new ToolStripMenuItem();
            this.toolStripSeparator10 = new ToolStripSeparator();
            this.miInstallCounters = new ToolStripMenuItem();
            this.miRemoveCounters = new ToolStripMenuItem();
            this.toolStripSeparator13 = new ToolStripSeparator();
            this.createLoggingSetToolStripMenuItem = new ToolStripMenuItem();
            this.startLoggingToolStripMenuItem = new ToolStripMenuItem();
            this.stopLoggingToolStripMenuItem = new ToolStripMenuItem();
            this.removeLoggingSetToolStripMenuItem = new ToolStripMenuItem();
            this.miOpenLogs = new ToolStripMenuItem();
            this.miExplore = new ToolStripMenuItem();
            this.miRunProcDump = new ToolStripMenuItem();
            this.toolStripSeparator4 = new ToolStripSeparator();
            this.miExitControl = new ToolStripMenuItem();
            this.trayMenuStrip.SuspendLayout();
            base.SuspendLayout();
            this.trayIcon.ContextMenuStrip = this.trayMenuStrip;
            this.trayIcon.Icon = (Icon)resources.GetObject("trayIcon.Icon");
            this.trayIcon.Text = "Photon stopped";
            this.trayIcon.MouseClick += new MouseEventHandler(this.OnClickTrayIcon);
            this.trayMenuStrip.Items.AddRange(new ToolStripItem[] { this.miPhotonTitle, this.miLicense, this.miFreeLicense, this.toolStripSeparator1, this.photonInstancesToolStripMenuItem, this.miLoadBalancingIpMenu, this.toolStripSeparator5, this.miDashboard, this.miCounters, this.miOpenLogs, this.miExplore, this.miRunProcDump, this.toolStripSeparator4, this.miExitControl });
            this.trayMenuStrip.Name = "contextMenuStrip1";
            this.trayMenuStrip.Size = new Size(0xc1, 0x108);
            this.trayMenuStrip.VisibleChanged += new EventHandler(this.OnMenuVisibilityChange);
            this.miPhotonTitle.DropDownItems.AddRange(new ToolStripItem[] { this.miSdkVersion, this.miLatestVersion, this.toolStripSeparator11, this.miAutostartEnable });
            this.miPhotonTitle.Name = "miPhotonTitle";
            this.miPhotonTitle.Size = new Size(0xc0, 0x16);
            this.miPhotonTitle.Text = "Photon Control";
            this.miSdkVersion.Name = "miSdkVersion";
            this.miSdkVersion.Size = new Size(0xd0, 0x16);
            this.miSdkVersion.Text = "Version";
            this.miLatestVersion.Name = "miLatestVersion";
            this.miLatestVersion.Size = new Size(0xd0, 0x16);
            this.miLatestVersion.Text = "Current version";
            this.miLatestVersion.ToolTipText = "Go to download page.";
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new Size(0xcd, 6);
            this.miAutostartEnable.Name = "miAutostartEnable";
            this.miAutostartEnable.Size = new Size(0xd0, 0x16);
            this.miAutostartEnable.Text = "Autostart Photon Control";
            this.miAutostartEnable.ToolTipText = "Click to toggle autostart of Photon Control";
            this.miAutostartEnable.Click += new EventHandler(this.OnClickAutostartToggle);
            this.miLicense.DropDownItems.AddRange(new ToolStripItem[] { this.miLicenseCompany, this.miLicenseeMail, this.toolStripSeparator6, this.miMaxConnections, this.miExpires, this.miFloatingLicense, this.miLicensedIPs, this.toolStripSeparator3, this.miLicenseMonitorAvailable, this.miHardwareID });
            this.miLicense.Name = "miLicense";
            this.miLicense.Size = new Size(0xc0, 0x16);
            this.miLicense.Text = "License info";
            this.miLicense.ToolTipText = "Only updated on start of Photon Control";
            this.miLicenseCompany.Enabled = false;
            this.miLicenseCompany.Name = "miLicenseCompany";
            this.miLicenseCompany.Size = new Size(0x110, 0x16);
            this.miLicenseCompany.Text = "Licensed for";
            this.miLicenseeMail.Enabled = false;
            this.miLicenseeMail.Name = "miLicenseeMail";
            this.miLicenseeMail.Size = new Size(0x110, 0x16);
            this.miLicenseeMail.Text = "Licensee mail";
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new Size(0x10d, 6);
            this.miMaxConnections.Enabled = false;
            this.miMaxConnections.Name = "miMaxConnections";
            this.miMaxConnections.Size = new Size(0x110, 0x16);
            this.miMaxConnections.Text = "Max Connections";
            this.miExpires.Enabled = false;
            this.miExpires.Name = "miExpires";
            this.miExpires.Size = new Size(0x110, 0x16);
            this.miExpires.Text = "Expiry Date";
            this.miFloatingLicense.Enabled = false;
            this.miFloatingLicense.Name = "miFloatingLicense";
            this.miFloatingLicense.Size = new Size(0x110, 0x16);
            this.miFloatingLicense.Text = "Floating license";
            this.miLicensedIPs.Enabled = false;
            this.miLicensedIPs.Name = "miLicensedIPs";
            this.miLicensedIPs.Size = new Size(0x110, 0x16);
            this.miLicensedIPs.Text = "Licensed IPs";
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(0x10d, 6);
            this.miLicenseMonitorAvailable.BackColor = SystemColors.Control;
            this.miLicenseMonitorAvailable.Enabled = false;
            this.miLicenseMonitorAvailable.Font = new Font("Segoe UI", 9f);
            this.miLicenseMonitorAvailable.Name = "miLicenseMonitorAvailable";
            this.miLicenseMonitorAvailable.Size = new Size(0x110, 0x16);
            this.miLicenseMonitorAvailable.Text = "License Monitor: checking availability";
            this.miHardwareID.DropDownItems.AddRange(new ToolStripItem[] { this.copyToClipboardToolStripMenuItem });
            this.miHardwareID.Name = "miHardwareID";
            this.miHardwareID.Size = new Size(0x110, 0x16);
            this.miHardwareID.Text = "Hardware ID";
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new Size(0xa9, 0x16);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copyToClipboardToolStripMenuItem.Click += new EventHandler(this.OnClickHardwareIdToClipboard);
            this.miFreeLicense.Name = "miFreeLicense";
            this.miFreeLicense.Size = new Size(0xc0, 0x16);
            this.miFreeLicense.Text = "Get Your Free License";
            this.miFreeLicense.ToolTipText = "Photon is free to use for up to 100 concurrent players.";
            this.miFreeLicense.Visible = false;
            this.miFreeLicense.Click += new EventHandler(this.OnClickGetFreeLicense);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(0xbd, 6);
            this.photonInstancesToolStripMenuItem.BackColor = SystemColors.ButtonFace;
            this.photonInstancesToolStripMenuItem.Enabled = false;
            this.photonInstancesToolStripMenuItem.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.photonInstancesToolStripMenuItem.Name = "photonInstancesToolStripMenuItem";
            this.photonInstancesToolStripMenuItem.Size = new Size(0xc0, 0x16);
            this.photonInstancesToolStripMenuItem.Text = "Photon instances:";
            this.miLoadBalancingIpMenu.DropDownItems.AddRange(new ToolStripItem[] { this.miCurrentGameServerIp, this.miSetLocalIp, this.miSetPublicIp, this.miSetAutodetectIp });
            this.miLoadBalancingIpMenu.Name = "miLoadBalancingIpMenu";
            this.miLoadBalancingIpMenu.Size = new Size(0xc0, 0x16);
            this.miLoadBalancingIpMenu.Text = "Game Server IP Config";
            this.miLoadBalancingIpMenu.ToolTipText = "Configures the Game Server IP for clients.";
            this.miCurrentGameServerIp.BackColor = SystemColors.ButtonFace;
            this.miCurrentGameServerIp.Enabled = false;
            this.miCurrentGameServerIp.Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.miCurrentGameServerIp.Name = "miCurrentGameServerIp";
            this.miCurrentGameServerIp.Size = new Size(0xc9, 0x16);
            this.miCurrentGameServerIp.Text = "Current IP";
            this.miSetLocalIp.Name = "miSetLocalIp";
            this.miSetLocalIp.Size = new Size(0xc9, 0x16);
            this.miSetLocalIp.Text = "Set Local IP";
            this.miSetLocalIp.ToolTipText = "Local network IP. Not available through the internet. Won't adjust on deployment.";
            this.miSetLocalIp.Click += new EventHandler(this.OnClickGameServerToPrivateIp);
            this.miSetPublicIp.Name = "miSetPublicIp";
            this.miSetPublicIp.Size = new Size(0xc9, 0x16);
            this.miSetPublicIp.Text = "Set Public IP";
            this.miSetPublicIp.ToolTipText = "Applies current public IP. Careful: This won't adjust if IP changes or server is deployed elsewhere.";
            this.miSetPublicIp.Click += new EventHandler(this.OnClickGameServerToPublicIp);
            this.miSetAutodetectIp.Name = "miSetAutodetectIp";
            this.miSetAutodetectIp.Size = new Size(0xc9, 0x16);
            this.miSetAutodetectIp.Text = "Set Autodetect Public IP";
            this.miSetAutodetectIp.ToolTipText = "With blank IP setup, gameservers detect their public IP on start. Great for deployment.";
            this.miSetAutodetectIp.Click += new EventHandler(this.OnClickGameServerToAutodetectPublicIP);
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new Size(0xbd, 6);
            this.miDashboard.DropDownItems.AddRange(new ToolStripItem[] { this.miDashboardOpen, this.toolStripSeparator8, this.miDashboardStart, this.miDashboardRestart, this.miDashboardStop, this.toolStripSeparator9, this.miDashboardInstall, this.miDashboardRemove });
            this.miDashboard.Name = "miDashboard";
            this.miDashboard.Size = new Size(0xc0, 0x16);
            this.miDashboard.Text = "Dashboard";
            this.miDashboardOpen.Name = "miDashboardOpen";
            this.miDashboardOpen.Size = new Size(0x9c, 0x16);
            this.miDashboardOpen.Text = "Open";
            this.miDashboardOpen.Click += new EventHandler(this.OnClickDashboardOpen);
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new Size(0x99, 6);
            this.miDashboardStart.Name = "miDashboardStart";
            this.miDashboardStart.Size = new Size(0x9c, 0x16);
            this.miDashboardStart.Text = "Start";
            this.miDashboardStart.Click += new EventHandler(this.OnClickDashboardStart);
            this.miDashboardRestart.Name = "miDashboardRestart";
            this.miDashboardRestart.Size = new Size(0x9c, 0x16);
            this.miDashboardRestart.Text = "Restart";
            this.miDashboardRestart.Click += new EventHandler(this.OnClickDashboardRestart);
            this.miDashboardStop.Name = "miDashboardStop";
            this.miDashboardStop.Size = new Size(0x9c, 0x16);
            this.miDashboardStop.Text = "Stop";
            this.miDashboardStop.Click += new EventHandler(this.OnClickDashboardStop);
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new Size(0x99, 6);
            this.miDashboardInstall.Name = "miDashboardInstall";
            this.miDashboardInstall.Size = new Size(0x9c, 0x16);
            this.miDashboardInstall.Text = "Install service";
            this.miDashboardInstall.Click += new EventHandler(this.OnClickDashboardInstall);
            this.miDashboardRemove.Name = "miDashboardRemove";
            this.miDashboardRemove.Size = new Size(0x9c, 0x16);
            this.miDashboardRemove.Text = "Remove service";
            this.miDashboardRemove.Click += new EventHandler(this.OnClickDashboardRemove);
            this.miCounters.DropDownItems.AddRange(new ToolStripItem[] { this.miStartPerfmon, this.toolStripSeparator10, this.miInstallCounters, this.miRemoveCounters, this.toolStripSeparator13, this.createLoggingSetToolStripMenuItem, this.startLoggingToolStripMenuItem, this.stopLoggingToolStripMenuItem, this.removeLoggingSetToolStripMenuItem });
            this.miCounters.Name = "miCounters";
            this.miCounters.Size = new Size(0xc0, 0x16);
            this.miCounters.Text = "PerfMon Counters";
            this.miStartPerfmon.Name = "miStartPerfmon";
            this.miStartPerfmon.Size = new Size(0xb7, 0x16);
            this.miStartPerfmon.Text = "Start PerfMon";
            this.miStartPerfmon.Click += new EventHandler(this.OnClickPerfmonStart);
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new Size(180, 6);
            this.miInstallCounters.Name = "miInstallCounters";
            this.miInstallCounters.Size = new Size(0xb7, 0x16);
            this.miInstallCounters.Text = "Install Counters";
            this.miInstallCounters.Click += new EventHandler(this.OnClickCountersInstall);
            this.miRemoveCounters.Name = "miRemoveCounters";
            this.miRemoveCounters.Size = new Size(0xb7, 0x16);
            this.miRemoveCounters.Text = "Remove Counters";
            this.miRemoveCounters.Click += new EventHandler(this.OnClickCountersRemove);
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new Size(180, 6);
            this.createLoggingSetToolStripMenuItem.Name = "createLoggingSetToolStripMenuItem";
            this.createLoggingSetToolStripMenuItem.Size = new Size(0xb7, 0x16);
            this.createLoggingSetToolStripMenuItem.Text = "Create Logging Set";
            this.createLoggingSetToolStripMenuItem.Click += new EventHandler(this.OnClickPerfmonLoggingCreate);
            this.startLoggingToolStripMenuItem.Name = "startLoggingToolStripMenuItem";
            this.startLoggingToolStripMenuItem.Size = new Size(0xb7, 0x16);
            this.startLoggingToolStripMenuItem.Text = "Start Logging";
            this.startLoggingToolStripMenuItem.Click += new EventHandler(this.OnClickPerfmonLoggingStart);
            this.stopLoggingToolStripMenuItem.Name = "stopLoggingToolStripMenuItem";
            this.stopLoggingToolStripMenuItem.Size = new Size(0xb7, 0x16);
            this.stopLoggingToolStripMenuItem.Text = "Stop Logging";
            this.stopLoggingToolStripMenuItem.Click += new EventHandler(this.OnClickPerfmonLoggingStop);
            this.removeLoggingSetToolStripMenuItem.Name = "removeLoggingSetToolStripMenuItem";
            this.removeLoggingSetToolStripMenuItem.Size = new Size(0xb7, 0x16);
            this.removeLoggingSetToolStripMenuItem.Text = "Remove Logging Set";
            this.removeLoggingSetToolStripMenuItem.Click += new EventHandler(this.OnClickPerfmonLoggingDelete);
            this.miOpenLogs.Name = "miOpenLogs";
            this.miOpenLogs.Size = new Size(0xc0, 0x16);
            this.miOpenLogs.Text = "Open Logs";
            this.miOpenLogs.ToolTipText = "Requires ../bin_tools/baretail/baretail-open-photonlogs.cmd";
            this.miOpenLogs.Click += new EventHandler(this.OnClickOpenLogs);
            this.miExplore.Name = "miExplore";
            this.miExplore.Size = new Size(0xc0, 0x16);
            this.miExplore.Text = "Explore Working Path";
            this.miExplore.ToolTipText = "Opens Windows Explorer";
            this.miExplore.Click += new EventHandler(this.OnClickExplore);
            this.miRunProcDump.Name = "miRunProcDump";
            this.miRunProcDump.Size = new Size(0xc0, 0x16);
            this.miRunProcDump.Text = "Run ProcDump";
            this.miRunProcDump.Click += new EventHandler(this.OnClickRunProcDump);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new Size(0xbd, 6);
            this.miExitControl.Name = "miExitControl";
            this.miExitControl.Size = new Size(0xc0, 0x16);
            this.miExitControl.Text = "Exit Photon Control";
            this.miExitControl.ToolTipText = "Does not stop Photon. Removes the TrayIcon.";
            this.miExitControl.Click += new EventHandler(this.OnClickExitPhotonControl);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new Size(0x124, 0x110);
            base.Name = "ControlForm";
            this.Text = "Form1";
            this.trayMenuStrip.ResumeLayout(false);
            base.ResumeLayout(false);
            // see http://blog.stephencleary.com/2009/11/reverse-compiling-windows-forms.html
            // or http://www.fast818.com/n391.ashx
         //   this.SuspendLayout();
            // 
            // ControlForm
            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
    //        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      //      this.ClientSize = new System.Drawing.Size(284, 262);
         //   this.Name = "ControlForm";
           // this.Text = "Form1";
          //  this.ResumeLayout(false);

        }


        #endregion

        private readonly Icon iconRunning;
        private readonly Icon iconStopped;

        private ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private ToolStripMenuItem createLoggingSetToolStripMenuItem;
        private ToolStripMenuItem miAutostartEnable;
        private ToolStripMenuItem miCounters;
        private ToolStripMenuItem miCurrentGameServerIp;
        private ToolStripMenuItem miDashboard;
        private ToolStripMenuItem miDashboardInstall;
        private ToolStripMenuItem miDashboardOpen;
        private ToolStripMenuItem miDashboardRemove;
        private ToolStripMenuItem miDashboardRestart;
        private ToolStripMenuItem miDashboardStart;
        private ToolStripMenuItem miDashboardStop;
        private ToolStripMenuItem miExitControl;
        private ToolStripMenuItem miExpires;
        private ToolStripMenuItem miExplore;
        private ToolStripMenuItem miFloatingLicense;
        private ToolStripMenuItem miFreeLicense;
        private ToolStripMenuItem miHardwareID;
        private ToolStripMenuItem miInstallCounters;
        private ToolStripMenuItem miLatestVersion;
        private ToolStripMenuItem miLicense;
        private ToolStripMenuItem miLicenseCompany;
        private ToolStripMenuItem miLicensedIPs;
        private ToolStripMenuItem miLicenseeMail;
        private ToolStripMenuItem miLicenseMonitorAvailable;
        private ToolStripMenuItem miLoadBalancingIpMenu;
        private ToolStripMenuItem miMaxConnections;
        private ToolStripMenuItem miOpenLogs;
        private ToolStripMenuItem miPhotonTitle;
        private ToolStripMenuItem miRemoveCounters;
        private ToolStripMenuItem miRunProcDump;
        private ToolStripMenuItem miSdkVersion;
        private ToolStripMenuItem miSetAutodetectIp;
        private ToolStripMenuItem miSetLocalIp;
        private ToolStripMenuItem miSetPublicIp;
        private ToolStripMenuItem miStartPerfmon;
        private ToolStripMenuItem startLoggingToolStripMenuItem;
        private ToolStripMenuItem stopLoggingToolStripMenuItem;
        private ToolStripMenuItem removeLoggingSetToolStripMenuItem;
        private ToolStripMenuItem photonInstancesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenuStrip;

    }
}