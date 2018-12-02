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
    public partial class CreateFile : Form
    {
        public CreateFile()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Inode inode = new Inode();
            string acc = "1";
            acc += (checkBox4.Checked == true) ? '1' : '0';
            acc += (checkBox7.Checked == true) ? '1' : '0';
            acc += (checkBox10.Checked == true) ? '1' : '0';
            acc += (checkBox5.Checked == true) ? '1' : '0';
            acc += (checkBox8.Checked == true) ? '1' : '0';
            acc += (checkBox11.Checked == true) ? '1' : '0';
            acc += (checkBox6.Checked == true) ? '1' : '0';
            acc += (checkBox9.Checked == true) ? '1' : '0';
            acc += (checkBox12.Checked == true) ? '1' : '0';

            acc += (checkBox1.Checked == true) ? '1' : '0';
            acc += (checkBox2.Checked == true) ? '1' : '0';
            acc += (checkBox3.Checked == true) ? '1' : '0';

            inode.Access_Property = Emulator.setAccess(acc);
            inode.User_ID_Property = Convert.ToByte(textBox3.Text);
            inode.Group_ID_Property = Convert.ToByte(textBox4.Text);
            inode.File_Size_Property = Convert.ToInt32(textBox5.Text);
            inode.File_Create_Property = Convert.ToDateTime(textBox6.Text);
            inode.File_Modif_Property = Convert.ToDateTime(textBox7.Text);
            inode.Block_Count_Property = 0;

            for (int i = 0; i < inode.Array_Of_Address_Property.Length; i++)
            {
                inode.Array_Of_Address_Property[i] = 0;
            }
            inode.Number_Property = Emulator.getFreeInode();

            RootDirRecord rec = new RootDirRecord();
            rec.Name_Property = textBox1.Text;
            rec.Name_Property += (new string(' ', 20 - rec.Name_Property.Length));
            rec.Exstension_Property = textBox2.Text;
            rec.Exstension_Property += (new string(' ', 5 - rec.Exstension_Property.Length));
            rec.Number_Inode_Property = inode.Number_Property;

            this.Close();
            Emulator.CreateRecord(rec, inode);
            Emulator.Bind(Emulator.RecCurPos.Number_Inode_Property);
        }

        private void CreateFile_Load(object sender, EventArgs e)
        {
                textBox3.Text = Emulator.CurrentUser.ID_Property.ToString();
                textBox4.Text = Emulator.CurrentUser.Group_ID_Property.ToString();
                textBox5.Text = 0.ToString();
                textBox6.Text = DateTime.Now.ToString();
                textBox7.Text = DateTime.Now.ToString();
        }
    }
}
