using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S5FS
{
    public partial class Group : Form
    {
        public Group()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UserManager main = this.Owner as UserManager;
            if (main != null)
            {
                main.temp1 = (byte)numericUpDown1.Value;
                this.Close();
            }
        }
    }
}
