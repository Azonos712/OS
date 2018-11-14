using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;

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

        private FileStream fs;
        //private FileStream fs=File.Create(Path_Property);
        public FileStream fs_Property
        {
            get { return fs; }
            set { fs = value; }
        }
        
        private SuperBlock sb = new SuperBlock();//Создание объекта СуперБлок
        internal SuperBlock SB { get => sb; set => sb = value; }

        public void CreateMainFile(int hdd_size, ushort block_size)
        {
            fs = null;

            try
            {
                //fs = File.Open(Path_Property, FileMode.Open, FileShare.None);
                fs = File.Create(Path_Property);//Создание файла ФС по указаному пути

                //ОПРЕДЕЛЕНИЕ СУПЕРБЛОКА
                SB.HDD_Size_Property = hdd_size * 1024 * 1024;//524288000 Размер жесткого диска

                SB.FS_Type_Property = "s5fs      ";//Название ФС
                SB.Block_Size_Property = block_size;//4096 Размер 1 блока(кластера)
                SB.FS_Size_Property = SB.HDD_Size_Property / SB.Block_Size_Property;//128 000 Размер ФС в блоках
                SB.Inode_Size_Property = 68 * (SB.FS_Size_Property / 2);//4 352 000 Размер области массива инодов
                SB.Block_Free_Property = SB.FS_Size_Property;//Количество свободных блоков
                SB.Inode_Free_Property = (SB.FS_Size_Property / 2);//Количество свободных инодов
                SB.Inode_Bitmap_Size_Property = (SB.FS_Size_Property / 2) / 8;//Размер битовой карты инодов
                SB.Bitmap_Size_Property = (SB.FS_Size_Property / 8);//Размер битовой карты блоков
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

                for (int i = 0; i < SB.Block_Size_Property - 36; i++)//Заполняем оставшийся суперблок нулями
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ БЛОКОВ
                SB.Bitmap_Block_Size_Property = (SB.Bitmap_Size_Property / SB.Block_Size_Property) + 1;
                for (int i = 0; i < (SB.Bitmap_Block_Size_Property * SB.Block_Size_Property); i++)
                    fs.WriteByte(0);


                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ ИНОДОВ
                SB.Inode_Bitmap_Block_Size_Property = (SB.Inode_Bitmap_Size_Property / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Inode_Bitmap_Block_Size_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ МАССИВА ИНОДОВ
                SB.Inode_Block_Size_Property = (SB.Inode_Size_Property / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Inode_Block_Size_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ
                SB.User_Info_Block_Property = ((301 * 100) / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.User_Info_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ЗАПИСЕЙ КОРНЕВОГО КАТАЛОГА
                SB.Record_Block_Property = ((29 * ((SB.FS_Size_Property / 2) / 2)) / SB.Block_Size_Property) + 1;
                for (int i = 0; i < SB.Record_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ДАННЫХ
                SB.Data_Block_Property = SB.FS_Size_Property - 1 - SB.Bitmap_Block_Size_Property -
                    SB.Inode_Bitmap_Block_Size_Property - SB.Inode_Block_Size_Property - SB.User_Info_Block_Property
                    - SB.Record_Block_Property;
                for (int i = 0; i < SB.Data_Block_Property * SB.Block_Size_Property; i++)
                    fs.WriteByte(0);

                //РАЗМЕТКА БИТОВОЙ КАРТЫ БЛОКОВ
                SB.Service_Block_Property = 1 + SB.Bitmap_Block_Size_Property +
                    SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Property
                    + SB.Record_Block_Property;
                CheckBitMap();
                /*fs.Seek((1 * SB.Block_Size_Property), SeekOrigin.Begin);//Перемещаемся на позицию бит.карты блоков
                int countblock = 0;
                for (int i = 0; i < SB.Bitmap_Block_Size_Property * SB.Block_Size_Property; i++)
                {
                    string bits = "";
                    for (int j = 0; j < 8; j++)
                    {
                        if (countblock < SB.Service_Block_Property)
                        {
                            bits += '1';
                            SB.Block_Free_Property--;
                        }
                        else
                        {
                            bits += '0';
                        }
                        countblock++;
                    }
                    fs.WriteByte(Convert.ToByte(bits, 2));
                }*/

                //РАЗМЕТКА ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ(СОЗДАНИЕ ГЛАВНОГО ПОЛЬЗОВАТЕЛЯ)
                SB.Number_Users_Property = 0;
                UserInfo root = new UserInfo();
                root.ID_Property = (byte)SB.numID_Property.First();
                SB.numID_Property.Remove(root.ID_Property);
                root.Group_ID_Property = 0;
                root.Login_Property = "root";
                root.Login_Property += (new string(' ', 12 - root.Login_Property.Length));
                root.Hash_Property = getHashSHA256("root");
                root.Homedir_Property = "RootDir\\";
                root.Homedir_Property += (new string(' ', 255 - root.Homedir_Property.Length));
                
                AddUser(root);
                
                //Comands_fs comand = new Comands_fs();
                //comand.addInode(1, 0, 0, 777, 1, 0, 1, 2048, 0); //создаем корневой каталог
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!");
            }
            finally
            {
                //if (fs != null)
                    //fs.Close();
            }

        }

        public void ReadMainFile()
        {
            FileStream fs = File.OpenRead(Path_Property);

        }

        public byte[] getHashSHA256(string text)
        {
            byte[] B = Encoding.ASCII.GetBytes(text);
            SHA256Managed H = new SHA256Managed();
            byte[] Hb = H.ComputeHash(B);
            //string SHb = string.Empty;
            //foreach (byte x in Hb)
            //{
            //    SHb += String.Format("{0:x}", x);
            //}
            return Hb;
        }

        public void AddUser(UserInfo u)
        {
            //fs.Close();
            //fs = File.OpenRead(Path_Property);
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.Block_Size_Property), SeekOrigin.Begin);

            UserInfo temp = new UserInfo();
            for (int i = 0; i < (SB.User_Info_Block_Property * SB.Block_Size_Property); i = (i + 301))
            {
                temp = ReadUser();
                if (temp.ID_Property == 0 && temp.Group_ID_Property == 0 && temp.Login_Property == (new string('\0', 12)) && temp.Hash_Property.SequenceEqual(new byte[32])== true && temp.Homedir_Property == (new string('\0', 255)))
                {
                    fs.Seek(-301, SeekOrigin.Current);
                    break;
                }
            }

            //fs.Close();
            //fs = File.OpenWrite(Path_Property);

            byte[] b = new byte[1];
            b[0] = u.ID_Property;
            fs.Write(b, 0, b.Length);
            b[0] = u.Group_ID_Property;
            fs.Write(b, 0, b.Length);
            //fs.Write(BitConverter.GetBytes(u.ID_Property), 0, BitConverter.GetBytes(u.ID_Property).Length);
            //fs.Write(BitConverter.GetBytes(u.Group_ID_Property), 0, BitConverter.GetBytes(u.Group_ID_Property).Length);
            fs.Write(Encoding.ASCII.GetBytes(u.Login_Property), 0, Encoding.ASCII.GetBytes(u.Login_Property).Length);
            fs.Write(u.Hash_Property, 0, u.Hash_Property.Length);
            fs.Write(Encoding.ASCII.GetBytes(u.Homedir_Property), 0, Encoding.ASCII.GetBytes(u.Homedir_Property).Length);
            SB.Number_Users_Property++;
            CheckBitMap();
        }

        public UserInfo ReadUser()
        {
            //fs.Close();
            //fs = File.OpenRead(Path_Property);
            UserInfo temp = new UserInfo();
            byte[] u1 = new byte[1];
            for (int i = 0; i < 1; i++)
                u1[i] = (byte)fs.ReadByte();
            temp.ID_Property = u1[0];

            byte[] u2 = new byte[1];
            for (int i = 0; i < 1; i++)
                u2[i] = (byte)fs.ReadByte();
            temp.Group_ID_Property = u2[0];

            byte[] u3 = new byte[12];
            for (int i = 0; i < 12; i++)
                u3[i] = (byte)fs.ReadByte();
            temp.Login_Property = Encoding.ASCII.GetString(u3);

            byte[] u4 = new byte[32];
            for (int i = 0; i < 32; i++)
                u4[i] = (byte)fs.ReadByte();
            temp.Hash_Property = u4;

            byte[] u5 = new byte[255];
            for (int i = 0; i < 255; i++)
                u5[i] = (byte)fs.ReadByte();
            temp.Homedir_Property = Encoding.ASCII.GetString(u5);

            return temp;
        }

        public int CheckUser(string login, string password)
        {
            //fs.Close();
            //fs = File.OpenRead(Path_Property);
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.Block_Size_Property), SeekOrigin.Begin);
            int r = 0;
            for (int i =0;i< 100; i++)
            {
                UserInfo temp = ReadUser();
                if (login.Replace(" ", string.Empty) == temp.Login_Property.Replace(" ", string.Empty))
                {
                    r++;
                    if (temp.Hash_Property.SequenceEqual(getHashSHA256(password)) == true)
                    {
                        r++;
                    }
                    return r;
                }
            }

            return r;
        }

        public void CheckBitMap()
        {
            int Busy100 = 1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property;//количество блоков занятых изначально
            int bitcount = 0;//отслеживаем текущее кол-во битов
            long CurPos = 0;//запоминаем следующую позицию записи в битовой карте блоков
            string bits = "";
            SB.Block_Free_Property=SB.FS_Size_Property;
            for (int i=0;i<SB.FS_Size_Property; i++)
            {
                if(Busy100>i)
                {
                    bits += '1';
                    SB.Block_Free_Property--;
                }
                else
                {
                    //fs.Close();
                    //fs = File.OpenRead(Path_Property);
                    fs.Seek(i * SB.Block_Size_Property, SeekOrigin.Begin);
                    byte one = (byte)fs.ReadByte();
                    byte two = (byte)fs.ReadByte();
                    byte three = (byte)fs.ReadByte();
                    if (one != 0 || two != 0 || three != 0)
                    {
                        bits += '1';
                        SB.Block_Free_Property--;
                    }
                    else
                    {
                        bits += '0';
                    }
                }

                bitcount++;

                if (bitcount == 8)
                {
                    //fs.Close();
                    //fs = File.OpenWrite(Path_Property);
                    bitcount = 0;
                    fs.Seek((1 * SB.Block_Size_Property)+CurPos, SeekOrigin.Begin);
                    fs.WriteByte(Convert.ToByte(bits, 2));
                    CurPos++;
                    bits = "";
                }
            }

        }

        public void CreateRecord()
        {
            AddInode();
        }

        public void AddInode()
        {

        }
    }
}
