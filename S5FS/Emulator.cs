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
    static class Emulator
    {
        //Путь к файлу
        private static string Path;
        public static string Path_Property
        {
            get { return Path; }
            set { Path = value; }
        }

        //Объект файла
        private static FileStream FS;
        public static FileStream fs
        {
            get { return FS; }
            set { FS = value; }
        }

        //Объект суперблока
        private static SuperBlock sb = new SuperBlock();
        internal static SuperBlock SB { get => sb; set => sb = value; }

        //Текущий пользователь
        private static UserInfo CU;
        public static UserInfo CurrentUser
        {
            get { return CU; }
            set { CU = value; }
        }

        //Смещение текущей записи
        private static long CPR;
        public static long cpr
        {
            get { return CPR; }
            set { CPR = value; }
        }

        //Смещение текущего положения в корне
        private static long CP;
        public static long CurrentPosition
        {
            get { return CP; }
            set { CP = value; }
        }

        //Инод текущей записи
        //private long CIR;
        //public long cir
        //{
        //    get { return CIR; }
        //    set { CIR = value; }
        //}

        public static void CreateMainFile(int hdd_size, ushort block_size)
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

                //ЗАПИСЬ СУПЕРбЛОКА В ФС
                fs.Write(Encoding.ASCII.GetBytes(SB.FS_Type_Property), 0, Encoding.ASCII.GetBytes(SB.FS_Type_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.One_Block_Size_Property), 0, BitConverter.GetBytes(SB.One_Block_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.FS_Block_Size_Property), 0, BitConverter.GetBytes(SB.FS_Block_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Block_Free_Property), 0, BitConverter.GetBytes(SB.Block_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Free_Property), 0, BitConverter.GetBytes(SB.Inode_Free_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Inode_Bitmap_Size_Property).Length);
                fs.Write(BitConverter.GetBytes(SB.Bitmap_Size_Property), 0, BitConverter.GetBytes(SB.Bitmap_Size_Property).Length);

                //Заполняем оставшийся суперблок нулями
                for (int i = 0; i < SB.One_Block_Size_Property - 36; i++)
                    fs.WriteByte(0);

                //Определение служебных областей
                Designation();

                for (int i = 0; i < (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property); i++)
                    fs.WriteByte(0);
                
                for (int i = 0; i < SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);
                
                for (int i = 0; i < SB.Inode_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);
                
                for (int i = 0; i < SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);
                
                for (int i = 0; i < SB.Record_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);
                
                for (int i = 0; i < SB.Data_Block_Size_Property * SB.One_Block_Size_Property; i++)
                    fs.WriteByte(0);

                //РАЗМЕТКА БИТОВОЙ КАРТЫ БЛОКОВ
                CheckBitMap();

                //РАЗМЕТКА ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ(СОЗДАНИЕ ГЛАВНОГО ПОЛЬЗОВАТЕЛЯ)
                SB.Number_Users_Property = 0;
                UserInfo root = new UserInfo();
                root.ID_Property = (byte)SB.numID_Property.First();
                SB.numID_Property.Remove(root.ID_Property);
                root.Group_ID_Property = 0;
                if (SB.numGroupID_Property.Contains(root.Group_ID_Property) != true)
                {
                    SB.numGroupID_Property.Add(root.Group_ID_Property);
                }
                root.Login_Property = "root";
                root.Login_Property += (new string(' ', 12 - root.Login_Property.Length));
                root.Hash_Property = getHashSHA256("root");
                root.Homedir_Property = "RootDir\\";
                root.Homedir_Property += (new string(' ', 255 - root.Homedir_Property.Length));

                Inode inode = new Inode();
                inode.Access_Property = setAccess("0111000000011");
                inode.User_ID_Property = root.ID_Property;
                inode.Group_ID_Property = root.Group_ID_Property;
                inode.File_Size_Property = 0;
                inode.File_Create_Property = DateTime.Now;
                inode.File_Modif_Property = DateTime.Now;
                inode.Block_Count_Property = SB.Record_Block_Size_Property;

                for (int i = 0; i < inode.A_Block_Address_Property.Length; i++)
                {
                    inode.A_Block_Address_Property[i] = 0;
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


        public static void ReadMainFile()
        {
            fs = new FileStream(Path_Property, FileMode.Open);

            //Считываем суперблок
            SB = ReadSuperBlock();

            Designation();

            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property), SeekOrigin.Begin);

            //Определение ID пользователей и групп
            for (int i = 0; i < SB.Max_Number_Users_Property; i++)
            {
                UserInfo temp = ReadUser();
                if (temp.ID_Property == 0 && temp.Group_ID_Property == 0 && temp.Login_Property == (new string('\0', 12)) && temp.Hash_Property.SequenceEqual(new byte[32]) == true && temp.Homedir_Property == (new string('\0', 255)))
                {
                    continue;
                }
                SB.Number_Users_Property++;
                SB.numID_Property.Remove(temp.ID_Property);
                if (SB.numGroupID_Property.Contains(temp.Group_ID_Property) != true)
                {
                    SB.numGroupID_Property.Add(temp.Group_ID_Property);
                }
            }
            
        }

        public static void Designation()
        {
            //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ БЛОКОВ
            SB.Bitmap_Block_Size_Property = SB.Bitmap_Size_Property / SB.One_Block_Size_Property;// 
            if (SB.Bitmap_Size_Property % SB.One_Block_Size_Property != 0)
            {
                SB.Bitmap_Block_Size_Property++;
            }
            //ОПРЕДЕЛЕНИЕ БИТОВОЙ КАРТЫ ИНОДОВ
            SB.Inode_Bitmap_Block_Size_Property = SB.Inode_Bitmap_Size_Property / SB.One_Block_Size_Property;//
            if (SB.Inode_Bitmap_Size_Property % SB.One_Block_Size_Property != 0)
            {
                SB.Inode_Bitmap_Block_Size_Property++;
            }
            //ОПРЕДЕЛЕНИЕ МАССИВА ИНОДОВ
            SB.Inode_Block_Size_Property = SB.Inode_Size_Property / SB.One_Block_Size_Property;//
            if (SB.Inode_Size_Property % SB.One_Block_Size_Property != 0)
            {
                SB.Inode_Block_Size_Property++;
            }
            //Доступное количество записей в директории, в зависимости от размера кластера
            switch (SB.One_Block_Size_Property)
            {
                case 512:
                    SB.Available_Property = 137;
                    break;
                case 1024:
                    SB.Available_Property = 265;
                    break;
                case 2048:
                    SB.Available_Property = 521;
                    break;
                case 4096:
                    SB.Available_Property = 1033;
                    break;
            }
            //Определение максимального числа пользователей
            while (true)
            {
                if (SB.FS_Block_Size_Property / 2 > SB.Available_Property + SB.Max_Number_Users_Property * SB.Available_Property && SB.Max_Number_Users_Property < SB.Available_Property / 4)
                {
                    SB.Max_Number_Users_Property++;
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i < SB.Max_Number_Users_Property; i++)
                SB.numID_Property.Add(i);
            MessageBox.Show(SB.Max_Number_Users_Property.ToString());
            //ОПРЕДЕЛЕНИЕ ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ
            SB.User_Info_Block_Size_Property = (301 * SB.Max_Number_Users_Property) / SB.One_Block_Size_Property;//8
            if ((301 * SB.Max_Number_Users_Property) % SB.One_Block_Size_Property != 0)
            {
                SB.User_Info_Block_Size_Property++;
            }
            //ОПРЕДЕЛЕНИЕ ЗАПИСЕЙ КОРНЕВОГО КАТАЛОГА
            SB.Record_Block_Size_Property = (29 * (SB.Max_Number_Users_Property * SB.Available_Property)) / SB.One_Block_Size_Property;//2
            if ((29 * (SB.Max_Number_Users_Property * SB.Available_Property)) % SB.One_Block_Size_Property != 0)
            {
                SB.Record_Block_Size_Property++;
            }
            //РАСЧЁТ КОЛИЧЕСТВА БЛОКОВ СЛУЖЕБНЫХ ОБЛАСТЕЙ
            SB.Service_Block_Size_Property = 1 + SB.Bitmap_Block_Size_Property +//15
                SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property
                + SB.Record_Block_Size_Property;
            //ОПРЕДЕЛЕНИЕ ОБЛАСТИ ДАННЫХ
            SB.Data_Block_Size_Property = SB.FS_Block_Size_Property - SB.Service_Block_Size_Property;//241
        }

        public static SuperBlock ReadSuperBlock()
        {
            SuperBlock temp = new SuperBlock();
            byte[] sb1 = new byte[10];
            for (int i = 0; i < 10; i++)
                sb1[i] = (byte)fs.ReadByte();
            temp.FS_Type_Property = Encoding.ASCII.GetString(sb1);

            byte[] sb2 = new byte[2];
            for (int i = 0; i < 2; i++)
                sb2[i] = (byte)fs.ReadByte();
            temp.One_Block_Size_Property = BitConverter.ToUInt16(sb2, 0);

            byte[] sb3 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb3[i] = (byte)fs.ReadByte();
            temp.FS_Block_Size_Property = BitConverter.ToInt32(sb3, 0);

            byte[] sb4 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb4[i] = (byte)fs.ReadByte();
            temp.Inode_Size_Property = BitConverter.ToInt32(sb4, 0);

            byte[] sb5 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb5[i] = (byte)fs.ReadByte();
            temp.Block_Free_Property = BitConverter.ToInt32(sb5, 0);

            byte[] sb6 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb6[i] = (byte)fs.ReadByte();
            temp.Inode_Free_Property = BitConverter.ToInt32(sb6, 0);

            byte[] sb7 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb7[i] = (byte)fs.ReadByte();
            temp.Inode_Bitmap_Size_Property = BitConverter.ToInt32(sb7, 0);

            byte[] sb8 = new byte[4];
            for (int i = 0; i < 4; i++)
                sb6[i] = (byte)fs.ReadByte();
            temp.Bitmap_Size_Property = BitConverter.ToInt32(sb6, 0);

            return temp;
        }

        public static byte[] getHashSHA256(string text)
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

        public static ushort setAccess(string bin)
        {
            bin += new string('0', 16 - bin.Length);
            ushort nbin = Convert.ToUInt16(bin, 2);
            return nbin;
        }

        public static void AddUser(UserInfo u, Inode I)
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

            RootDirRecord r = new RootDirRecord();
            r.Name_Property = u.Homedir_Property.Replace(" ", string.Empty);
            r.Name_Property += (new string(' ', 20 - r.Name_Property.Length));
            r.Exstension_Property = ".dir ";
            r.Number_Inode_Property = I.Number_Property;
            CreateRecord(r, I);
        }

        public static UserInfo ReadUser()
        {
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

        public static RootDirRecord ReadRecord()
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

        public static int CheckUser(string login, string password)
        {
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property), SeekOrigin.Begin);
            int r = 0;
            for (int i = 0; i < SB.Max_Number_Users_Property; i++)
            {
                UserInfo temp = ReadUser();
                if (login.Replace(" ", string.Empty) == temp.Login_Property.Replace(" ", string.Empty))
                {
                    r++;
                    if (temp.Hash_Property.SequenceEqual(getHashSHA256(password)) == true)
                    {
                        r++;
                        CurrentUser = temp;
                    }
                    return r;
                }
            }

            return r;
        }

        public static void CheckBitMap()
        {
            int bitcount = 0;//отслеживаем текущее кол-во битов
            long CurPos = 0;//запоминаем следующую позицию записи в битовой карте блоков
            string bits = "";
            SB.Block_Free_Property = SB.FS_Block_Size_Property;
            for (int i = 0; i < SB.FS_Block_Size_Property; i++)
            {
                if (SB.Service_Block_Size_Property > i)
                {
                    bits += '1';
                    SB.Block_Free_Property--;
                }
                else
                {
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
            fs.Seek(20, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes(SB.Block_Free_Property), 0, BitConverter.GetBytes(SB.Block_Free_Property).Length);

        }

        public static void CheckBitMapInode()
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

            fs.Seek(24, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes(SB.Inode_Free_Property), 0, BitConverter.GetBytes(SB.Inode_Free_Property).Length);
        }

        public static int getFreeInode()
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
            return -1;
        }
        public static int getFreeBlock()
        {
            int B = 0;
            fs.Seek((1 * SB.One_Block_Size_Property), SeekOrigin.Begin);
            for (int i = 0; i < ((SB.FS_Block_Size_Property) / 8); i++)
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
                        return B;
                    }
                    B++;
                }
            }
            return -1;
        }
        public static void CreateRecord(RootDirRecord R, Inode I)
        {
            fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property), SeekOrigin.Begin);

            RootDirRecord temp = new RootDirRecord();
            for (int i = 0; i < (SB.Record_Block_Size_Property * SB.One_Block_Size_Property); i = (i + 29))
            {
                temp = ReadRecord();
                if (temp.Name_Property == (new string('\0', 20)) && temp.Exstension_Property == (new string('\0', 5)) && temp.Number_Inode_Property == 0)
                {
                    fs.Seek(-29, SeekOrigin.Current);
                    break;
                }
            }

            cpr = fs.Position;
            //cir = R.Number_Inode_Property;
            fs.Write(Encoding.ASCII.GetBytes(R.Name_Property), 0, Encoding.ASCII.GetBytes(R.Name_Property).Length);
            fs.Write(Encoding.ASCII.GetBytes(R.Exstension_Property), 0, Encoding.ASCII.GetBytes(R.Exstension_Property).Length);
            fs.Write(BitConverter.GetBytes(R.Number_Inode_Property), 0, BitConverter.GetBytes(R.Number_Inode_Property).Length);

            AddInode(I);
            CheckBitMap();
        }

        public static void AddInode(Inode I)
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

        public static void Bind(int cir)
        {
            int seektemp = 0;
            byte[] temp;
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property) + (cir * 68) + 28, SeekOrigin.Begin);
            for (int k=0;k<10;k++)
            {
                if (k == 9)
                {
                    temp = new byte[4];
                    for (int i = 0; i < 4; i++)
                        temp[i] = (byte)fs.ReadByte();
                    seektemp = BitConverter.ToInt32(temp, 0);
                    if (seektemp == 0)
                    {
                        long saveseek = fs.Position;
                        int freetemp = getFreeBlock();
                        fs.Seek(saveseek, SeekOrigin.Begin);
                        fs.Seek(-4, SeekOrigin.Current);
                        byte[] test = BitConverter.GetBytes(freetemp * SB.One_Block_Size_Property);
                        fs.Write(BitConverter.GetBytes(freetemp * SB.One_Block_Size_Property), 0, BitConverter.GetBytes(freetemp * SB.One_Block_Size_Property).Length);
                        fs.Seek(freetemp * SB.One_Block_Size_Property, SeekOrigin.Begin);
                    }
                    else
                    {
                        fs.Seek(seektemp, SeekOrigin.Begin);
                    }
                    for(int j = 0; j < SB.Available_Property; j++)
                    {
                        temp = new byte[4];
                        for (int i = 0; i < 4; i++)
                            temp[i] = (byte)fs.ReadByte();
                        seektemp = BitConverter.ToInt32(temp, 0);
                        if (seektemp == 0)
                        {
                            fs.Seek(-4, SeekOrigin.Current);
                            fs.Write(BitConverter.GetBytes(cpr), 0, BitConverter.GetBytes(cpr).Length);
                            break;
                        }

                    }
                    break;
                }

                temp = new byte[4];
                for (int i = 0; i < 4; i++)
                    temp[i] = (byte)fs.ReadByte();
                seektemp = BitConverter.ToInt32(temp, 0);
                if (seektemp == 0)
                {
                    fs.Seek(-4, SeekOrigin.Current);
                    fs.Write(BitConverter.GetBytes(cpr), 0, BitConverter.GetBytes(cpr).Length);
                    break;
                }
            }

            CheckBitMap();
        }

        public static void SetCurrentPosition()
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int a=0; a < (SB.Max_Number_Users_Property * SB.Available_Property); a++)
            {
                RootDirRecord temp = ReadRecord();
                if(temp.Name_Property.Replace(" ", string.Empty) == CurrentUser.Homedir_Property.Replace(" ", string.Empty) && temp.Exstension_Property == ".dir ")
                {
                    fs.Seek(-29, SeekOrigin.Current);
                    CurrentPosition = fs.Position;
                }
            }
        }
    }
}
