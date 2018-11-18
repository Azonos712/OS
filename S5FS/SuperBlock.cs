using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    class SuperBlock
    {
        public SuperBlock()
        {
            for (int i = 0; i < 100; i++)
                numID_Property.Add(i);

            for (int i = 0; i < 2; i++)
                numGroupID_Property.Add(i);
        }

        //Тип ФС
        private string FS_Type;
        public string FS_Type_Property
        {
            get { return FS_Type; }
            set { FS_Type = value; }
        }

        //Размер кластера(блока)
        private ushort Block_Size;
        public ushort Block_Size_Property
        {
            get { return Block_Size; }
            set { Block_Size = value; }
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


        //Данные не хранящиеся в ФС-------------------------------------------------------


        private List<int> numID=new List<int>();//Доступны ID
        public List<int> numID_Property
        {
            get { return numID; }
            set { numID = value; }
        }
        private List<int> numGroupID = new List<int>();//Доступные группы
        public List<int> numGroupID_Property
        {
            get { return numGroupID; }
            set { numGroupID = value; }
        }

        //Размер ЖД
        private int HDD_Size;
        public int HDD_Size_Property
        {
            get { return HDD_Size; }
            set { HDD_Size = value; }
        }

        //Размер битовой карты блоков в блоках
        private int Bitmap_Block_Size;
        public int Bitmap_Block_Size_Property
        {
            get { return Bitmap_Block_Size; }
           set { Bitmap_Block_Size = value; }
        }

        //Размер битовой карты инодов в блоках
        private int Inode_Bitmap_Block_Size;
        public int Inode_Bitmap_Block_Size_Property
        {
            get { return Inode_Bitmap_Block_Size; }
            set { Inode_Bitmap_Block_Size = value; }
        }
        
        //Размер массива инодов в блоках
        private int Inode_Block_Size;
        public int Inode_Block_Size_Property
        {
            get { return Inode_Block_Size; }
            set { Inode_Block_Size = value; }
        }

        //Размер информации пользователей в блоках
        private int User_Info_Block;
        public int User_Info_Block_Property
        {
            get { return User_Info_Block; }
            set { User_Info_Block = value; }
        }

        //Размер записей корневого каталога в блоках
        private int Record_Block;
        public int Record_Block_Property
        {
            get { return Record_Block; }
            set { Record_Block = value; }
        }

        //Размер данных в блоках
        private int Data_Block;
        public int Data_Block_Property
        {
            get { return Data_Block; }
            set { Data_Block = value; }
        }

        //Размер служебной части в блоках
        private int Service_Block;
        public int Service_Block_Property
        {
            get { return Service_Block; }
            set { Service_Block = value; }
        }

        //Количество пользователей
        private int Number_Users;
        public int Number_Users_Property
        {
            get { return Number_Users; }
            set { Number_Users = value; }
        }
    }
}
