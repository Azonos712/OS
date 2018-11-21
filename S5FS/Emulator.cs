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
                SB.HDD_Size_Property = hdd_size * 1024 * 1024;//1 048 576 | Размер жесткого диска
                SB.FS_Type_Property = "s5fs      ";//Название ФС
                SB.One_Block_Size_Property = block_size;//4096 | Размер 1 блока(кластера)
                SB.FS_Block_Size_Property = SB.HDD_Size_Property / SB.One_Block_Size_Property;//256 | Размер ФС в блоках
                SB.Inode_Size_Property = 68 * (SB.FS_Block_Size_Property / 2);//8 704 | Размер области массива инодов
                SB.Block_Free_Property = SB.FS_Block_Size_Property;//256 | Количество свободных блоков
                SB.Inode_Free_Property = (SB.FS_Block_Size_Property / 2);//128 | Количество свободных инодов
                SB.Inode_Bitmap_Size_Property = (SB.FS_Block_Size_Property / 2) / 8;//16 | Размер битовой карты инодов
                SB.Bitmap_Size_Property = (SB.FS_Block_Size_Property / 8);//32 | Размер битовой карты блоков
                /*var t = SB.GetType();
                int TotalSizeSB = 0;
                foreach (var prop in t.GetFields(System.Reflection.BindingFlags.Instance| System.Reflection.BindingFlags.NonPublic))
                {
                    TotalSizeSB += System.Runtime.InteropServices.Marshal.SizeOf(prop.GetValue(SB));
                }Подсчёт размера всех полей*/

                //byte[] temp1 = BitConverter.GetBytes(SB.One_Block_Size_Property);
                //byte[] temp2 = BitConverter.GetBytes(16385);
                //byte[] temp3 = BitConverter.GetBytes(15);
                //(BitConverter.IsLittleEndian)
                
                fs.Write(Encoding.ASCII.GetBytes(SB.FS_Type_Property), 0, Encoding.ASCII.GetBytes(SB.FS_Type_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.One_Block_Size_Property), 0, BitConverter.GetBytes(SB.One_Block_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.FS_Block_Size_Property), 0, BitConverter.GetBytes(SB.FS_Block_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Block_Free_Property), 0, BitConverter.GetBytes(SB.Block_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Free_Property), 0, BitConverter.GetBytes(SB.Inode_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Bitmap_Size_Property).Length);

                for (int i = 0; i < SB.One_Block_Size_Property - 36; i++)//Заполняем оставшийся суперблок нулями
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ БЛОКОВ
                SB.Bitmap_Block_Size_Property = SB.Bitmap_Size_Property / SB.One_Block_Size_Property;// 1
                if (SB.Bitmap_Size_Property % SB.One_Block_Size_Property!=0)
                {
                    SB.Bitmap_Block_Size_Property++;
                }
                for (int i = 0; i < (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property); i++)
                    fs.WriteByte(0);


                //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ ИНОДОВ
                SB.Inode_Bitmap_Block_Size_Property = SB.Inode_Bitmap_Size_Property / SB.One_Block_Size_Property;//1
                if(SB.Inode_Bitmap_Size_Property % SB.One_Block_Size_Property != 0)
                {
                    SB.Inode_Bitmap_Block_Size_Property++;
                }
                for (int i = 0; i < SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ МАССИВА ИНОДОВ
                SB.Inode_Block_Size_Property = SB.Inode_Size_Property / SB.One_Block_Size_Property;//3
                if (SB.Inode_Size_Property % SB.One_Block_Size_Property != 0)
                {
                    SB.Inode_Block_Size_Property++;
                }
                for (int i = 0; i < SB.Inode_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ
                SB.User_Info_Block_Size_Property = (301 * 100) / SB.One_Block_Size_Property;//8
                if ((301 * 100) % SB.One_Block_Size_Property != 0)
                {
                    SB.User_Info_Block_Size_Property++;
                }
                for (int i = 0; i < SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ЗАПИСЕЙ КОРНЕВОГО КАТАЛОГА
                SB.Record_Block_Size_Property = (29 * ((SB.FS_Block_Size_Property / 2) / 2) + 100) / SB.One_Block_Size_Property;//1
                if (((29 * ((SB.FS_Block_Size_Property / 2) / 2) + 100)) % SB.One_Block_Size_Property != 0)
                {
                    SB.Record_Block_Size_Property++;
                }
                for (int i = 0; i < SB.Record_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //ОПРЕДЕЛЕНИЕ ДАННЫХ
                SB.Service_Block_Size_Property = 1 + SB.Bitmap_Block_Size_Property +//15
                    SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property
                    + SB.Record_Block_Size_Property;

                SB.Data_Block_Size_Property = SB.FS_Block_Size_Property - SB.Service_Block_Size_Property;//241
                for (int i = 0; i < SB.Data_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //РАЗМЕТКА БИТОВОЙ КАРТЫ БЛОКОВ
                CheckBitMap();
                /*fs.Seek((1 * SB.One_Block_Size_Property), SeekOrigin.Begin);//Перемещаемся на позицию бит.карты блоков
                int countblock = 0;
                for (int i = 0; i < SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property; i++)
                {
                    string bits = "";
                    for (int j = 0; j < 8; j++)
                    {
                        if (countblock < SB.Service_Block_Size_Property)
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

                Inode inode = new Inode();
                inode.Access_Property = setAccess("01110000000110");
                inode.User_ID_Property = root.ID_Property;
                inode.Group_ID_Property = root.Group_ID_Property;
                inode.File_Size_Property = 0;
                inode.File_Create_Property = DateTime.Now;
                inode.File_Modif_Property = DateTime.Now;
                inode.Block_Count_Property = SB.Data_Block_Size_Property;

                for (int i = 0; i < inode.A_Block_Address_Property.Length; i++)
                {
                    inode.A_Block_Address_Property[i]=0;
                }
                inode.Number_Property = getFreeInode();

                AddUser(root, inode);
                
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
            
            byte[] sb1 = new byte[10];
            for (int i = 0; i < 10; i++)
                sb1[i] = (byte)fs.ReadByte();
            SB.FS_Type_Property = Encoding.ASCII.GetString(sb1);

            byte[] sb2 = new byte[2];
            for (int i = 0; i < 2; i++)
                sb2[i] = (byte)fs.ReadByte();
            SB.One_Block_Size_Property = BitConverter.ToUInt16(sb2,0);

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

        public ushort setAccess(string bin)
        {
            bin += new string('0', 16 - bin.Length);
            ushort nbin = Convert.ToUInt16(bin, 2);
            return nbin;
        }

        public void AddUser(UserInfo u, Inode I)
        {
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property), SeekOrigin.Begin);

            UserInfo temp = new UserInfo();
            for (int i = 0; i < (SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property); i = (i + 301))
            {
                temp = ReadUser();
                if (temp.ID_Property == 0 && temp.Group_ID_Property == 0 && temp.Login_Property == (new string('\0', 12)) && temp.Hash_Property.SequenceEqual(new byte[32]) == true && temp.Homedir_Property == (new string('\0', 255)))
                {
                    fs.Seek(-301, SeekOrigin.Current);
                    break;
                }
            }

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

            string name = u.Homedir_Property.Replace(" ", string.Empty);
            name += (new string(' ', 20 - name.Length));
            CreateRecord(name, ".dir ", I);
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

        public RootDirRecord ReadRecord()
        {
            RootDirRecord temp = new RootDirRecord();
            byte[] r1 = new byte[20];
            for (int i = 0; i < 20; i++)
                r1[i] = (byte)fs.ReadByte();
            temp.Name_Property = Encoding.ASCII.GetString(r1);

            byte[] r2 = new byte[5];
            for (int i = 0; i < 5; i++)
                r2[i] = (byte)fs.ReadByte();
            temp.Exstension_Property = Encoding.ASCII.GetString(r2);

            byte[] r3 = new byte[4];
            for (int i = 0; i < 4; i++)
                r3[i] = (byte)fs.ReadByte();
            temp.Number_Inode_Property = BitConverter.ToInt32(r3, 0);

            return temp;
        }

        public int CheckUser(string login, string password)
        {
            //fs.Close();
            //fs = File.OpenRead(Path_Property);
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property), SeekOrigin.Begin);
            int r = 0;
            for (int i = 0; i < 100; i++)
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
            SB.Block_Free_Property = SB.FS_Block_Size_Property;
            for (int i = 0; i < SB.FS_Block_Size_Property; i++)
            {
                if (Busy100 > i)
                {
                    bits += '1';
                    SB.Block_Free_Property--;
                }
                else
                {
                    //fs.Close();
                    //fs = File.OpenRead(Path_Property);
                    fs.Seek(i * SB.One_Block_Size_Property, SeekOrigin.Begin);
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
                    fs.Seek((1 * SB.One_Block_Size_Property) + CurPos, SeekOrigin.Begin);
                    fs.WriteByte(Convert.ToByte(bits, 2));
                    CurPos++;
                    bits = "";
                }
            }

        }

        public void CheckBitMapInode()
        {
            int bitcount = 0;//отслеживаем текущее кол-во битов
            long CurPos = 0;//запоминаем следующую позицию записи в битовой карте блоков
            string bits = "";
            SB.Inode_Free_Property = SB.FS_Block_Size_Property / 2;
            for (int i = 0; i < (SB.FS_Block_Size_Property / 2); i++)
            {
                fs.Seek(((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                    + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)) + i * 68, SeekOrigin.Begin);
                byte one = (byte)fs.ReadByte();
                byte two = (byte)fs.ReadByte();
                byte three = (byte)fs.ReadByte();
                if (one != 0 || two != 0 || three != 0)
                {
                    bits += '1';
                    SB.Inode_Free_Property--;
                }
                else
                {
                    bits += '0';
                }


                bitcount++;

                if (bitcount == 8)
                {
                    bitcount = 0;
                    fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property) + CurPos, SeekOrigin.Begin);
                    fs.WriteByte(Convert.ToByte(bits, 2));
                    CurPos++;
                    bits = "";
                }
            }
        }

        public int getFreeInode()
        {
            int I = 0;
            fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property), SeekOrigin.Begin);
            for (int i = 0; i < ((SB.FS_Block_Size_Property / 2) / 8); i++)
            {
                byte one = (byte)fs.ReadByte();
                string bin = Convert.ToString(one, 2);
                if (bin.Length < 8)
                {
                    bin = new string('0', 8 - bin.Length) + bin;
                }
                for (int j = 0; j < 8; j++)
                {
                    if (bin[j] == '0')
                    {
                        return I;
                    }
                    I++;
                }
            }
            return 0;
        }
        public void CreateRecord(string homedir, string exstension, Inode I)
        {
            fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property), SeekOrigin.Begin);

            RootDirRecord temp = new RootDirRecord();
            for (int i = 0; i < (SB.Record_Block_Size_Property * SB.One_Block_Size_Property); i = (i + 29))
            {
                temp = ReadRecord();
                if (temp.Name_Property == (new string('\0', 20)) && temp.Exstension_Property == (new string('\0', 5)) && temp.Number_Inode_Property==0)
                {
                    fs.Seek(-29, SeekOrigin.Current);
                    break;
                }
            }

            fs.Write(Encoding.ASCII.GetBytes(homedir), 0, Encoding.ASCII.GetBytes(homedir).Length);
            fs.Write(Encoding.ASCII.GetBytes(exstension), 0, Encoding.ASCII.GetBytes(exstension).Length);
            fs.Write(BitConverter.GetBytes(I.Number_Property), 0, BitConverter.GetBytes(I.Number_Property).Length);

            AddInode(I);
        }

        public void AddInode(Inode I)
        {
            fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (I.Number_Property * 68), SeekOrigin.Begin);


            fs.Write(BitConverter.GetBytes(I.Access_Property), 0, BitConverter.GetBytes(I.Access_Property).Length);
            byte[] b = new byte[1];
            b[0] = I.User_ID_Property;
            fs.Write(b, 0, b.Length);
            b[0] = I.Group_ID_Property;
            fs.Write(b, 0, b.Length);
            fs.Write(BitConverter.GetBytes(I.File_Size_Property), 0, BitConverter.GetBytes(I.File_Size_Property).Length);
            //DateTime t = new DateTime(Ticks);
            fs.Write(BitConverter.GetBytes(I.File_Create_Property.Ticks), 0, BitConverter.GetBytes(I.File_Create_Property.Ticks).Length);
            fs.Write(BitConverter.GetBytes(I.File_Modif_Property.Ticks), 0, BitConverter.GetBytes(I.File_Modif_Property.Ticks).Length);
            fs.Write(BitConverter.GetBytes(I.Block_Count_Property), 0, BitConverter.GetBytes(I.Block_Count_Property).Length);
            for (int i = 0; i < I.A_Block_Address_Property.Length; i++)
            {
                fs.Write(BitConverter.GetBytes(I.A_Block_Address_Property[i]), 0, BitConverter.GetBytes(I.A_Block_Address_Property[i]).Length);
            }

            CheckBitMapInode();
        }
    }
}
