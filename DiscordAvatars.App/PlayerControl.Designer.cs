namespace DiscordAvatars.App;

partial class PlayerControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        chkPlayerEnabled = new CheckBox();
        cmbPlayer = new ComboBox();
        imgAvatar = new PictureBox();
        lblUsername = new Label();
        ((System.ComponentModel.ISupportInitialize)imgAvatar).BeginInit();
        SuspendLayout();
        // 
        // chkPlayerEnabled
        // 
        chkPlayerEnabled.AutoSize = true;
        chkPlayerEnabled.Location = new Point(7, 10);
        chkPlayerEnabled.Name = "chkPlayerEnabled";
        chkPlayerEnabled.Size = new Size(39, 19);
        chkPlayerEnabled.TabIndex = 0;
        chkPlayerEnabled.Text = "P1";
        chkPlayerEnabled.UseVisualStyleBackColor = true;
        // 
        // cmbPlayer
        // 
        cmbPlayer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        cmbPlayer.FormattingEnabled = true;
        cmbPlayer.Location = new Point(52, 8);
        cmbPlayer.Name = "cmbPlayer";
        cmbPlayer.Size = new Size(149, 23);
        cmbPlayer.TabIndex = 1;
        // 
        // imgAvatar
        // 
        imgAvatar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        imgAvatar.Location = new Point(207, 3);
        imgAvatar.Name = "imgAvatar";
        imgAvatar.Size = new Size(32, 32);
        imgAvatar.TabIndex = 2;
        imgAvatar.TabStop = false;
        // 
        // lblUsername
        // 
        lblUsername.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblUsername.AutoSize = true;
        lblUsername.Location = new Point(245, 11);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(48, 15);
        lblUsername.TabIndex = 3;
        lblUsername.Text = "Player 1";
        // 
        // PlayerControl
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(lblUsername);
        Controls.Add(imgAvatar);
        Controls.Add(cmbPlayer);
        Controls.Add(chkPlayerEnabled);
        Name = "PlayerControl";
        Size = new Size(373, 38);
        ((System.ComponentModel.ISupportInitialize)imgAvatar).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private CheckBox chkPlayerEnabled;
    private ComboBox cmbPlayer;
    private PictureBox imgAvatar;
    private Label lblUsername;
}
