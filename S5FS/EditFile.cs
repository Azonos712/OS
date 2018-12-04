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
    public partial class EditFile : Form
    {
        public EditFile()
        {
            InitializeComponent();
        }

        string temptext;

        private void button1_Click(object sender, EventArgs e)
        {
            Emulator.FillData(temptext, richTextBox1.Text);
            this.Close();
        }

        private void EditFile_Load(object sender, EventArgs e)
        {
            Work main = this.Owner as Work;
            if (main != null)
            {
                temptext = main.SelectFile;
                richTextBox1.Text = Emulator.ReadData(temptext);

                RootDirRecord tempR = Emulator.FindRec(main.SelectFile);
                Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
                if (Emulator.CheckWriteAccess(Emulator.CurrentUser, tempI) == false)
                {
                    richTextBox1.Enabled = false;
                    button1.Enabled = false;
                }
            }

        }
    }
}
