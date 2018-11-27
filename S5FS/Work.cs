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
        //private Emulator emul = new Emulator();
        //internal Emulator EMUL { get => emul; set => emul = value; }



        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateFile CF = new CreateFile();
            //CF.Owner = this;
            CF.ShowDialog();
        }

        private void менеджерПользователейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserManager UM = new UserManager();
            UM.Owner = this;
            UM.ShowDialog();
        }

        private void Work_Load(object sender, EventArgs e)
        {

        }
    }
}
