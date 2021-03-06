﻿using System;
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

        public byte temp1;

        private void UserManager_Load(object sender, EventArgs e)
        {
            SetListOfUsers();
        }

        public void SetListOfUsers()
        {
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("Логин", 60);
            listView1.Columns.Add("ID пользователя", 100);
            listView1.Columns.Add("ID группы", 70);
            listView1.Columns.Add("Домашняя директория", 130);
            listView1.Columns.Add("Тип пользователя", 110);


            Emulator.fs.Seek((1 + Emulator.SB.Bitmap_Block_Size_Property + Emulator.SB.Inode_Bitmap_Block_Size_Property + Emulator.SB.Inode_Block_Size_Property) * Emulator.SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < Emulator.SB.Max_Number_Users_Property; i++)
            {
                UserInfo usertemp = Emulator.ReadUser();
                if (usertemp.ID_Property == 0 && usertemp.Group_ID_Property == 0 && usertemp.Login_Property == (new string('\0', 12)) && usertemp.Hash_Property.SequenceEqual(new byte[32]) == true && usertemp.Homedir_Property == (new string('\0', 255)))
                {
                    continue;
                }
                string type;
                if (usertemp.Group_ID_Property != 0)
                {
                    type = "User";
                }
                else
                {
                    type = "Admin";
                }
                string[] temp = { usertemp.Login_Property.Replace(" ", string.Empty), usertemp.ID_Property.ToString().Replace(" ", string.Empty), usertemp.Group_ID_Property.ToString().Replace(" ", string.Empty), usertemp.Homedir_Property.Replace(" ", string.Empty), type };
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
            if (listView1.SelectedIndices.Count != 0)
            {
                int index = listView1.SelectedIndices[0];
                if ((listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString()) || (Emulator.CurrentUser.Group_ID_Property == 0))
                {
                    if (listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) != 0.ToString())
                    {
                        Emulator.DeleteUser(listView1.Items[index].SubItems[0].Text.Replace(" ", string.Empty), listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty));
                        MessageBox.Show("Удаление из системы завершено!");
                        if ((Emulator.CurrentUser.Group_ID_Property != 0) || listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString())
                        {
                            ReEnter();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователя root невозможно удалить!");
                    }
                }
                else
                {
                    MessageBox.Show("Вы можете удалить из системы только себя!");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя!");
            }

            SetListOfUsers();
            listView1.SelectedIndices.Clear();

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

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count != 0)
            {
                int index = listView1.SelectedIndices[0];
                if (listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString() || (Emulator.CurrentUser.Group_ID_Property == 0))
                {
                    if (listView1.Items[index].SubItems[2].Text.Replace(" ", string.Empty) != 0.ToString() || (listView1.Items[index].SubItems[2].Text.Replace(" ", string.Empty) == 0.ToString() && listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString()) || Emulator.CurrentUser.ID_Property == 0)
                    //if (listView1.Items[index].SubItems[2].Text.Replace(" ", string.Empty) == 0.ToString() && listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString() && listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) != 0.ToString() || listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) == Emulator.CurrentUser.ID_Property.ToString())
                    {
                        if (listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty) != 0.ToString())
                        {
                            Group G = new Group();
                            G.Owner = this;
                            G.ShowDialog();
                            if ((temp1 == 0 && Emulator.CurrentUser.Group_ID_Property == 0) || temp1 != 0)
                            {
                                if (temp1 != Convert.ToInt32(listView1.Items[index].SubItems[2].Text.Replace(" ", string.Empty)))
                                {
                                    Emulator.ChangeUserGroup(listView1.Items[index].SubItems[0].Text.Replace(" ", string.Empty), listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty), temp1);
                                    if (Emulator.CurrentUser.ID_Property.ToString() == listView1.Items[index].SubItems[1].Text.Replace(" ", string.Empty))
                                    {
                                        Emulator.CurrentUser.Group_ID_Property = temp1;
                                    }
                                    MessageBox.Show("Изменение группы завершено!");
                                }
                                else
                                {
                                    MessageBox.Show("Вы изменяете группу на тоже значение!");
                                }
                            }
                            else
                            {
                                MessageBox.Show("В группу администраторов могут добавить только администраторы!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Группу пользователя root невозможно изменить!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Можно изменить только свою группу администратора!");
                    }
                }
                else
                {
                    MessageBox.Show("Вы можете изменить только свою группу");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя!");
            }

            SetListOfUsers();
            listView1.SelectedIndices.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Emulator.CurrentUser.Group_ID_Property == 0)
            {
                SignUp SU = new SignUp();
                //CF.Owner = this;
                SU.ShowDialog();
                SetListOfUsers();
                listView1.SelectedIndices.Clear();
            }
            else
            {
                MessageBox.Show("Пользователей могут добавлять только администраторы!");
            }
        }
    }
}
