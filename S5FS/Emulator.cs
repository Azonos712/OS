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

        public void CreateMainFile(int hdd_size,ushort block_size)
        {
            FileStream fs = null;

            try
            {
                fs = File.Create(Path_Property);//Создание файла ФС по указаному пути

                //ОПРЕДЕЛЕНИЕ СУПЕРБЛОКА
                SuperBlock SB = new SuperBlock();//Создание объекта СуперБлок
                SB.HDD_Size_Property = hdd_size*1024*1024;//524288000 Размер жесткого диска

                SB.FS_Type_Property = "s5fs      ";//Название ФС
                SB.Block_Size_Property = block_size;//4096 Размер 1 блока(кластера)
                SB.FS_Size_Property = SB.HDD_Size_Property/SB.Block_Size_Property;//128 000 Размер ФС в блоках
                SB.Inode_Size_Property = 68*(SB.FS_Size_Property/2);//4 352 000 Размер области массива инодов
                SB.Block_Free_Property = SB.FS_Size_Property;//Количество свободных блоков
                SB.Inode_Free_Property = (SB.FS_Size_Property/2);//Количество свободных инодов
                SB.Inode_Bitmap_Size_Property = (SB.FS_Size_Property / 2) / 4;//Размер битовой карты инодов
                SB.Bitmap_Size_Property = (SB.FS_Size_Property / 4);//Размер битовой карты блоков
                /*var t = SB.GetType();
                int TotalSizeSB = 0;
                foreach (var prop in t.GetFields(System.Reflection.BindingFlags.Instance| System.Reflection.BindingFlags.NonPublic))
                {
                    TotalSizeSB += System.Runtime.InteropServices.Marshal.SizeOf(prop.GetValue(SB));
                }Подсчёт размера всех полей*/
                fs.Write(Encoding.ASCII.GetBytes(SB.FS_Type_Property), 0, Encoding.ASCII.GetBytes(SB.FS_Type_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Block_Size_Property), 0, BitConverter.GetBytes(SB.Block_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.FS_Size_Property), 0, BitConverter.GetBytes(SB.FS_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Block_Free_Property), 0, BitConverter.GetBytes(SB.Block_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Free_Property), 0, BitConverter.GetBytes(SB.Inode_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Bitmap_Size_Property).Length);
                for (int i = 0; i < SB.Block_Size_Property - 36; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ БЛОКОВ
                SB.Bitmap_Block_Size_Property=(SB.Bitmap_Size_Property / SB.Block_Size_Property)+1;
                for (int i = 0; i < (SB.Bitmap_Block_Size_Property*SB.Block_Size_Property); i++)
                    fs.WriteByte(1);

                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ ИНОДОВ
                SB.Inode_Bitmap_Block_Size_Property = (SB.Inode_Bitmap_Size_Property / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Inode_Bitmap_Block_Size_Property*SB.Block_Size_Property; i++)
                   fs.WriteByte(2);

                //ОПРЕДЕЛЕНИЕ МАССИВА ИНОДОВ
                SB.Inode_Block_Size_Property = (SB.Inode_Size_Property / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Inode_Block_Size_Property*SB.Block_Size_Property; i++)
                    fs.WriteByte(3);

                //ОПРЕДЕЛЕНИЕ ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ
                SB.User_Info_Block_Property = ((301 * 100) / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.User_Info_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(4);

                //ОПРЕДЕЛЕНИЕ ЗАПИСЕЙ КОРНЕВОГО КАТАЛОГА
                SB.Record_Block_Property = ((29 * ((SB.FS_Size_Property / 2)/2)) / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Record_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(5);

                //ОПРЕДЕЛЕНИЕ ДАННЫХ
                SB.Data_Block_Property = SB.FS_Size_Property - 1 - SB.Bitmap_Block_Size_Property -
                    SB.Inode_Bitmap_Block_Size_Property - SB.Inode_Block_Size_Property - SB.User_Info_Block_Property
                    - SB.Record_Block_Property;
                for (int i = 0; i < SB.Data_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(6);

                
                //Comands_fs comand = new Comands_fs();
                //comand.addInode(1, 0, 0, 777, 1, 0, 1, 2048, 0); //создаем корневой каталог
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

        public void ReadMainFile()
        {

        }
    }
}
