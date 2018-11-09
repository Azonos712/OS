using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace S5FS
{
    class Emulator
    {
        //Путь к файлу
        private string Path;
        public string Path_Property
        {
            get { return Path; }
            set { Path = value; }
        }

        public void CreateMainFile()
        {
            FileStream fs = null;

            try
            {
                fs = File.Create(Path_Property);
                SuperBlock SB = new SuperBlock();
                SB.FS_Type_Property = "s5fs";
                SB.FS_Size_Property = 128000;
                SB.Inode_Size_Property = 3000000;
                SB.Block_Size_Property = 4096;
                fs.Write(Encoding.ASCII.GetBytes(SB.FS_Type_Property), 0, Encoding.ASCII.GetBytes(SB.FS_Type_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.FS_Size_Property), 0, BitConverter.GetBytes(SB.FS_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Block_Size_Property), 0, BitConverter.GetBytes(SB.Block_Size_Property).Length);
                //MessageBox.Show((SB.FS_Size_Property).ToString(), "Ошибка!");
                //StringBuilder sb = new StringBuilder();
                //foreach(var b in BitConverter.GetBytes(SB.FS_Size_Property))
                //{
                //   sb.Append(b);
                //}
                //MessageBox.Show( (sb).ToString() , "Ошибка!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!");
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
           
        }
    }
}
