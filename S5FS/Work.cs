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
    public partial class Work : Form
    {
        public Work()
        {
            InitializeComponent();
        }
        private Emulator emul = new Emulator();
        internal Emulator EMUL { get => emul; set => emul = value; }

        private void Work_Load(object sender, EventArgs e)
        {
            SignIn main = this.Owner as SignIn;
            if (main != null)
            {
                EMUL = main.Em1;
                main.Hide();
            }
        }

        private void Work_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
