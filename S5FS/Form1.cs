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
            //this.EnableBlur
        }
        
        public int temp1;
        public ushort temp2;

        private void button1_Click(object sender, EventArgs e)
        {
            if (Emulator.Path_Property != null)
            {
                if (System.IO.File.Exists(Emulator.Path_Property)==true)
                {
                    MessageBox.Show("Файл присутствует,считываем данные");
                    Emulator.ReadMainFile();
                }
                else
                {
                    MessageBox.Show("Файл отсутствует,создаём файл");
                    Create nf = new Create();
                    nf.Owner = this;
                    nf.ShowDialog();
                    Emulator.CreateMainFile(temp1, temp2);
                }
                
                Program.Context.MainForm = new SignIn();
                this.Close();
                Program.Context.MainForm.Show();
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
                Emulator.Path_Property= fbd.SelectedPath + "\\FS.fs";//приписываем к пути выходной файл
                textBox_path.Text = Emulator.Path_Property;//отображает выбранный путь на форме
            }
        }
    }
}
