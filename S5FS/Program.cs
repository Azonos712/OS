using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S5FS
{
    static class Program
    {
        public static ApplicationContext Context { get; set; }

        //private static Emulator emu = new Emulator();
        //public static Emulator emulator { get => emu; set => emu = value; }
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Context = new ApplicationContext(new Form1());
            Application.Run(Context);
        }
    }
}
