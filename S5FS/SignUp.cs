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
    public partial class SignUp : Form
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SignIn main = this.Owner as SignIn;
            if (main != null)
            {
                switch (main.Em1.CheckUser(textBox2.Text.ToString(), textBox1.Text.ToString()))
                {
                    case 0:
                        UserInfo temp = new UserInfo();
                        temp.ID_Property = (byte)main.Em1.SB.numID_Property.First();
                        main.Em1.SB.numID_Property.Remove(temp.ID_Property);
                        temp.Group_ID_Property = 1;
                        temp.Login_Property = textBox2.Text.ToString();
                        temp.Login_Property += (new string(' ', 12 - temp.Login_Property.Length));
                        temp.Hash_Property = main.Em1.getHashSHA256(textBox1.Text.ToString());
                        temp.Homedir_Property = "RootDir\\" + textBox2.Text.ToString() + "\\";
                        temp.Homedir_Property += (new string(' ', 255 - temp.Homedir_Property.Length));
                        main.Em1.AddUser(temp);
                        MessageBox.Show("Вы успешно зарегистрировались!");
                        this.Close();
                        break;
                    case 1:
                        MessageBox.Show("Имя пользователя уже используется!");
                        break;
                    case 2:
                        MessageBox.Show("Вы уже зарегестрированы!");
                        break;
                }
            }
        }
    }
}
