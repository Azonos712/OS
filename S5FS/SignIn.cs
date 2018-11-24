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

        private Emulator em1 = new Emulator();
        internal Emulator Em1 { get => em1; set => em1 = value; }
        


        private void button1_Click(object sender, EventArgs e)
        {
            //Em1.fs_Property = File.OpenRead(Em1.Path_Property);//Чтение файла ФС по указаному пути
            switch (Em1.CheckUser(textBox2.Text.ToString(), textBox1.Text.ToString()))
            {
                case 0:
                    MessageBox.Show("Неверное имя пользователя!");
                    break;
                case 1:
                    MessageBox.Show("Неверный пароль!");
                    break;
                case 2:
                    MessageBox.Show("Вы вошли в систему!");
                    Work w = new Work();
                    w.Owner = this;
                    w.ShowDialog();
                    break;
            }
            //Em1.fs_Property.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SignUp su = new SignUp();
            su.Owner = this;
            su.ShowDialog();
        }

        private void SignIn_Load(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                Em1 = main.Em;
                main.Hide();
            }
        }

        private void SignIn_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
