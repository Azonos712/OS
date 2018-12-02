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
    public partial class SignIn : Form
    {

        public SignIn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            switch (Emulator.CheckUser(textBox2.Text.ToString(), textBox1.Text.ToString()))
            {
                case 0:
                    MessageBox.Show("Неверное имя пользователя!");
                    break;
                case 1:
                    MessageBox.Show("Неверный пароль!");
                    break;
                case 2:
                    MessageBox.Show("Вы вошли в систему!");
                    Emulator.SetCurrentPosition("root");

                    Program.Context.MainForm = new Work();
                    this.Close();
                    Program.Context.MainForm.Show();
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SignUp su = new SignUp();
            su.ShowDialog();
        }
    }
}
