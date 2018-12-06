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
    public partial class Work : Form
    {
        public Work()
        {
            InitializeComponent();
            //Добавления колонок
            listView1.ColumnClick += new ColumnClickEventHandler(ClickOnColumn);
            ColumnHeader c = new ColumnHeader();
            c.Text = "Имя";
            c.Width = c.Width + 80;
            ColumnHeader c2 = new ColumnHeader();
            c2.Text = "Размер";
            c2.Width = c2.Width + 60;
            ColumnHeader c3 = new ColumnHeader();
            c3.Text = "Тип";
            c3.Width = c3.Width + 60;
            ColumnHeader c4 = new ColumnHeader();
            c4.Text = "Изменен";
            c4.Width = c4.Width + 60;
            listView1.Columns.Add(c);
            listView1.Columns.Add(c2);
            listView1.Columns.Add(c3);
            listView1.Columns.Add(c4);

        }

        private string SF;
        public string SelectFile
        {
            get { return SF; }
            set { SF = value; }
        }
        //Активность контролов в свойствах
        private bool Ch;
        public bool Change
        {
            get { return Ch; }
            set { Ch = value; }
        }

        private RootDirRecord SavedRecord;
        private Inode SavedInode;
        private string textdata;
        private int numofcopy;

        //private Emulator emul = new Emulator();
        //internal Emulator EMUL { get => emul; set => emul = value; }



        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateFile CF = new CreateFile();
            //CF.Owner = this;
            CF.ShowDialog();
            RefreshTreeList();

            удалитьToolStripMenuItem.Enabled = false;
            копироватьToolStripMenuItem.Enabled = false;
            свойстваToolStripMenuItem.Enabled = false;
        }

        private void менеджерПользователейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserManager UM = new UserManager();
            UM.Owner = this;
            UM.ShowDialog();
            RefreshTreeList();

            удалитьToolStripMenuItem.Enabled = false;
            копироватьToolStripMenuItem.Enabled = false;
            свойстваToolStripMenuItem.Enabled = false;
        }

        private void Work_Load(object sender, EventArgs e)
        {
            RefreshTreeList();
        }

        private void ClickOnColumn(object sender, ColumnClickEventArgs e)
        {
            //обработка нажатия на колонку имя(изменение порядка сортировки) 
            if (e.Column == 0)
            {
                if (listView1.Sorting == SortOrder.Descending)
                    listView1.Sorting = SortOrder.Ascending;
                else
                    listView1.Sorting = SortOrder.Descending;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            RootDirRecord tempR = Emulator.FindRec(e.Node.Text);
            Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
            if (Emulator.CheckID(Emulator.CurrentUser, tempI) == true)
            {
                listView1.Items.Clear();
                toolStripTextBox1.Text = e.Node.Name;
                Emulator.SetCurrentPosition(e.Node.Text);
                setListView();
            }
            else
            {
                MessageBox.Show("У вас недостаточно прав!");
            }
        }

        public void setListView()
        {
            //Заполнение ListView
            Emulator.GetAllFromDir(Emulator.RecCurPos.Number_Inode_Property);
            ListViewItem lw = new ListViewItem();
            string[] str1 = Emulator.Dirs.ToArray();
            foreach (string s1 in str1)
            {
                lw = new ListViewItem(s1, 0);
                lw.Name = s1;
                listView1.Items.Add(lw);
            }
            string[] str2 = Emulator.Files.ToArray();
            foreach (string s2 in str2)
            {
                lw = new ListViewItem(s2, 1);
                lw.Name = s2;
                listView1.Items.Add(lw);
            }

            if (отображатьСкрытыеФайлыToolStripMenuItem.Checked == true)
            {
                string[] str3 = Emulator.HideFiles.ToArray();
                foreach (string s3 in str3)
                {
                    lw = new ListViewItem(s3, 2);
                    lw.Name = s3;
                    listView1.Items.Add(lw);
                }
            }

            if (Emulator.CurrentUser.Group_ID_Property == 0)
            {
                string[] str4 = Emulator.SysFiles.ToArray();
                foreach (string s4 in str4)
                {
                    lw = new ListViewItem(s4, 3);
                    lw.Name = s4;
                    listView1.Items.Add(lw);
                }
            }

            if (Emulator.CurrentUser.Group_ID_Property == 0 && отображатьСкрытыеФайлыToolStripMenuItem.Checked == true)
            {
                string[] str5 = Emulator.HideSysFiles.ToArray();
                foreach (string s5 in str5)
                {
                    lw = new ListViewItem(s5, 4);
                    lw.Name = s5;
                    listView1.Items.Add(lw);
                }
            }
        }



        private void списокИконокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
            //listView1.Items.Clear();
            //setListView();
        }

        private void списокИзображенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void плиткиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
            //listView1.Items.Clear();
            //setListView();
        }

        private void списокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
            //listView1.Items.Clear();
            //setListView();
        }

        private void таблицаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            //listView1.Items.Clear();
            //setListView();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex == 0)
            {
                int index = listView1.SelectedIndices[0];
                SelectFile = listView1.Items[index].Text;
                RootDirRecord tempR = Emulator.FindRec(SelectFile);
                Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
                if (Emulator.CheckID(Emulator.CurrentUser, tempI) == true)
                {
                    foreach (TreeNode node in treeView1.Nodes)
                    {
                        if (node.Text == listView1.SelectedItems[0].Text)
                        {
                            treeView1.SelectedNode = node;
                            treeView1.Select();
                            break;
                        }
                        foreach (TreeNode innode in node.Nodes)
                        {
                            if (innode.Text == listView1.SelectedItems[0].Text)
                            {
                                treeView1.SelectedNode = innode;
                                treeView1.Select();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("У вас недостаточно прав!");
                }
            }
            else
            {
                int index = listView1.SelectedIndices[0];
                SelectFile = listView1.Items[index].Text;
                SelectFile = SelectFile.Substring(0, SelectFile.LastIndexOf('.'));
                RootDirRecord tempR = Emulator.FindRec(SelectFile);
                Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
                if (Emulator.CheckReadAccess(Emulator.CurrentUser, tempI) == true)
                {
                    EditFile EF = new EditFile();
                    EF.Owner = this;
                    EF.ShowDialog();
                    //удалитьToolStripMenuItem.Enabled = false;
                    //копироватьToolStripMenuItem.Enabled = false;
                    //свойстваToolStripMenuItem.Enabled = false;
                }
                else
                {
                    MessageBox.Show("У вас недостаточно прав!");
                }
            }

        }

        private void свойстваToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listView1.SelectedIndices[0];
            SelectFile = listView1.Items[index].Text;
            SelectFile = SelectFile.Substring(0, SelectFile.LastIndexOf('.'));
            RootDirRecord tempR = Emulator.FindRec(SelectFile);
            Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
            if (Emulator.CheckID(Emulator.CurrentUser, tempI) == true)
            {
                Change = true;
            }
            else
            {
                Change = false;
            }

            Prop P = new Prop();
            P.Owner = this;
            P.ShowDialog();

            удалитьToolStripMenuItem.Enabled = false;
            копироватьToolStripMenuItem.Enabled = false;
            свойстваToolStripMenuItem.Enabled = false;

            RefreshTreeList();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int index = listView1.SelectedIndices[0];
            SelectFile = listView1.Items[index].Text;
            SelectFile = SelectFile.Substring(0, SelectFile.LastIndexOf('.'));

            RootDirRecord tempR = Emulator.FindRec(SelectFile);
            Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
            if (Emulator.CheckID(Emulator.CurrentUser, tempI) == true)
            {

                int[] MainPosandNum = Emulator.FindBind(Emulator.CurrentPosition, SelectFile);
                int Seek = Emulator.FindRecSeek(SelectFile);
                UserInfo tempU = Emulator.FindUser(tempI.User_ID_Property);
                Emulator.DeleteRIB(MainPosandNum[0], Seek, tempU, MainPosandNum[1], false);

                RefreshTreeList();

            }
            else
            {
                MessageBox.Show("У вас недостаточно прав!");
            }
            удалитьToolStripMenuItem.Enabled = false;
            копироватьToolStripMenuItem.Enabled = false;
            свойстваToolStripMenuItem.Enabled = false;


            RefreshTreeList();
            //this.Show();
            //Activate();
            //this.Activate();
            //this.OnActivated();
            //this.Activated;
            //this.Refresh();
        }

        public void RefreshTreeList()
        {
            bool IsOpen = false;
            TreeNode cur = new TreeNode();
            if (treeView1.Nodes.Count != 0)
            {
                IsOpen = treeView1.Nodes[0].IsExpanded;
                cur = treeView1.SelectedNode;
            }
            treeView1.Nodes.Clear();
            listView1.Clear();
            //Заполнение TreeView
            Emulator.GetAllFromDir(0);
            string[] str = Emulator.Dirs.ToArray();
            treeView1.Nodes.Add("root", "root", 0);
            treeView1.SelectedNode = treeView1.Nodes[0];
            treeView1.Select();

            foreach (string s in str)
            {
                treeView1.Nodes[0].Nodes.Add("root\\" + s, s, 0);
            }

            if (IsOpen == true)
            {
                treeView1.ExpandAll();
            }

            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Text == cur.Text)
                {
                    treeView1.SelectedNode = node;
                    treeView1.Select();
                    break;
                }
                foreach (TreeNode innode in node.Nodes)
                {
                    if (innode.Text == cur.Text)
                    {
                        treeView1.SelectedNode = innode;
                        treeView1.Select();
                        break;
                    }
                }
            }
            //treeView1.SelectedNode = cur;
            //treeView1.Select();

        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listView1.SelectedIndices[0];
            SelectFile = listView1.Items[index].Text;
            SelectFile = SelectFile.Substring(0, SelectFile.LastIndexOf('.'));

            RootDirRecord tempR = Emulator.FindRec(SelectFile);
            Inode tempI = Emulator.FindInode(tempR.Number_Inode_Property);
            if (Emulator.CheckID(Emulator.CurrentUser, tempI) == true)
            {
                SavedRecord = Emulator.FindRec(SelectFile);

                SavedInode = Emulator.FindInode(SavedRecord.Number_Inode_Property);

                textdata = Emulator.ReadData(SelectFile);

                numofcopy = 1;

                вставитьToolStripMenuItem.Enabled = true;
            }
            else
            {
                MessageBox.Show("У вас недостаточно прав!");
            }
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavedRecord.Number_Inode_Property = Emulator.getFreeInode();
            RootDirRecord ForCopy = new RootDirRecord();
            ForCopy.Name_Property = SavedRecord.Name_Property;
            ForCopy.Exstension_Property = SavedRecord.Exstension_Property;
            ForCopy.Number_Inode_Property = SavedRecord.Number_Inode_Property;
            ForCopy.Name_Property = ForCopy.Name_Property.Replace(" ", string.Empty);
            ForCopy.Name_Property = SavedRecord.Name_Property.Replace(" ", string.Empty) + '_' + numofcopy;
            ForCopy.Name_Property += (new string(' ', 20 - ForCopy.Name_Property.Length));
            numofcopy++;

            SavedInode.Number_Property = ForCopy.Number_Inode_Property;
            SavedInode.File_Create_Property = DateTime.Now;
            SavedInode.File_Modif_Property = DateTime.Now;
            for (int i = 0; i < SavedInode.Array_Of_Address_Property.Length; i++)
            {
                SavedInode.Array_Of_Address_Property[i] = 0;
            }

            Emulator.CreateRecord(ForCopy, SavedInode);
            Emulator.Bind(Emulator.RecCurPos.Number_Inode_Property);
            Emulator.FillData(ForCopy.Name_Property.Replace(" ", string.Empty), textdata);

            RefreshTreeList();
        }



        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                if (listView1.SelectedItems[0].ImageIndex == 0)
                {
                    удалитьToolStripMenuItem.Enabled = false;
                    копироватьToolStripMenuItem.Enabled = false;
                    свойстваToolStripMenuItem.Enabled = false;
                }
                else
                {
                    удалитьToolStripMenuItem.Enabled = true;
                    копироватьToolStripMenuItem.Enabled = true;
                    свойстваToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void отображатьСкрытыеФайлыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshTreeList();
        }

        private void планировщикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.Context.MainForm = new Plan();
            this.Close();
            Program.Context.MainForm.Show();
        }
    }
}
