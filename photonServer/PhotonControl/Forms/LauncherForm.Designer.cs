using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace PhotonControl.Forms
{
    partial class LauncherForm
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(LauncherForm));
            this.trayIcon = new NotifyIcon(this.components);
            this.toolTipCopy = new ToolTip(this.components);
            this.startStopButton = new Button();
            this.panel1 = new Panel();
            this.panel3 = new Panel();
            this.pictureBox1 = new PictureBox();
            this.label1 = new Label();
            this.label2 = new Label();
            this.panel3.SuspendLayout();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            base.SuspendLayout();
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new EventHandler(this.OnTrayIconClick);
            this.trayIcon.MouseClick += new MouseEventHandler(this.OnTrayIconClick);
            this.trayIcon.MouseDoubleClick += new MouseEventHandler(this.OnTrayIconClick);
            this.startStopButton.FlatStyle = FlatStyle.Flat;
            this.startStopButton.Font = new Font("Verdana", 14.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.startStopButton.Location = new Point(0x28c, 0x10);
            this.startStopButton.Name = "startStopButton";
            this.startStopButton.Size = new Size(0x4c, 0x26);
            this.startStopButton.TabIndex = 5;
            this.startStopButton.Text = "Start";
            this.startStopButton.UseVisualStyleBackColor = false;
            this.startStopButton.Click += new EventHandler(this.OnStartStopButtonClick);
            this.panel1.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel1.BackgroundImage");
            this.panel1.BackgroundImageLayout = ImageLayout.None;
            this.panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(740, 400);
            this.panel1.TabIndex = 4;
            this.panel3.BackColor = Color.Black;
            this.panel3.Controls.Add(this.pictureBox1);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.startStopButton);
            this.panel3.Dock = DockStyle.Bottom;
            this.panel3.ForeColor = Color.White;
            this.panel3.Location = new Point(0, 400);
            this.panel3.Margin = new Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new Size(740, 0x40);
            this.panel3.TabIndex = 6;
            this.panel3.Click += new EventHandler(this.OnAddressButtonClick);
            this.pictureBox1.BackColor = Color.Transparent;
            this.pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            this.pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new Point(9, 11);
            this.pictureBox1.Margin = new Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(0x8a, 0x2c);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.label1.BackColor = Color.Transparent;
            this.label1.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.label1.Location = new Point(0x91, 1);
            this.label1.Name = "label1";
            this.label1.Size = new Size(450, 0x19);
            this.label1.TabIndex = 6;
            this.label1.Text = "Message";
            this.label1.TextAlign = ContentAlignment.MiddleCenter;
            this.label2.BackColor = Color.Transparent;
            this.label2.Font = new Font("Microsoft Sans Serif", 20.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.label2.Location = new Point(0x91, 0x12);
            this.label2.Name = "label2";
            this.label2.Size = new Size(450, 0x27);
            this.label2.TabIndex = 7;
            this.label2.TextAlign = ContentAlignment.MiddleCenter;
            this.label2.Click += new EventHandler(this.OnAddressButtonClick);
            this.label2.MouseEnter += new EventHandler(this.OnAddressButtonMouseHover);
            this.label2.MouseLeave += new EventHandler(this.OnAddressButtonMouseHover);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = SystemColors.Window;
            this.BackgroundImageLayout = ImageLayout.None;
            base.ClientSize = new Size(740, 0x1d0);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.panel3);
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            base.Icon = (Icon)resources.GetObject("$this.Icon");
            base.MaximizeBox = false;
            base.Name = "LauncherForm";
            base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Photon Launcher";
            base.FormClosed += new FormClosedEventHandler(this.OnFormClosed);
            this.panel3.ResumeLayout(false);
            ((ISupportInitialize)this.pictureBox1).EndInit();
            base.ResumeLayout(false);


            //this.SuspendLayout();
            //// 
            //// LauncherForm
            //// 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.ClientSize = new System.Drawing.Size(284, 262);
            //this.Name = "LauncherForm";
            //this.Text = "LauncherForm";
            //this.ResumeLayout(false);

        }

        #endregion
        private Icon iconRunning;
        private Icon iconStopped;
        private Label label1;
        private Label label2;
        private Panel panel1;
        private Panel panel3;
        private PictureBox pictureBox1;
        private Button startStopButton;
        private ToolTip toolTipCopy;
        private NotifyIcon trayIcon;
    }
}