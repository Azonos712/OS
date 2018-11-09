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

        Emulator em = new Emulator();

        private void button1_Click(object sender, EventArgs e)
        {
            if (em.Path_Property != null)
            {
                em.CreateMainFile();
            }
            else
            {
                MessageBox.Show("Вы не выбрали путь сохранения файла!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_path_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();//предлагает пользователю выбрать папку
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                em.Path_Property= fbd.SelectedPath + "\\FS.txt";//приписываем к пути выходной файл
                textBox_path.Text = em.Path_Property;//отображает выбранный путь на форме
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
