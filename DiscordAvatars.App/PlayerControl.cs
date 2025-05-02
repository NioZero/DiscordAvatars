using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordAvatars.App;

public partial class PlayerControl : UserControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    #region properties

    public bool PlayerEnabled
    {
        get { return chkPlayerEnabled.Checked; }
        set
        {
            chkPlayerEnabled.Checked = value;

        }
    }

    #endregion

    public PlayerControl()
    {
        InitializeComponent();
    }
}
