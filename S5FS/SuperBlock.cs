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

            //for (int i = 0; i < 2; i++)
               //numGroupID_Property.Add(i);
        }
        
        private string FS_Type;
        /// <summary>
        /// Тип файловой системы (Её название)
        /// </summary>
        public string FS_Type_Property
        {
            get { return FS_Type; }
            set { FS_Type = value; }
        }
        
        private ushort One_Block_Size;
        /// <summary>
        /// Размер 1 кластера (Блока)
        /// </summary>
        public ushort One_Block_Size_Property
        {
            get { return One_Block_Size; }
            set { One_Block_Size = value; }
        }
        
        private int FS_Block_Size;
        /// <summary>
        /// Размер файловой системы в блоках
        /// </summary>
        public int FS_Block_Size_Property
        {
            get { return FS_Block_Size; }
            set { FS_Block_Size = value; }
        }
        
        private int Inode_Size;
        /// <summary>
        /// Размер области инодов
        /// </summary>
        public int Inode_Size_Property
        {
            get { return Inode_Size; }
            set { Inode_Size = value; }
        }
        
        private int Block_Free;
        /// <summary>
        /// Количество свободных блоков
        /// </summary>
        public int Block_Free_Property
        {
            get { return Block_Free; }
            set { Block_Free = value; }
        }
        
        private int Inode_Free;
        /// <summary>
        /// Количество свободных инодов
        /// </summary>
        public int Inode_Free_Property
        {
            get { return Inode_Free; }
            set { Inode_Free = value; }
        }
        
        private int Inode_Bitmap_Size;
        /// <summary>
        /// Размер области битовой карты инодов
        /// </summary>
        public int Inode_Bitmap_Size_Property
        {
            get { return Inode_Bitmap_Size; }
            set { Inode_Bitmap_Size = value; }
        }
        
        private int Bitmap_Size;
        /// <summary>
        /// Размер области битовой карты блоков
        /// </summary>
        public int Bitmap_Size_Property
        {
            get { return Bitmap_Size; }
            set { Bitmap_Size = value; }
        }


        //----------------------------------------Данные не хранящиеся в ФС-------------------------------------------------------


        private List<int> numID=new List<int>();
        /// <summary>
        /// Доступные ID для пользователей
        /// </summary>
        public List<int> numID_Property
        {
            get { return numID; }
            set { numID = value; }
        }

        private List<int> numGroupID = new List<int>();
        /// <summary>
        /// Доступные ID групп для пользователей
        /// </summary>
        public List<int> numGroupID_Property
        {
            get { return numGroupID; }
            set { numGroupID = value; }
        }

        //Размер ЖД
        private int HDD_Size;
        /// <summary>
        /// Размер жесткого диска
        /// </summary>
        public int HDD_Size_Property
        {
            get { return HDD_Size; }
            set { HDD_Size = value; }
        }
        
        private int Bitmap_Block_Size;
        /// <summary>
        /// Размер области битовой карты блоков в блоках
        /// </summary>
        public int Bitmap_Block_Size_Property
        {
            get { return Bitmap_Block_Size; }
           set { Bitmap_Block_Size = value; }
        }
        
        private int Inode_Bitmap_Block_Size;
        /// <summary>
        /// Размер области битовой карты инодов в блоках
        /// </summary>
        public int Inode_Bitmap_Block_Size_Property
        {
            get { return Inode_Bitmap_Block_Size; }
            set { Inode_Bitmap_Block_Size = value; }
        }
        
        private int Inode_Block_Size;
        /// <summary>
        /// Размер области инодов в блоках
        /// </summary>
        public int Inode_Block_Size_Property
        {
            get { return Inode_Block_Size; }
            set { Inode_Block_Size = value; }
        }
        
        private int User_Info_Block_Size;
        /// <summary>
        /// Размер области информации пользователей в блоках
        /// </summary>
        public int User_Info_Block_Size_Property
        {
            get { return User_Info_Block_Size; }
            set { User_Info_Block_Size = value; }
        }
        
        private int Record_Block_Size;
        /// <summary>
        /// Размер области записей корневого каталога в блоках
        /// </summary>
        public int Record_Block_Size_Property
        {
            get { return Record_Block_Size; }
            set { Record_Block_Size = value; }
        }
        
        private int Data_Block_Size;
        /// <summary>
        /// Размер области данных в блоках
        /// </summary>
        public int Data_Block_Size_Property
        {
            get { return Data_Block_Size; }
            set { Data_Block_Size = value; }
        }
        
        private int Service_Block_Size;
        /// <summary>
        /// Размер области служебной части в блоках
        /// </summary>
        public int Service_Block_Size_Property
        {
            get { return Service_Block_Size; }
            set { Service_Block_Size = value; }
        }
        
        private int Number_Users;
        /// <summary>
        /// Количество пользователей
        /// </summary>
        public int Number_Users_Property
        {
            get { return Number_Users; }
            set { Number_Users = value; }
        }
    }
}
