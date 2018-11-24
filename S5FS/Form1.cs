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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        public int temp1;
        public ushort temp2;
        
        private Emulator em = new Emulator();
        internal Emulator Em { get => em; set => em = value; }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Em.Path_Property != null)
            {
                if (System.IO.File.Exists(Em.Path_Property)==true)
                {
                    MessageBox.Show("Файл присутствует,считываем данные");
                    Em.ReadMainFile();
                }
                else
                {
                    MessageBox.Show("Файл отсутствует,создаём файл");
                    Create nf = new Create();
                    nf.Owner = this;
                    nf.ShowDialog();
                    Em.CreateMainFile(temp1, temp2);
                }
                SignIn si = new SignIn();
                si.Owner = this;
                si.ShowDialog();
            }
            else
            {
                MessageBox.Show("Вы не выбрали путь к файлу!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_path_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();//предлагает пользователю выбрать папку
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Em.Path_Property= fbd.SelectedPath + "\\FS.fs";//приписываем к пути выходной файл
                textBox_path.Text = Em.Path_Property;//отображает выбранный путь на форме
            }
        }
    }
}
