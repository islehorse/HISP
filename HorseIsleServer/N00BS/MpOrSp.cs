using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HISP
{
    public partial class MpOrSp : Form
    {
        public bool Mutliplayer = false;

        public MpOrSp()
        {
            InitializeComponent();
        }

        private void Singleplayer_Click(object sender, EventArgs e)
        {
            Mutliplayer = false;
            DialogResult = DialogResult.OK;
        }

        private void Multiplayer_Click(object sender, EventArgs e)
        {
            Mutliplayer = true;
            DialogResult = DialogResult.OK;
        }

    }
}
