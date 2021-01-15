using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.kougami.sign
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void Setting_Load(object sender, EventArgs e)
        {
            text_robotqq.Text = Config.Get("config.ini", "all", "robot");
        }

        private void text_robotqq_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void text_robotqq_TextChanged(object sender, EventArgs e)
        {
            Config.Set("config.ini", "all", "robot", text_robotqq.Text);
            if (text_robotqq.Text == "")
            {
                Program.enable = false;
            }
            else
            {
                Program.enable = true;
            }
        }
    }
}
