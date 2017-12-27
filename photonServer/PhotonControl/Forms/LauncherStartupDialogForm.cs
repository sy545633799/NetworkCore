using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PhotonControl.Helper;

namespace PhotonControl.Forms
{
    public partial class LauncherStartupDialogForm : Form
    {
        private BindingList<KeyValuePair<string, string>> items = new BindingList<KeyValuePair<string, string>>();
        protected internal string[] localIps;
        public string SelectedIp { get; private set; }

        public LauncherStartupDialogForm()
        {
            InitializeComponent();
            FillMenu();
        }

        private void FillMenu()
        {
            GameServerConfigChanger.GetLocalIPs(out this.localIps);
            this.SelectedIp = this.localIps[0];
            foreach (string str in this.localIps)
            {
                this.items.Add(new KeyValuePair<string, string>("Private: " + str, str));
            }
            this.items.Add(new KeyValuePair<string, string>("Public: still retrieving...", string.Empty));
            GameServerConfigChanger.GetPublicIpCompleted += new GameServerConfigChanger.GetPublicIpCompletedHandler(this.GetPublicIpCompleted);
            GameServerConfigChanger.GetPublicIPAsync();
            this.selectIps.DataSource = this.items;
            this.selectIps.DisplayMember = "Key";
            this.selectIps.ValueMember = "Value";
            this.selectIps.SelectedIndex = 0;
        }

        private void GetPublicIpCompleted(string publicIp)
        {
            this.items.RemoveAt(this.items.Count - 1);
            this.items.Add(new KeyValuePair<string, string>("Public: " + publicIp, publicIp));
        }

        private void OnSelectIpsSelectionChangeCommitted(object sender, EventArgs e)
        {
            this.SelectedIp = this.selectIps.SelectedValue.ToString();
        }
    }
}
