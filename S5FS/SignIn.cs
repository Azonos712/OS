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
    public partial class SignIn : Form
    {

        public SignIn()
        {
            InitializeComponent();
        }
        Emulator newem = new Emulator();
        private void button1_Click(object sender, EventArgs e)
        {
            string l = newem.Path_Property;
            bool b=newem.CheckUser(textBox1.Text.ToString(), textBox2.Text.ToString());
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
                newem = main.Em;
                main.Hide();
            }
        }

        private void SignIn_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }


    }
}
