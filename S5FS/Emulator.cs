using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace S5FS
{
    //Stopwatch W = new Stopwatch();
    //W.Start();
    //F
    //W.Stop();
    //double tes = (double)W.ElapsedMilliseconds / 1000;
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
        private static int CPR;
        public static int cpr
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

        //Объект записи текущего положения в корне
        private static RootDirRecord RCP;
        public static RootDirRecord RecCurPos
        {
            get { return RCP; }
            set { RCP = value; }
        }

        //Объект инода текущей запииси в корне
        private static Inode ICR;
        public static Inode InoCurRec
        {
            get { return ICR; }
            set { ICR = value; }
        }

        public static List<string> Dirs = new List<string>();
        public static List<string> Files = new List<string>();
        public static List<string> HideFiles = new List<string>();
        public static List<string> SysFiles = new List<string>();
        public static List<string> HideSysFiles = new List<string>();


        public static void CreateMainFile(int hdd_size, ushort block_size)
        {
            fs = null;
            try
            {
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

                fs.Write(new byte[SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property);

                fs.Write(new byte[SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property);

                fs.Write(new byte[SB.Inode_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.Inode_Block_Size_Property * SB.One_Block_Size_Property);

                fs.Write(new byte[SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property);

                fs.Write(new byte[SB.Record_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.Record_Block_Size_Property * SB.One_Block_Size_Property);

                fs.Write(new byte[SB.Data_Block_Size_Property * SB.One_Block_Size_Property], 0, SB.Data_Block_Size_Property * SB.One_Block_Size_Property);

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
                inode.Access_Property = setAccess("0111111111000");
                inode.User_ID_Property = root.ID_Property;
                inode.Group_ID_Property = root.Group_ID_Property;
                inode.File_Size_Property = 0;
                inode.File_Create_Property = DateTime.Now;
                inode.File_Modif_Property = DateTime.Now;
                inode.Block_Count_Property = SB.Record_Block_Size_Property;

                for (int i = 0; i < inode.Array_Of_Address_Property.Length; i++)
                {
                    inode.Array_Of_Address_Property[i] = 0;
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
            //MessageBox.Show(SB.Max_Number_Users_Property.ToString());
            //ОПРЕДЕЛЕНИЕ ИНФОРМАЦИИ ПОЛЬЗОВАТЕЛЕЙ
            SB.User_Info_Block_Size_Property = (301 * SB.Max_Number_Users_Property) / SB.One_Block_Size_Property;//
            if ((301 * SB.Max_Number_Users_Property) % SB.One_Block_Size_Property != 0)
            {
                SB.User_Info_Block_Size_Property++;
            }
            //ОПРЕДЕЛЕНИЕ ЗАПИСЕЙ КОРНЕВОГО КАТАЛОГА
            SB.Record_Block_Size_Property = (29 * (SB.Max_Number_Users_Property * SB.Available_Property)) / SB.One_Block_Size_Property;//
            if ((29 * (SB.Max_Number_Users_Property * SB.Available_Property)) % SB.One_Block_Size_Property != 0)
            {
                SB.Record_Block_Size_Property++;
            }
            //РАСЧЁТ КОЛИЧЕСТВА БЛОКОВ СЛУЖЕБНЫХ ОБЛАСТЕЙ
            SB.Service_Block_Size_Property = 1 + SB.Bitmap_Block_Size_Property +//
                SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property
                + SB.Record_Block_Size_Property;
            //ОПРЕДЕЛЕНИЕ ОБЛАСТИ ДАННЫХ
            SB.Data_Block_Size_Property = SB.FS_Block_Size_Property - SB.Service_Block_Size_Property;//
        }

        public static SuperBlock ReadSuperBlock()
        {
            SuperBlock temp = new SuperBlock();

            byte[] sb1 = new byte[10];
            fs.Read(sb1, 0, sb1.Length);
            temp.FS_Type_Property = Encoding.ASCII.GetString(sb1);

            byte[] sb2 = new byte[2];
            fs.Read(sb2, 0, sb2.Length);
            temp.One_Block_Size_Property = BitConverter.ToUInt16(sb2, 0);

            byte[] sb3 = new byte[4];
            fs.Read(sb3, 0, sb3.Length);
            temp.FS_Block_Size_Property = BitConverter.ToInt32(sb3, 0);

            byte[] sb4 = new byte[4];
            fs.Read(sb4, 0, sb4.Length);
            temp.Inode_Size_Property = BitConverter.ToInt32(sb4, 0);

            byte[] sb5 = new byte[4];
            fs.Read(sb5, 0, sb5.Length);
            temp.Block_Free_Property = BitConverter.ToInt32(sb5, 0);

            byte[] sb6 = new byte[4];
            fs.Read(sb6, 0, sb6.Length);
            temp.Inode_Free_Property = BitConverter.ToInt32(sb6, 0);

            byte[] sb7 = new byte[4];
            fs.Read(sb7, 0, sb7.Length);
            temp.Inode_Bitmap_Size_Property = BitConverter.ToInt32(sb7, 0);

            byte[] sb8 = new byte[4];
            fs.Read(sb8, 0, sb8.Length);
            temp.Bitmap_Size_Property = BitConverter.ToInt32(sb8, 0);

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

        public static string getAccess(ushort nbin)
        {
            string bin = Convert.ToString(nbin, 2);
            bin = new string('0', 16 - bin.Length) + bin;
            return bin;
        }


        public static void AddUser(UserInfo u, Inode I)
        {
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property//задаём смещение к сектору пользователей
                + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property), SeekOrigin.Begin);

            UserInfo temp = new UserInfo();
            while (true)
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
            r.Name_Property = u.Login_Property.Replace(" ", string.Empty);
            r.Name_Property += (new string(' ', 20 - r.Name_Property.Length));
            r.Exstension_Property = ".dir ";
            r.Number_Inode_Property = I.Number_Property;
            CreateRecord(r, I);
        }

        public static Inode ReadInode()
        {
            Inode temp = new Inode();

            byte[] i1 = new byte[2];
            fs.Read(i1, 0, i1.Length);
            temp.Access_Property = BitConverter.ToUInt16(i1, 0);

            byte[] i2 = new byte[1];
            fs.Read(i2, 0, i2.Length);
            temp.User_ID_Property = i2[0];

            byte[] i3 = new byte[1];
            fs.Read(i3, 0, i3.Length);
            temp.Group_ID_Property = i3[0];

            byte[] i4 = new byte[4];
            fs.Read(i4, 0, i4.Length);
            temp.File_Size_Property = BitConverter.ToInt32(i4, 0);

            byte[] i5 = new byte[8];
            fs.Read(i5, 0, i5.Length);
            temp.File_Create_Property = new DateTime(BitConverter.ToInt64(i5, 0));

            byte[] i6 = new byte[8];
            fs.Read(i6, 0, i6.Length);
            temp.File_Modif_Property = new DateTime(BitConverter.ToInt64(i6, 0));

            byte[] i7 = new byte[4];
            fs.Read(i7, 0, i7.Length);
            temp.Block_Count_Property = BitConverter.ToInt32(i7, 0);

            for (int k = 0; k < 10; k++)
            {
                byte[] i8 = new byte[4];
                fs.Read(i8, 0, i8.Length);
                temp.Array_Of_Address_Property[k] = BitConverter.ToInt32(i8, 0);
            }

            return temp;
        }

        public static UserInfo ReadUser()
        {
            UserInfo temp = new UserInfo();

            byte[] u1 = new byte[1];
            fs.Read(u1, 0, u1.Length);
            temp.ID_Property = u1[0];

            byte[] u2 = new byte[1];
            fs.Read(u2, 0, u2.Length);
            temp.Group_ID_Property = u2[0];

            byte[] u3 = new byte[12];
            fs.Read(u3, 0, u3.Length);
            temp.Login_Property = Encoding.ASCII.GetString(u3);

            byte[] u4 = new byte[32];
            fs.Read(u4, 0, u4.Length);
            temp.Hash_Property = u4;

            byte[] u5 = new byte[255];
            fs.Read(u5, 0, u5.Length);
            temp.Homedir_Property = Encoding.ASCII.GetString(u5);

            return temp;
        }

        public static RootDirRecord ReadRecord()
        {
            RootDirRecord temp = new RootDirRecord();
            byte[] r1 = new byte[20];
            fs.Read(r1, 0, r1.Length);
            temp.Name_Property = Encoding.ASCII.GetString(r1);

            byte[] r2 = new byte[5];
            fs.Read(r2, 0, r2.Length);
            temp.Exstension_Property = Encoding.ASCII.GetString(r2);

            byte[] r3 = new byte[4];
            fs.Read(r3, 0, r3.Length);
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

        public static void SetValueBitmap(int Number, int Value, int Offset)
        {
            fs.Seek(Offset, SeekOrigin.Begin);
            int NumOfByte = Number / 8;
            fs.Seek(NumOfByte, SeekOrigin.Current);

            byte one = (byte)fs.ReadByte();
            string bin = Convert.ToString(one, 2);
            if (bin.Length < 8)
            {
                bin = new string('0', 8 - bin.Length) + bin;
            }
            Number = Number - 8 * NumOfByte;
            bin = bin.Remove(Number, 1).Insert(Number, Value.ToString());

            fs.Seek(-1, SeekOrigin.Current);
            fs.WriteByte(Convert.ToByte(bin, 2));
        }

        public static void CheckBitMap()
        {
            int bitcount = 0;//отслеживаем текущее кол-во битов
            long CurPos = 0;//запоминаем следующую позицию записи в битовой карте блоков
            string bits = "";
            byte[] buf;
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
                    bool busy = false;

                    buf = new byte[SB.One_Block_Size_Property];
                    fs.Read(buf, 0, SB.One_Block_Size_Property);
                    if (buf.SequenceEqual(new byte[SB.One_Block_Size_Property]) == false)
                    {
                        busy = true;
                    }

                    if (busy == true)
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
            byte[] buf;
            SB.Inode_Free_Property = SB.FS_Block_Size_Property / 2;
            for (int i = 0; i < (SB.FS_Block_Size_Property / 2); i++)
            {
                fs.Seek(((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                    + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)) + i * 68, SeekOrigin.Begin);
                bool busy = false;

                buf = new byte[68];
                fs.Read(buf, 0, 68);
                if (buf.SequenceEqual(new byte[68]) == false)
                {
                    busy = true;
                }

                if (busy == true)
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

            cpr = (int)fs.Position;
            //cir = R.Number_Inode_Property;
            fs.Write(Encoding.ASCII.GetBytes(R.Name_Property), 0, Encoding.ASCII.GetBytes(R.Name_Property).Length);
            fs.Write(Encoding.ASCII.GetBytes(R.Exstension_Property), 0, Encoding.ASCII.GetBytes(R.Exstension_Property).Length);
            fs.Write(BitConverter.GetBytes(R.Number_Inode_Property), 0, BitConverter.GetBytes(R.Number_Inode_Property).Length);

            AddInode(I);
            //CheckBitMap();
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
            for (int i = 0; i < I.Array_Of_Address_Property.Length; i++)
            {
                fs.Write(BitConverter.GetBytes(I.Array_Of_Address_Property[i]), 0, BitConverter.GetBytes(I.Array_Of_Address_Property[i]).Length);
            }

            SetValueBitmap(I.Number_Property, 1, (1 + SB.Bitmap_Block_Size_Property) * SB.One_Block_Size_Property);
            //CheckBitMapInode();
        }

        public static void Bind(int cir)
        {
            int seektemp = 0;
            byte[] temp;
            //Переходим к иноду, к которому идёт привязка
            fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property) + (cir * 68), SeekOrigin.Begin);
            //Считываем его для удобства
            Inode I = ReadInode();

            for (int k = 0; k < 10; k++)
            {

                if (k == 9)
                {
                    if (I.Array_Of_Address_Property[k] == 0)
                    {
                        int freetemp = getFreeBlock();
                        SetValueBitmap(freetemp, 1, 1 * SB.One_Block_Size_Property);
                        fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property) + (cir * 68) + 28 + (4 * k), SeekOrigin.Begin);
                        fs.Write(BitConverter.GetBytes(freetemp * SB.One_Block_Size_Property), 0, BitConverter.GetBytes(freetemp * SB.One_Block_Size_Property).Length);
                        fs.Seek(freetemp * SB.One_Block_Size_Property, SeekOrigin.Begin);
                    }
                    else
                    {
                        fs.Seek(I.Array_Of_Address_Property[k], SeekOrigin.Begin);
                    }

                    for (int j = 0; j < SB.One_Block_Size_Property / 4; j++)
                    {
                        temp = new byte[4];
                        fs.Read(temp, 0, temp.Length);
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

                if (I.Array_Of_Address_Property[k] == 0)
                {
                    fs.Seek(((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property) + (cir * 68) + 28 + (4 * k), SeekOrigin.Begin);
                    fs.Write(BitConverter.GetBytes(cpr), 0, BitConverter.GetBytes(cpr).Length);
                    break;
                }

            }
            //CheckBitMap();
        }

        public static void SetCurrentPosition(string NameOfObj)
        {
            //переходим в область записей корневого каталога
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int a = 0; a < (SB.Max_Number_Users_Property * SB.Available_Property); a++)
            {
                RootDirRecord temp = ReadRecord();
                if (temp.Name_Property.Replace(" ", string.Empty) == NameOfObj && temp.Exstension_Property == ".dir ")
                {
                    RecCurPos = temp;
                    fs.Seek(-29, SeekOrigin.Current);
                    CurrentPosition = fs.Position;
                    fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * RecCurPos.Number_Inode_Property, SeekOrigin.Begin);
                    InoCurRec = ReadInode();
                }
            }
        }

        public static void DeleteUser(string login, string id)
        {
            //переходим к области пользователей
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int y = 0; y < SB.Max_Number_Users_Property; y++)
            {
                UserInfo tempU = ReadUser();
                if (tempU.Login_Property.Replace(" ", string.Empty) == login && tempU.ID_Property == Convert.ToInt32(id))
                {
                    SB.numID_Property.Add(tempU.ID_Property);//Добавляем освободившийся ID
                    SB.numID_Property.Sort();//сортируем доступные ID
                    fs.Seek(-301, SeekOrigin.Current);//удаляем этого пользователя
                    byte[] tempArr = new byte[301];
                    fs.Write(tempArr, 0, tempArr.Length);
                    //Удаляем все записи и папку связанные с этим пользователем(начинаем с root директории)
                    DeleteAllInDir((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property, tempU, false);

                    break;
                }
            }
        }

        public static void DeleteAllInDir(long S, UserInfo U, bool ALL)
        {
            //Переходим в область хранения инодов,к нужной папке
            fs.Seek(S, SeekOrigin.Begin);
            Inode MainInode = ReadInode();

            for (int i = 0; i < 10; i++)
            {
                if (MainInode.Array_Of_Address_Property[i] != 0)
                {
                    if (i == 9)
                    {
                        //fs.Seek(MainInode.Array_Of_Address_Property[i], SeekOrigin.Begin);
                        for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                        {
                            fs.Seek(MainInode.Array_Of_Address_Property[i] + 4 * g, SeekOrigin.Begin);
                            byte[] tempArray = new byte[4];
                            fs.Read(tempArray, 0, tempArray.Length);
                            int tempSeek = BitConverter.ToInt32(tempArray, 0);

                            //Удаление записи,инода,привязки
                            if (tempSeek != 0)
                            {
                                DeleteRIB(MainInode.Array_Of_Address_Property[i], tempSeek, U, g, ALL);
                                //fs.Seek(CurPos, SeekOrigin.Begin);
                            }
                        }
                        break;
                    }
                    //Удаление записи,инода,привязки
                    DeleteRIB(S + 28, MainInode.Array_Of_Address_Property[i], U, i, ALL);

                }
            }
            //CheckBitMapInode();
            //CheckBitMap();
        }

        public static void DeleteRIB(long mainfolder, int current, UserInfo user, int number, bool ALL)
        {
            fs.Seek(current, SeekOrigin.Begin);
            RootDirRecord tempR = ReadRecord();
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
            Inode tempI = ReadInode();
            if (tempI.User_ID_Property == user.ID_Property || ALL == true /*|| (user.Group_ID_Property == 0 && tempI.User_ID_Property == user.ID_Property)*/)
            {
                string tempAccess = getAccess(tempI.Access_Property);
                if (Char.GetNumericValue(tempAccess[0]) == 0)
                {
                    //если папка,запускаем рекурсию для удаления файлов внутри папки
                    DeleteAllInDir((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, user, true);
                    //FullCleanDir();
                }
                else
                {
                    //если файл,то удаляем даные по привязкам
                    for (int f = 0; f < 10; f++)
                    {
                        if (tempI.Array_Of_Address_Property[f] != 0)
                        {
                            //fs.Seek(tempI.Array_Of_Address_Property[f], SeekOrigin.Begin);

                            if (f == 9)
                            {
                                for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                                {
                                    fs.Seek(tempI.Array_Of_Address_Property[f] + 4 * g, SeekOrigin.Begin);
                                    byte[] tempArray = new byte[4];
                                    fs.Read(tempArray, 0, tempArray.Length);
                                    int tempSeek = BitConverter.ToInt32(tempArray, 0);

                                    if (tempSeek != 0)
                                    {
                                        SetValueBitmap(tempSeek / SB.One_Block_Size_Property, 0, 1 * SB.One_Block_Size_Property);
                                        //fs.Seek(tempSeek, SeekOrigin.Begin);
                                        //for (int k = 0; k < SB.One_Block_Size_Property; k++)
                                        //{
                                        //    fs.WriteByte(0);
                                        //}
                                    }
                                }
                                break;
                            }

                            SetValueBitmap(tempI.Array_Of_Address_Property[f] / SB.One_Block_Size_Property, 0, 1 * SB.One_Block_Size_Property);
                            //for (int k = 0; k < SB.One_Block_Size_Property; k++)
                            //{
                            //    fs.WriteByte(0);
                            //}
                        }
                    }
                }

                //производим удаление


                //сначала инод
                SetValueBitmap(tempR.Number_Inode_Property, 0, (1 + SB.Bitmap_Block_Size_Property) * SB.One_Block_Size_Property);
                fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
                byte[] t1 = new byte[68];
                fs.Write(t1, 0, t1.Length);
                //Затем сама запись
                fs.Seek(current, SeekOrigin.Begin);
                byte[] t2 = new byte[29];
                fs.Write(t2, 0, t2.Length);
                //И наконец адрес в массиве привязок
                fs.Seek(mainfolder + 4 * number, SeekOrigin.Begin);
                byte[] t3 = new byte[4];
                fs.Write(t3, 0, t3.Length);
            }
        }

        //FullCleanDir();

        public static int[] FindBind(long SeekWhere, string name)
        {
            int[] ret = new int[2];
            fs.Seek(SeekWhere, SeekOrigin.Begin);
            RootDirRecord tempR = ReadRecord();
            int RetPos = (1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property;
            fs.Seek(RetPos, SeekOrigin.Begin);
            Inode tempI = ReadInode();

            for (int f = 0; f < 10; f++)
            {
                if (tempI.Array_Of_Address_Property[f] != 0)
                {
                    fs.Seek(tempI.Array_Of_Address_Property[f], SeekOrigin.Begin);

                    if (f == 9)
                    {
                        for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                        {
                            fs.Seek(tempI.Array_Of_Address_Property[f] + 4 * g, SeekOrigin.Begin);
                            byte[] tempArray = new byte[4];
                            fs.Read(tempArray, 0, tempArray.Length);
                            int tempSeek = BitConverter.ToInt32(tempArray, 0);

                            if (tempSeek != 0)
                            {
                                fs.Seek(tempSeek, SeekOrigin.Begin);
                                RootDirRecord tempR3 = ReadRecord();
                                if (tempR3.Name_Property.Replace(" ", string.Empty) == name)
                                {
                                    ret[0] = tempI.Array_Of_Address_Property[f];
                                    ret[1] = g;
                                    return ret;
                                }
                            }
                        }
                        break;
                    }

                    RootDirRecord tempR2 = ReadRecord();
                    if (tempR2.Name_Property.Replace(" ", string.Empty) == name)
                    {
                        ret[0] = RetPos + 28;
                        ret[1] = f;
                        return ret;
                    }
                }
            }

            return null;
        }

        public static void ChangeUserGroup(string login, string id, byte newgroup)
        {
            //переходим к области пользователей
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int y = 0; y < SB.Max_Number_Users_Property; y++)
            {
                UserInfo tempU = ReadUser();
                if (tempU.Login_Property.Replace(" ", string.Empty) == login && tempU.ID_Property == Convert.ToInt32(id))
                {
                    if (SB.numGroupID_Property.Contains(newgroup) == false)
                    {
                        SB.numGroupID_Property.Add(newgroup);//Добавляем новый ID группы
                        SB.numGroupID_Property.Sort();//сортируем ID группы
                    }
                    fs.Seek(-300, SeekOrigin.Current);
                    fs.WriteByte(newgroup);

                    ChangeUserGroupAllInDir((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property, tempU, newgroup);

                    break;
                }
            }
        }

        public static void ChangeUserGroupAllInDir(long S, UserInfo U, byte numg)
        {
            //Переходим в область хранения инодов,к нужной папке
            fs.Seek(S, SeekOrigin.Begin);
            Inode MainInode = ReadInode();

            for (int i = 0; i < 10; i++)
            {
                if (MainInode.Array_Of_Address_Property[i] != 0)
                {
                    if (i == 9)
                    {
                        for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                        {
                            fs.Seek(MainInode.Array_Of_Address_Property[i] + 4 * g, SeekOrigin.Begin);
                            byte[] tempArray = new byte[4];
                            fs.Read(tempArray, 0, tempArray.Length);
                            int tempSeek = BitConverter.ToInt32(tempArray, 0);

                            //Изменение записи,инода,привязки
                            if (tempSeek != 0)
                            {
                                ChangeUserGroupInode(tempSeek, U, numg);
                                //fs.Seek(CurPos, SeekOrigin.Begin);
                            }
                        }
                        break;
                    }
                    //Изменение записи,инода,привязки
                    ChangeUserGroupInode(MainInode.Array_Of_Address_Property[i], U, numg);

                }
            }

        }

        public static void ChangeUserGroupInode(int current, UserInfo user, byte numg)
        {
            fs.Seek(current, SeekOrigin.Begin);
            RootDirRecord tempR = ReadRecord();
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
            Inode tempI = ReadInode();
            if (tempI.User_ID_Property == user.ID_Property)
            {
                string tempAccess = getAccess(tempI.Access_Property);

                if (Char.GetNumericValue(tempAccess[0]) == 0)
                {
                    //если папка,запускаем рекурсию для изменения файлов внутри папки
                    ChangeUserGroupAllInDir((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, user, numg);
                }
                //производим изменение инода
                fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
                fs.Seek(3, SeekOrigin.Current);
                fs.WriteByte(numg);
            }
        }

        public static bool DirOrFile(long seek)
        {
            fs.Seek(seek, SeekOrigin.Begin);
            RootDirRecord tempR = ReadRecord();
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
            Inode tempI = ReadInode();

            string tempAccess = getAccess(tempI.Access_Property);

            if (Char.GetNumericValue(tempAccess[0]) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void GetAllFromDir(int NumI)
        {
            Dirs.Clear();
            Files.Clear();
            HideFiles.Clear();
            SysFiles.Clear();
            HideSysFiles.Clear();
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * NumI, SeekOrigin.Begin);
            Inode MainInode = ReadInode();

            for (int i = 0; i < 10; i++)
            {
                if (MainInode.Array_Of_Address_Property[i] != 0)
                {
                    if (i == 9)
                    {
                        for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                        {
                            fs.Seek(MainInode.Array_Of_Address_Property[i] + 4 * g, SeekOrigin.Begin);
                            byte[] tempArray = new byte[4];
                            fs.Read(tempArray, 0, tempArray.Length);
                            int tempSeek = BitConverter.ToInt32(tempArray, 0);

                            if (tempSeek != 0)
                            {
                                fs.Seek(tempSeek, SeekOrigin.Begin);
                                RootDirRecord temp2 = ReadRecord();
                                if (DirOrFile(tempSeek) == true)
                                {
                                    Dirs.Add(temp2.Name_Property.Replace(" ", string.Empty));
                                }
                                else
                                {
                                    Inode tempI2 = FindInode(temp2.Number_Inode_Property);
                                    string acc2 = getAccess(tempI2.Access_Property);
                                    if (acc2[11] == '1' && acc2[12] == '1')
                                    {
                                        HideSysFiles.Add(temp2.Name_Property.Replace(" ", string.Empty) + temp2.Exstension_Property.Replace(" ", string.Empty));
                                    }
                                    else if (acc2[11] == '1')
                                    {
                                        HideFiles.Add(temp2.Name_Property.Replace(" ", string.Empty) + temp2.Exstension_Property.Replace(" ", string.Empty));
                                    }
                                    else if (acc2[12] == '1')
                                    {
                                        SysFiles.Add(temp2.Name_Property.Replace(" ", string.Empty) + temp2.Exstension_Property.Replace(" ", string.Empty));
                                    }
                                    else
                                    {
                                        Files.Add(temp2.Name_Property.Replace(" ", string.Empty) + temp2.Exstension_Property.Replace(" ", string.Empty));
                                    }
                                }
                            }
                        }
                        break;
                    }

                    fs.Seek(MainInode.Array_Of_Address_Property[i], SeekOrigin.Begin);
                    RootDirRecord temp = ReadRecord();
                    if (DirOrFile(MainInode.Array_Of_Address_Property[i]) == true)
                    {
                        Dirs.Add(temp.Name_Property.Replace(" ", string.Empty));
                    }
                    else
                    {
                        Inode tempI = FindInode(temp.Number_Inode_Property);
                        string acc1 = getAccess(tempI.Access_Property);
                        if (acc1[11] == '1' && acc1[12] == '1')
                        {
                            HideSysFiles.Add(temp.Name_Property.Replace(" ", string.Empty) + temp.Exstension_Property.Replace(" ", string.Empty));
                        }
                        else if (acc1[11] == '1')
                        {
                            HideFiles.Add(temp.Name_Property.Replace(" ", string.Empty) + temp.Exstension_Property.Replace(" ", string.Empty));
                        }
                        else if (acc1[12] == '1')
                        {
                            SysFiles.Add(temp.Name_Property.Replace(" ", string.Empty) + temp.Exstension_Property.Replace(" ", string.Empty));
                        }
                        else
                        {
                            Files.Add(temp.Name_Property.Replace(" ", string.Empty) + temp.Exstension_Property.Replace(" ", string.Empty));
                        }

                    }

                }
            }
        }

        public static string ReadData(string nameoffile)
        {
            string str = "";
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < SB.Max_Number_Users_Property * SB.Available_Property; i++)
            {
                RootDirRecord tempR = ReadRecord();
                if ((tempR.Name_Property.Replace(" ", string.Empty)) == nameoffile)
                {
                    fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
                    Inode tempI = ReadInode();

                    //tempI.Block_Count_Property
                    //int Blocks = tempI.File_Size_Property / SB.One_Block_Size_Property;
                    //if (tempI.File_Size_Property % SB.One_Block_Size_Property != 0)
                    //{
                    //    Blocks++;
                    //}

                    for (int j = 0; j < 10; j++)
                    {
                        int CountOfRead = 0;
                        if (tempI.Block_Count_Property - 1 == j)
                        {
                            CountOfRead = tempI.File_Size_Property - ((tempI.Block_Count_Property - 1) * SB.One_Block_Size_Property);
                        }
                        else
                        {
                            CountOfRead = SB.One_Block_Size_Property;
                        }

                        if (tempI.Array_Of_Address_Property[j] != 0)
                        {
                            if (j == 9)
                            {
                                for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                                {
                                    fs.Seek(tempI.Array_Of_Address_Property[j] + 4 * g, SeekOrigin.Begin);
                                    byte[] tempArray = new byte[4];
                                    fs.Read(tempArray, 0, tempArray.Length);
                                    int tempSeek = BitConverter.ToInt32(tempArray, 0);

                                    if (tempSeek != 0)
                                    {
                                        fs.Seek(tempSeek, SeekOrigin.Begin);
                                        byte[] t2 = new byte[CountOfRead];
                                        fs.Read(t2, 0, t2.Length);
                                        str += Encoding.ASCII.GetString(t2);
                                    }
                                }
                                break;
                            }

                            fs.Seek(tempI.Array_Of_Address_Property[j], SeekOrigin.Begin);
                            byte[] t1 = new byte[CountOfRead];
                            fs.Read(t1, 0, t1.Length);
                            str += Encoding.ASCII.GetString(t1);
                        }
                    }
                    break;
                }
            }
            return str;
        }

        public static void FillData(string nameoffile, string data)
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < SB.Max_Number_Users_Property * SB.Available_Property; i++)
            {
                RootDirRecord tempR = ReadRecord();
                if ((tempR.Name_Property.Replace(" ", string.Empty)) == nameoffile)
                {
                    fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + 68 * tempR.Number_Inode_Property, SeekOrigin.Begin);
                    Inode tempI = ReadInode();

                    for (int f = 0; f < 10; f++)
                    {
                        if (tempI.Array_Of_Address_Property[f] != 0)
                        {
                            //fs.Seek(tempI.Array_Of_Address_Property[f], SeekOrigin.Begin);

                            if (f == 9)
                            {
                                for (int g = 0; g < SB.One_Block_Size_Property / 4; g++)
                                {
                                    fs.Seek(tempI.Array_Of_Address_Property[f] + 4 * g, SeekOrigin.Begin);
                                    byte[] tempArray = new byte[4];
                                    fs.Read(tempArray, 0, tempArray.Length);
                                    int tempSeek = BitConverter.ToInt32(tempArray, 0);

                                    if (tempSeek != 0)
                                    {
                                        SetValueBitmap(tempSeek / SB.One_Block_Size_Property, 0, 1 * SB.One_Block_Size_Property);
                                        //fs.Seek(tempSeek, SeekOrigin.Begin);
                                        //for (int k = 0; k < SB.One_Block_Size_Property; k++)
                                        //{
                                        //    fs.WriteByte(0);
                                        //}
                                    }
                                }
                                tempI.Array_Of_Address_Property[f] = 0;
                                break;
                            }

                            SetValueBitmap(tempI.Array_Of_Address_Property[f] / SB.One_Block_Size_Property, 0, 1 * SB.One_Block_Size_Property);
                            //for (int k = 0; k < SB.One_Block_Size_Property; k++)
                            //{
                            //    fs.WriteByte(0);
                            //}
                        }
                        tempI.Array_Of_Address_Property[f] = 0;
                    }
                    //CheckBitMap();

                    var result = (from Match m in Regex.Matches(data, @".{1," + SB.One_Block_Size_Property + "}") select m.Value).ToList();
                    string[] str = result.ToArray();
                    for (int j = 0; j < str.Length; j++)
                    {
                        if (j == 9)
                        {
                            int tempFB = getFreeBlock();
                            SetValueBitmap(tempFB, 1, 1 * SB.One_Block_Size_Property);
                            tempI.Array_Of_Address_Property[j] = tempFB * SB.One_Block_Size_Property;

                            for (int g = 0; g < str.Length - j; g++)
                            {
                                int tempFBin = getFreeBlock();
                                SetValueBitmap(tempFBin, 1, 1 * SB.One_Block_Size_Property);
                                fs.Seek(tempFB * SB.One_Block_Size_Property + g * 4, SeekOrigin.Begin);
                                fs.Write(BitConverter.GetBytes(tempFBin * SB.One_Block_Size_Property), 0, BitConverter.GetBytes(tempFBin * SB.One_Block_Size_Property).Length);
                                fs.Seek(tempFBin * SB.One_Block_Size_Property, SeekOrigin.Begin);
                                fs.Write(Encoding.ASCII.GetBytes(str[j + g]), 0, Encoding.ASCII.GetBytes(str[j + g]).Length);

                                //CheckBitMap();
                            }

                            break;
                        }

                        int FB = getFreeBlock();
                        SetValueBitmap(FB, 1, 1 * SB.One_Block_Size_Property);
                        tempI.Array_Of_Address_Property[j] = FB * SB.One_Block_Size_Property;
                        fs.Seek(FB * SB.One_Block_Size_Property, SeekOrigin.Begin);
                        fs.Write(Encoding.ASCII.GetBytes(str[j]), 0, Encoding.ASCII.GetBytes(str[j]).Length);

                        //CheckBitMap();
                    }
                    tempI.File_Size_Property = data.Length;
                    tempI.File_Modif_Property = DateTime.Now;
                    tempI.Block_Count_Property = str.Length;
                    tempI.Number_Property = tempR.Number_Inode_Property;
                    AddInode(tempI);
                    break;
                }
            }

            //CheckBitMap();

        }

        public static void Replacement(string name, RootDirRecord newR, Inode newI)
        {
            fs.Seek(FindRecSeek(name), SeekOrigin.Begin);

            fs.Write(Encoding.ASCII.GetBytes(newR.Name_Property), 0, Encoding.ASCII.GetBytes(newR.Name_Property).Length);
            fs.Write(Encoding.ASCII.GetBytes(newR.Exstension_Property), 0, Encoding.ASCII.GetBytes(newR.Exstension_Property).Length);
            fs.Write(BitConverter.GetBytes(newR.Number_Inode_Property), 0, BitConverter.GetBytes(newR.Number_Inode_Property).Length);

            AddInode(newI);

        }

        public static RootDirRecord FindRec(string name)
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < SB.Max_Number_Users_Property * SB.Available_Property; i++)
            {
                RootDirRecord tempR = ReadRecord();
                if ((tempR.Name_Property.Replace(" ", string.Empty)) == name)
                {
                    return tempR;
                }
            }
            return null;
        }

        public static UserInfo FindUser(int id)
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < SB.Max_Number_Users_Property; i++)
            {
                UserInfo tempU = ReadUser();
                if (tempU.ID_Property == id)
                {
                    return tempU;
                }
            }
            return null;
        }

        public static int FindRecSeek(string name)
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property + SB.Inode_Block_Size_Property + SB.User_Info_Block_Size_Property) * SB.One_Block_Size_Property, SeekOrigin.Begin);
            for (int i = 0; i < SB.Max_Number_Users_Property * SB.Available_Property; i++)
            {
                RootDirRecord tempR = ReadRecord();
                if ((tempR.Name_Property.Replace(" ", string.Empty)) == name)
                {
                    return (int)(fs.Position - 29);
                }
            }
            return -1;
        }

        public static Inode FindInode(int num)
        {
            fs.Seek((1 + SB.Bitmap_Block_Size_Property + SB.Inode_Bitmap_Block_Size_Property) * SB.One_Block_Size_Property + num * 68, SeekOrigin.Begin);
            Inode tempI = ReadInode();
            return tempI;
        }



        public static bool CheckID(UserInfo U, Inode I)
        {
            string tempAccess = getAccess(I.Access_Property);
            if (U.ID_Property == I.User_ID_Property || U.Group_ID_Property == 0 || (I.Group_ID_Property == 0 && I.User_ID_Property == 0 && tempAccess[0] == '0'))
            {
                return true;
            }
            return false;
        }



        public static bool CheckReadAccess(UserInfo U, Inode I)
        {
            if (U.Group_ID_Property == 0)
            {
                return true;
            }
            string tempAccess = getAccess(I.Access_Property);
            if (tempAccess[1] == '1' && U.ID_Property == I.User_ID_Property)
            {
                return true;
            }
            if (tempAccess[4] == '1' && U.Group_ID_Property == I.Group_ID_Property)
            {
                return true;
            }
            if (tempAccess[7] == '1')
            {
                return true;
            }
            return false;
        }

        public static bool CheckWriteAccess(UserInfo U, Inode I)
        {
            if (U.Group_ID_Property == 0)
            {
                return true;
            }
            string tempAccess = getAccess(I.Access_Property);
            if (tempAccess[2] == '1' && U.ID_Property == I.User_ID_Property)
            {
                return true;
            }
            if (tempAccess[5] == '1' && U.Group_ID_Property == I.Group_ID_Property)
            {
                return true;
            }
            if (tempAccess[8] == '1')
            {
                return true;
            }
            return false;
        }

        public static bool CheckRecord(string name, string exe)
        {
            fs.Seek((1 * SB.One_Block_Size_Property) + (SB.Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Bitmap_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.Inode_Block_Size_Property * SB.One_Block_Size_Property)
                + (SB.User_Info_Block_Size_Property * SB.One_Block_Size_Property), SeekOrigin.Begin);

            RootDirRecord temp = new RootDirRecord();
            for (int i = 0; i < (SB.Record_Block_Size_Property * SB.One_Block_Size_Property); i = (i + 29))
            {
                temp = ReadRecord();
                //&& temp.Exstension_Property.Replace(" ", string.Empty) == exe
                if (temp.Name_Property.Replace(" ", string.Empty) == name)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

