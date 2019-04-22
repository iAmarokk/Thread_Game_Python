using System;
using System.Threading;

namespace Python3
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            Python.InitScreen();
            for(int j =0;j<100;j++)
            Python.AddHare();
            int max = 20;
            Python[] p = new Python[max];
            Thread[] t = new Thread[max];
            for (int j = 0; j < max; j++)
            {
                p[j] = Python.Create();
                t[j] = new Thread(p[j].Run);
                //t[j].IsBackground = true;
                //t[j].Priority = (ThreadPriority)j;
                t[j].Start();
            }
            while(true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.KeyChar >= '0' && key.KeyChar <= '9')
                    t[Convert.ToInt16(key.KeyChar.ToString())].Abort();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
            
        }
    }
}
