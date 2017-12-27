using System;
using System.Drawing;
using System.Windows.Forms;

namespace PhotonControl.Forms
{
    partial class LauncherStartupDialogForm
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
            this.selectIps = new ComboBox();
            this.addressButton = new Button();
            this.buttonOk = new Button();
            this.buttonCancel = new Button();
            base.SuspendLayout();
            this.selectIps.Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.selectIps.FormattingEnabled = true;
            this.selectIps.Location = new Point(12, 0x38);
            this.selectIps.Name = "selectIps";
            this.selectIps.Size = new Size(0xf7, 0x18);
            this.selectIps.TabIndex = 0;
            this.selectIps.SelectionChangeCommitted += new EventHandler(this.OnSelectIpsSelectionChangeCommitted);
            this.addressButton.FlatAppearance.BorderSize = 0;
            this.addressButton.FlatStyle = FlatStyle.Flat;
            this.addressButton.Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.addressButton.Location = new Point(12, 12);
            this.addressButton.Name = "addressButton";
            this.addressButton.Size = new Size(0xc2, 0x26);
            this.addressButton.TabIndex = 2;
            this.addressButton.Text = "Select Photon IP:";
            this.addressButton.TextAlign = ContentAlignment.MiddleLeft;
            this.addressButton.UseVisualStyleBackColor = false;
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.FlatStyle = FlatStyle.Flat;
            this.buttonOk.Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.buttonOk.Location = new Point(0xa3, 0x65);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new Size(0x60, 0x26);
            this.buttonOk.TabIndex = 6;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.Flat;
            this.buttonCancel.Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.buttonCancel.Location = new Point(12, 0x65);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(0x60, 0x26);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = SystemColors.Window;
            base.ClientSize = new Size(0x10f, 0x97);
            base.ControlBox = false;
            base.Controls.Add(this.buttonCancel);
            base.Controls.Add(this.buttonOk);
            base.Controls.Add(this.addressButton);
            base.Controls.Add(this.selectIps);
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "LauncherStartupDialogForm";
            this.Text = "Photon Launcher";
            base.ResumeLayout(false);

            //this.components = new System.ComponentModel.Container();
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.Text = "LauncherStartupDialogForm";
        }

        #endregion

        private Button addressButton;
        private Button buttonCancel;
        private Button buttonOk;
        private ComboBox selectIps;
    }
}