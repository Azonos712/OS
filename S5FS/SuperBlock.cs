using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    class SuperBlock
    {
        //Тип ФС
        private string FS_Type;
        public string FS_Type_Property
        {
            get { return FS_Type; }
            set { FS_Type = value; }
        }

        //Размер ФС в блоках
        private int FS_Size;
        public int FS_Size_Property
        {
            get { return FS_Size; }
            set { FS_Size = value; }
        }

        //Размер массива инодов
        private int Inode_Size;
        public int Inode_Size_Property
        {
            get { return Inode_Size; }
            set { Inode_Size = value; }
        }

        //Размер кластера(блока)
        private int Block_Size;
        public int Block_Size_Property
        {
            get { return Block_Size; }
            set { Block_Size = value; }
        }

        //Число свободных блоков
        private int Block_Free;
        public int Block_Free_Property
        {
            get { return Block_Free; }
            set { Block_Free = value; }
        }

        //Число свободных инодов
        private int Inode_Free;
        public int Inode_Free_Property
        {
            get { return Inode_Free; }
            set { Inode_Free = value; }
        }

        //Размер логического блока
        private int Block_Log;
        public int Block_Log_Property
        {
            get { return Block_Log; }
            set { Block_Log = value; }
        }

        //Размер битовой карты инодов
        private int Inode_Bitmap_Size;
        public int Inode_Bitmap_Size_Property
        {
            get { return Inode_Bitmap_Size; }
            set { Inode_Bitmap_Size = value; }
        }

        //Размер битовой карты
        private int Bitmap_Size;
        public int Bitmap_Size_Property
        {
            get { return Bitmap_Size; }
            set { Bitmap_Size = value; }
        }
    }
}
