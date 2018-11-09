using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    class Inode
    {
        //Права доступа
        private ushort Access;
        public ushort Access_Property
        {
            get { return Access; }
            set { Access = value; }
        }

        //ID владельца
        private byte User_ID;
        public byte User_ID_Property
        {
            get { return User_ID; }
            set { User_ID = value; }
        }

        //ID группы
        private byte Group_ID;
        public byte Group_ID_Property
        {
            get { return Group_ID; }
            set { Group_ID = value; }
        }

        //Размер файла
        private int File_Size;
        public int File_Size_Property
        {
            get { return File_Size; }
            set { File_Size = value; }
        }

        //Дата создания файла
        private DateTime File_Create;//8байт
        public DateTime File_Create_Property
        {
            get { return File_Create; }
            set { File_Create = value; }
        }

        //Дата последней модификации файла
        private DateTime File_Modif;
        public DateTime File_Modif_Property
        {
            get { return File_Modif; }
            set { File_Modif = value; }
        }

        //Число блоков, занимаемых файлом
        private int Block_Count;
        public int Block_Count_Property
        {
            get { return Block_Count; }
            set { Block_Count = value; }
        }

        //Массив адресов блоков данных файла
        private int[] A_Block_Address;
        public int[] A_Block_Address_Property
        {
            get { return A_Block_Address; }
            set { A_Block_Address = value; }
        }
    }
}
