using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S5FS
{
    public partial class Plan : Form
    {
        public Plan()
        {
            InitializeComponent();
        }

        List<Process> HIGH = new List<Process>();
        int iH = 0;
        //List<Process> NORMAL = new List<Process>();
        //int iN = 0;
        List<Process> LOW = new List<Process>();
        int iL = 0;
        List<Process> SHOW = new List<Process>();
        static int NumPID = 1;
        //Названия для процессов
        string[] NameOfProc = { "init      ", "top       ", "xwd       ", "vmware    ", "kdeinit   ", "keventd   ", "kapm-idled", "kswapd    ", "kreclaimd ", "bdflush   ", "kupdated  " };
        Random R = new Random();
        int step = 1;
        int quant = 0;
        int after = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            Process P = new Process()
            {
                PID = NumPID,
                name = NameOfProc[R.Next(0, 10)],
                priority = (int)numericUpDown1.Value,
                state = 'R',
                time = 0,
                fulltime = (int)numericUpDown2.Value
            };
            NumPID++;

            dataGridView1.Columns[0].Frozen = true;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            int rowNum = dataGridView1.Rows.Add();
            dataGridView1.Rows[rowNum].Cells[0].Value = "PID=" + P.PID + " | " + /*"Name=" + P.name + " | " +*/ "Priority=" + P.priority.ToString() + " | " + "State=" + P.state + " | " + "Burst=" + P.fulltime.ToString();

            ref Process pointer = ref P;
            if (P.priority <= 5)
            {
                HIGH.Add(pointer);
                dataGridView1.Rows[rowNum].Cells[0].Style.BackColor = Color.Red;
            }
            else
            {
                LOW.Add(pointer);
                dataGridView1.Rows[rowNum].Cells[0].Style.BackColor = Color.Green;
            }
            SHOW.Add(P);
            dataGridView1.ClearSelection();
            //HIGH[0].name = "KUKU SUCHARA";
        }

        public void Scheduler()
        {
            //Переходим к той очереди, в которой есть процесс
            if (HIGH.Count != 0)
            {
                if (LOW.Count != 0)
                {
                    LOW[0].state = 'R';
                }
                if (HIGH[0].time >= HIGH[0].fulltime)
                {
                    HIGH[0].state = 'F';
                    HIGH.RemoveAt(0);
                    quant = 0;
                }

                if (quant == 5)
                {
                    if (HIGH[0].state == 'E')
                    {
                        HIGH[0].state = 'R';
                    }
                    quant = 0;
                    HIGH.Add(HIGH[0]);
                    HIGH.RemoveAt(0);
                }

                if (HIGH.Count != 0)
                {
                    switch (HIGH[0].state)
                    {
                        case 'R':
                            HIGH[0].state = 'E';
                            Execute(HIGH[0]);
                            break;
                        case 'E':
                            Execute(HIGH[0]);
                            break;
                    }
                }

                if (HIGH.Count == 0 && LOW.Count != 0)
                {
                    RunLOW();
                }
            }
            else if (LOW.Count != 0)
            {
                RunLOW();
            }
            else
            {
                MessageBox.Show("Процессы выполнены!");
                return;
            }

            

            dataGridView1.FirstDisplayedScrollingColumnIndex = dataGridView1.Columns.Add(step.ToString(), step.ToString());
            for (int j = 0; j < SHOW.Count; j++)
            {
                dataGridView1.Rows[j].Cells[0].Value = "PID=" + SHOW[j].PID + " | " /*+ "Name=" + SHOW[j].name + " | "*/ + "Priority=" + SHOW[j].priority.ToString() + " | " + "State=" + SHOW[j].state + " | " + "Burst=" + SHOW[j].fulltime.ToString();

                if (SHOW[j].priority <= 5)
                {
                    dataGridView1.Rows[j].Cells[0].Style.BackColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[j].Cells[0].Style.BackColor = Color.Green;
                }

                if (SHOW[j].state == 'E')
                    dataGridView1.Rows[j].Cells[step].Style.BackColor = Color.Black;
                if (SHOW[j].state == 'R')
                    dataGridView1.Rows[j].Cells[step].Style.BackColor = Color.Gray;
            }
            step++;


        }


        public void RunLOW()
        {
            if (LOW[0].time >= LOW[0].fulltime)
            {
                LOW[0].state = 'F';
                LOW.RemoveAt(0);
            }

            Sort();

            if (LOW.Count != 0)
            {

                Sampling();

                after++;
                if (after == 3)
                {
                    LOW[0].priority++;
                    LOW[LOW.Count - 1].priority--;
                    after = 0;
                }


                switch (LOW[0].state)
                {
                    case 'R':
                        LOW[0].state = 'E';
                        LOW[0].time++;
                        break;
                    case 'E':
                        LOW[0].time++;
                        break;
                }
                for (int k = 1; k < LOW.Count; k++)
                {
                    LOW[k].wait++;
                }
            }
        }

        public void Execute(Process P)
        {
            quant++;
            P.time++;
        }

        public void Sort()
        {
            LOW = LOW.OrderBy(x => x.priority).ToList();

            for (int x = 0; x < LOW.Count; x++)
            {
                LOW[x].state = 'R';
            }
        }

        public void Sampling()
        {
            int i = 0;
            int j = LOW.Count - 1;
            int FirstPriority = LOW[0].priority;
            int LastPriority = LOW[LOW.Count - 1].priority;
            List<Process> First = new List<Process>();
            while (LOW.Count != i && LOW[i].priority == FirstPriority)
            {
                First.Add(LOW[i]);
                i++;
            }
            First = First.OrderBy(x => x.wait).ToList();
            First.Reverse();

            int g = 0;
            //int time = First[0].time;
            int wait = First[0].wait;
            List<Process> Time = new List<Process>();
            while (First.Count != g && First[g].wait == wait)
            {
                Time.Add(First[g]);
                g++;
            }
            Time = Time.OrderBy(x => x.time).ToList();
            for (int v = 0; v < Time.Count; v++)
            {
                First[v] = Time[v];
            }


            for (int k = 0; k < First.Count; k++)
            {
                LOW[k] = First[k];
            }



            if (FirstPriority != LastPriority)
            {
                List<Process> Last = new List<Process>();
                while (j != -1 && LOW[j].priority == LastPriority)
                {
                    Last.Add(LOW[j]);
                    j--;
                }
                Last = Last.OrderBy(x => x.wait).ToList();
                //Last.Reverse();
                for (int h = LOW.Count - Last.Count, f = 0; h < LOW.Count; h++, f++)
                {
                    LOW[h] = Last[f];
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Scheduler();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            while(LOW.Count!=0 || HIGH.Count != 0)
            {
                Scheduler();
                Thread.Sleep(100);
            }
        }
    }
}
