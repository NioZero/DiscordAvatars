namespace DiscordAvatars.App;

partial class FormPrincipal
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
        groupPlayerList = new GroupBox();
        playerControl4 = new PlayerControl();
        playerControl3 = new PlayerControl();
        playerControl2 = new PlayerControl();
        playerControl1 = new PlayerControl();
        groupPlayerList.SuspendLayout();
        SuspendLayout();
        // 
        // groupPlayerList
        // 
        groupPlayerList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupPlayerList.Controls.Add(playerControl4);
        groupPlayerList.Controls.Add(playerControl3);
        groupPlayerList.Controls.Add(playerControl2);
        groupPlayerList.Controls.Add(playerControl1);
        groupPlayerList.Location = new Point(8, 44);
        groupPlayerList.Name = "groupPlayerList";
        groupPlayerList.Size = new Size(407, 198);
        groupPlayerList.TabIndex = 4;
        groupPlayerList.TabStop = false;
        groupPlayerList.Text = "Players List";
        // 
        // playerControl4
        // 
        playerControl4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        playerControl4.Location = new Point(6, 154);
        playerControl4.Name = "playerControl4";
        playerControl4.Size = new Size(395, 38);
        playerControl4.TabIndex = 3;
        // 
        // playerControl3
        // 
        playerControl3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        playerControl3.Location = new Point(6, 110);
        playerControl3.Name = "playerControl3";
        playerControl3.Size = new Size(395, 38);
        playerControl3.TabIndex = 2;
        // 
        // playerControl2
        // 
        playerControl2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        playerControl2.Location = new Point(6, 66);
        playerControl2.Name = "playerControl2";
        playerControl2.Size = new Size(395, 38);
        playerControl2.TabIndex = 1;
        // 
        // playerControl1
        // 
        playerControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        playerControl1.Location = new Point(6, 22);
        playerControl1.Name = "playerControl1";
        playerControl1.Size = new Size(395, 38);
        playerControl1.TabIndex = 0;
        // 
        // FormPrincipal
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(427, 433);
        Controls.Add(groupPlayerList);
        Name = "FormPrincipal";
        Text = "FormPrincipal";
        groupPlayerList.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private GroupBox groupPlayerList;
    private PlayerControl playerControl1;
    private PlayerControl playerControl4;
    private PlayerControl playerControl3;
    private PlayerControl playerControl2;
}