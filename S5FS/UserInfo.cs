using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    class UserInfo
    {
        //ID пользователя
        private byte ID;
        public byte ID_Property
        {
            get { return ID; }
            set { ID = value; }
        }

        //ID группы
        private byte Group_ID;
        public byte Group_ID_Property
        {
            get { return Group_ID; }
            set { Group_ID = value; }
        }

        //Логин
        private string Login;
        public string Login_Property
        {
            get { return Login; }
            set { Login = value; }
        }

        //Hash
        private int Hash;
        public int Hash_Property
        {
            get { return Hash; }
            set { Hash = value; }
        }

        //Homedir
        private int Homedir;
        public int Homedir_Property
        {
            get { return Homedir; }
            set { Homedir = value; }
        }
    }
}
