using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PhotonControl.Helper;

namespace PhotonControl.Forms
{
    public partial class LauncherForm : Form
    {
        private const string BackgroundImageName = "LauncherBackground.png";
        private const string GameServerConfigPaths = @"..\LoadBalancing\GameServer1\bin\Photon.LoadBalancing.dll.config;..\LoadBalancing\GameServer2\bin\Photon.LoadBalancing.dll.config";
        private string gsConfiguredIp;

        private const string InstanceName = "Simple";
        private readonly Timer intervalTimer;

        private string photonWorkingDirectory;

        private static readonly Version requiredFrameworkVersion;



        public LauncherForm()
        {
            this.InitializeComponent();
            this.InitializeLocalizedFormItems();
            this.InitializeVariables();
            this.label1.Text = Program.ResourceManager.GetString("launcherPhotonStartingText");
            this.trayIcon.Icon = this.iconStopped;
            try
            {
                this.panel1.BackgroundImage = Image.FromFile(Path.Combine(this.photonWorkingDirectory, "LauncherBackground.png"));
            }
            catch (FileNotFoundException)
            {
            }
            catch (OutOfMemoryException)
            {
            }
            PhotonHandler.AddPhotonToFirewall(this.photonWorkingDirectory);
            Timer timer = new Timer
            {
                Interval = 0x3e8
            };
            this.intervalTimer = timer;
            this.intervalTimer.Tick += new EventHandler(this.TimerTick);
            this.intervalTimer.Start();

        }

        private void InitializeVariables()
        {
            string str;
            this.iconRunning = (Icon)PhotonControl.Properties.Resources.ResourceManager.GetObject("Running");
            this.iconStopped = (Icon)PhotonControl.Properties.Resources.ResourceManager.GetObject("Stopped");
            if (string.IsNullOrEmpty(PhotonControlSettings.Default.PhotonWorkingDirectory))
            {
                this.photonWorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + @"\";
            }
            else
            {
                this.photonWorkingDirectory = PhotonControlSettings.Default.PhotonWorkingDirectory;
            }
            this.gsConfiguredIp = GameServerConfigChanger.GetCurrentConfigIp(PhotonControlSettings.Default.GameServerConfigPaths, out str);
        }

        private void InitializeLocalizedFormItems()
        {
        }

        private bool IsPhotonRunningAsApp()
        {
            ProcessInfo info = PhotonHandler.GetActiveProcesses().Find(p => (p.InstanceName == "Simple") && (p.Command == ProcessInfo.PhotonCommand.RunAsApplication));
            return ((info != null) && !info.Process.HasExited);
        }

        private void OnAddressButtonClick(object sender, EventArgs e)
        {
            string text = string.Format("{0}:{1}", this.gsConfiguredIp, 0x13bf);
            Clipboard.SetText(text);
            string caption = string.Format("{0}: {1}", Program.ResourceManager.GetString("launcherUdpAddressCopiedText"), text);
            this.toolTipCopy.SetToolTip(this.label2, caption);
        }

