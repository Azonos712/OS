using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    class RootDirRecord
    {
        //Имя
        private string Name;
        public string Name_Property
        {
            get { return Name; }
            set { Name = value; }
        }

        //Расширение
        private string Exstension;
        public string Exstension_Property
        {
            get { return Exstension; }
            set { Exstension = value; }
        }

        //Номер inode
        private int Number_Inode;
        public int Number_Inode_Property
        {
            get { return Number_Inode; }
            set { Number_Inode = value; }
        }
    }
}
