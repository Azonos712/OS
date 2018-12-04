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
    public partial class Prop : Form
    {
        public Prop()
        {
            InitializeComponent();
        }
        string temptext;
        RootDirRecord tempR;
        Inode tempI;
        string OldName;
        string OldEx;
        private void button1_Click(object sender, EventArgs e)
        {
            //Inode inode = new Inode();
            if (checkBox1.Checked == true)
            {
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                checkBox9.Checked = false;
            }
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

            tempI.Access_Property = Emulator.setAccess(acc);
            tempI.User_ID_Property = Convert.ToByte(textBox3.Text);
            tempI.Group_ID_Property = Convert.ToByte(textBox4.Text);
            tempI.File_Size_Property = Convert.ToInt32(textBox5.Text);
            tempI.File_Create_Property = Convert.ToDateTime(textBox6.Text);
            tempI.File_Modif_Property = Convert.ToDateTime(textBox7.Text);

            tempI.Number_Property = tempR.Number_Inode_Property;

            RootDirRecord newtempR = new RootDirRecord();
            newtempR.Name_Property = textBox1.Text;
            newtempR.Name_Property += (new string(' ', 20 - newtempR.Name_Property.Length));
            newtempR.Exstension_Property = textBox2.Text;
            newtempR.Exstension_Property += (new string(' ', 5 - newtempR.Exstension_Property.Length));
            newtempR.Number_Inode_Property = tempR.Number_Inode_Property;
            //&& OldEx== newtempR.Exstension_Property.Replace(" ", string.Empty)
            if (OldName== newtempR.Name_Property.Replace(" ", string.Empty) )
            {
                this.Close();
                Emulator.Replacement(tempR.Name_Property.Replace(" ", string.Empty), newtempR, tempI);
            }
            else
            {
                if (Emulator.CheckRecord(newtempR.Name_Property.Replace(" ", string.Empty), newtempR.Exstension_Property.Replace(" ", string.Empty)) == true)
                {
                    this.Close();
                    Emulator.Replacement(tempR.Name_Property.Replace(" ", string.Empty), newtempR, tempI);
                }
                else
                {
                    MessageBox.Show("Запись с таким именем уже существует!");
                }
            }
            
        }

        private void Properties_Load(object sender, EventArgs e)
        {
            Work main = this.Owner as Work;
            if (main != null)
            {
                temptext = main.SelectFile;
                tempR = Emulator.FindRec(temptext);
                tempI = Emulator.FindInode(tempR.Number_Inode_Property);

                textBox1.Text = tempR.Name_Property.Replace(" ", string.Empty);
                OldName = tempR.Name_Property.Replace(" ", string.Empty);
                textBox2.Text = tempR.Exstension_Property.Replace(" ", string.Empty);
                OldEx = tempR.Exstension_Property.Replace(" ", string.Empty);
                textBox3.Text = tempI.User_ID_Property.ToString();
                textBox4.Text = tempI.Group_ID_Property.ToString();
                textBox5.Text = tempI.File_Size_Property.ToString();
                textBox6.Text = tempI.File_Create_Property.ToString();
                textBox7.Text = tempI.File_Modif_Property.ToString();

                string acc = Emulator.getAccess(tempI.Access_Property);

                checkBox4.Checked = (Char.GetNumericValue(acc[1]) == 1) ? true : false;
                checkBox7.Checked = (Char.GetNumericValue(acc[2]) == 1) ? true : false;
                checkBox10.Checked = (Char.GetNumericValue(acc[3]) == 1) ? true : false;
                checkBox5.Checked = (Char.GetNumericValue(acc[4]) == 1) ? true : false;
                checkBox8.Checked = (Char.GetNumericValue(acc[5]) == 1) ? true : false;
                checkBox11.Checked = (Char.GetNumericValue(acc[6]) == 1) ? true : false;
                checkBox6.Checked = (Char.GetNumericValue(acc[7]) == 1) ? true : false;
                checkBox9.Checked = (Char.GetNumericValue(acc[8]) == 1) ? true : false;
                checkBox12.Checked = (Char.GetNumericValue(acc[9]) == 1) ? true : false;

                checkBox1.Checked = (Char.GetNumericValue(acc[10]) == 1) ? true : false;
                checkBox2.Checked = (Char.GetNumericValue(acc[11]) == 1) ? true : false;
                checkBox3.Checked = (Char.GetNumericValue(acc[12]) == 1) ? true : false;

                if (main.Change == false)
                {
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;
                    checkBox4.Enabled = false;
                    checkBox5.Enabled = false;
                    checkBox6.Enabled = false;
                    checkBox7.Enabled = false;
                    checkBox8.Enabled = false;
                    checkBox9.Enabled = false;
                    checkBox10.Enabled = false;
                    checkBox11.Enabled = false;
                    checkBox12.Enabled = false;
                    button1.Enabled = false;
                }
            }
        }
    }
}
