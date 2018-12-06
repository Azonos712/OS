using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5FS
{
    public class Process
    {
        /// <summary>
        /// ID процесса
        /// </summary>
        private int pid;
        public int PID
        {
            get { return pid; }
            set { pid = value; }
        }

        /// <summary>
        /// Имя процесса
        /// </summary>
        private string nm;
        public string name
        {
            get { return nm; }
            set { nm = value; }
        }

        /// <summary>
        /// Приоритет процесса
        /// </summary>
        private int prior;
        public int priority
        {
            get { return prior; }
            set { prior = value; }
        }

        /// <summary>
        /// Состояние процесса
        /// </summary>
        char stt;
        public char state
        {
            get { return stt; }
            set { stt = value; }
        }

        /// <summary>
        /// Выполненое количество квантов процесса
        /// </summary>
        private int tm;
        public int time
        {
            get { return tm; }
            set { tm = value; }
        }

        /// <summary>
        /// CPU birst(Общее количество квантов для выполнения процесса)
        /// </summary>
        private int flltm;
        public int fulltime
        {
            get { return flltm; }
            set { flltm = value; }
        }

        /// <summary>
        /// Время ожидания процесса
        /// </summary>
        private int wt;
        public int wait
        {
            get { return wt; }
            set { wt = value; }
        }
    }
}