        private void OnAddressButtonMouseHover(object sender, EventArgs e)
        {
            if (this.IsPhotonRunningAsApp())
            {
                string caption = Program.ResourceManager.GetString("launcherCopyAddressText");
                this.toolTipCopy.SetToolTip(this.label2, caption);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.intervalTimer != null)
            {
                this.intervalTimer.Stop();
                this.intervalTimer.Dispose();
            }
            base.OnClosing(e);
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) && this.IsPhotonRunningAsApp())
            {
                string str;
                if (MessageBox.Show(Program.ResourceManager.GetString("launcherPhotonStopWarningMsg"), Program.ResourceManager.GetString("launcherPhotonStoppingText"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
                PhotonHandler.StopPhotonApplication("Simple", this.photonWorkingDirectory, out str);
                if (!string.IsNullOrEmpty(str))
                {
                    MessageBox.Show(str);
                }
            }
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            if (base.WindowState == FormWindowState.Minimized)
            {
                base.ShowInTaskbar = false;
                this.trayIcon.Visible = true;
            }
            else
            {
                base.ShowInTaskbar = true;
                this.trayIcon.Visible = false;
            }
            base.OnResize(e);
        }

        protected override void OnShown(EventArgs e)
        {
            string str;
            string str2;
            if (!FrameworkVersionChecker.CheckRequiredFrameworkVersion(requiredFrameworkVersion, out str))
            {
                MessageBox.Show(str);
            }
            LauncherStartupDialogForm form = new LauncherStartupDialogForm();
            if (form.localIps.Length > 1)
            {
                form.StartPosition = FormStartPosition.CenterParent;
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    base.Close();
                    return;
                }
            }
            this.gsConfiguredIp = form.SelectedIp;
            GameServerConfigChanger.EditConfigFiles(@"..\LoadBalancing\GameServer1\bin\Photon.LoadBalancing.dll.config;..\LoadBalancing\GameServer2\bin\Photon.LoadBalancing.dll.config", this.gsConfiguredIp, out str2);
            if (!string.IsNullOrEmpty(str2))
            {
                MessageBox.Show(str2);
                this.label1.Text = Program.ResourceManager.GetString("launcherFailedToSetIpText");
                this.startStopButton.Enabled = false;
                this.startStopButton.Visible = false;
                this.startStopButton.Text = null;
            }
            else
            {
                PhotonHandler.StartPhotonApplication("Simple", this.photonWorkingDirectory, out str2);
                if (!string.IsNullOrEmpty(str2))
                {
                    MessageBox.Show(str2);
                    this.label1.Text = Program.ResourceManager.GetString("launcherFailedToStartText");
                    this.startStopButton.Text = Program.ResourceManager.GetString("launcherStartButtonText");
                }
            }
        }

        private void OnStartStopButtonClick(object sender, EventArgs e)
        {
            if (this.IsPhotonRunningAsApp())
            {
                this.StopPhoton();
            }
            else
            {
                this.StartPhoton();
            }
        }

        private void OnTrayIconClick(object sender, EventArgs e)
        {
            base.ShowInTaskbar = true;
            base.WindowState = FormWindowState.Normal;
        }

        private void OnTrayIconClick(object sender, MouseEventArgs e)
        {
            base.ShowInTaskbar = true;
            base.WindowState = FormWindowState.Normal;
        }

        private void SetEnabledMenuItems()
        {
            if (this.IsPhotonRunningAsApp())
            {
                this.label1.Text = string.Format("{0}", Program.ResourceManager.GetString("launcherPhotonStartedText"));
                this.label2.Text = string.Format("{0}:{1}", this.gsConfiguredIp, 0x13bf);
                this.startStopButton.Text = Program.ResourceManager.GetString("launcherStopButtonText");
                this.trayIcon.Icon = this.iconRunning;
            }
            else
            {
                this.label1.Text = Program.ResourceManager.GetString("launcherPhotonStoppedText");
                this.startStopButton.Text = Program.ResourceManager.GetString("launcherStartButtonText");
                this.trayIcon.Icon = this.iconStopped;
            }
        }

        private void StartPhoton()
        {
            string str;
            if (this.IsPhotonRunningAsApp())
            {
                str = Program.ResourceManager.GetString("launcherPhotonAlreadyRunningText");
            }
            else
            {
                this.startStopButton.Text = Program.ResourceManager.GetString("launcherWaitButtonText");
                PhotonHandler.StartPhotonApplication("Simple", this.photonWorkingDirectory, out str);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str);
                this.SetEnabledMenuItems();
            }
        }

        private void StopPhoton()
        {
            string str;
            if (!this.IsPhotonRunningAsApp())
            {
                str = Program.ResourceManager.GetString("launcherPhotonNotRunningText");
            }
            else
            {
                this.startStopButton.Text = Program.ResourceManager.GetString("launcherWaitButtonText");
                PhotonHandler.StopPhotonApplication("Simple", this.photonWorkingDirectory, out str);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MessageBox.Show(str);
                this.SetEnabledMenuItems();
            }
        }

        protected void TimerTick(object sender, EventArgs e)
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
    }
}
