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
        }

        private void менеджерПользователейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserManager UM = new UserManager();
            UM.Owner = this;
            UM.ShowDialog();
        }

        private void Work_Load(object sender, EventArgs e)
        {

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
            listView1.Items.Clear();
            toolStripTextBox1.Text = e.Node.Name;
            Emulator.SetCurrentPosition(e.Node.Text);
            setListView();
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
        }

        private void Work_Activated(object sender, EventArgs e)
        {
            RefreshTreeList();


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
                EditFile EF = new EditFile();
                int index = listView1.SelectedIndices[0];
                SelectFile = listView1.Items[index].Text;
                EF.Owner = this;
                EF.ShowDialog();
            }
        }

        private void свойстваToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Prop P = new Prop();
            int index = listView1.SelectedIndices[0];
            SelectFile = listView1.Items[index].Text;
            P.Owner = this;
            P.ShowDialog();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listView1.SelectedIndices[0];
            SelectFile = listView1.Items[index].Text;

            int[] MainPosandNum = Emulator.FindBind(Emulator.CurrentPosition, SelectFile);
            int Seek = Emulator.FindRecSeek(SelectFile);
            Emulator.DeleteRIB(MainPosandNum[0], Seek, Emulator.CurrentUser, MainPosandNum[1]);

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

            SavedRecord = Emulator.FindRec(SelectFile);

            SavedInode = Emulator.FindInode(SavedRecord.Number_Inode_Property);

            textdata = Emulator.ReadData(SelectFile);

            numofcopy = 1;
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
    }
}
