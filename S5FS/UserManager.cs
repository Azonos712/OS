using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S5FS
{
    public partial class UserManager : Form
    {
        public UserManager()
        {
            InitializeComponent();
        }

        private void UserManager_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns.Add("Логин", 45);
            listView1.Columns.Add("ID пользователя", 100);
            listView1.Columns.Add("ID группы", 100);
            listView1.Columns.Add("Домашняя директория", 150);


            Emulator.fs.Seek((1 + Emulator.SB.Bitmap_Block_Size_Property + Emulator.SB.Inode_Bitmap_Block_Size_Property + Emulator.SB.Inode_Block_Size_Property) * Emulator.SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < Emulator.SB.Max_Number_Users_Property; i++)
            {
                UserInfo usertemp = Emulator.ReadUser();
                if (usertemp.ID_Property == 0 && usertemp.Group_ID_Property == 0 && usertemp.Login_Property == (new string('\0', 12)) && usertemp.Hash_Property.SequenceEqual(new byte[32]) == true && usertemp.Homedir_Property == (new string('\0', 255)))
                {
                    continue;
                }
                string[] temp = { usertemp.Login_Property.Replace(" ", string.Empty), usertemp.ID_Property.ToString().Replace(" ", string.Empty), usertemp.Group_ID_Property.ToString().Replace(" ", string.Empty), usertemp.Homedir_Property.Replace(" ", string.Empty) };
                ListViewItem lvi = new ListViewItem(temp);
                if (usertemp.Login_Property == Emulator.CurrentUser.Login_Property)
                {
                    lvi.BackColor = Color.GreenYellow;
                    //listView1.Items[i].BackColor = Color.GreenYellow;
                }
                listView1.Items.Add(lvi);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReEnter();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public void ReEnter()
        {
            Work main = this.Owner as Work;
            if (main != null)
            {
                Program.Context.MainForm = new SignIn();
                main.Close();
                this.Close();
                Program.Context.MainForm.Show();
            }
        }
    }
}
