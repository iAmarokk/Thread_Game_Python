using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Python3
{
    public struct Coord
    {
        public int x;
        public int y;
        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Coord Plus(Coord c)
        {
            return new Coord(this.x + c.x, this.y + c.y);
        }
        public static Coord operator + (Coord c1, Coord c2)
        {
            return new Coord(c1.x + c2.x, c1.y + c2.y);
        }
    }

    class Python
    {
        public static readonly Coord size = new Coord(70, 24);
        public static readonly char aNone = ' ';
        public static readonly char aWall = '#';
        public static readonly char aBody = 'O';
        public static readonly char aHare = '"';
        public static readonly char aDead = 'x';
        public static readonly char[] aHead = new char[] { '<', '>', '^', 'v' };
        public static Random random = new Random();
        private static object block = new object();
        private static int number = 0;

        private static char[,] screen = new char[size.x, size.y];

        public static void InitScreen()
        {
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    if (x * y == 0 || x == size.x - 1 || y == size.y - 1)
                        PutScreen(new Coord(x,y),ConsoleColor.DarkGray,aWall);
                    else
                        PutScreen(new Coord(x,y),ConsoleColor.DarkGray,aNone);
        }

        private static void PutScreen(Coord coord, ConsoleColor color,char a)
        {
            lock(block)
            {
                if (!OnScreen(coord))
                    return;
                screen[coord.x, coord.y] = a;
                Console.ForegroundColor = color;
                Console.SetCursorPosition(coord.x, coord.y);
                Console.Write(a);
            }
        }

        public static void AddHare()
        {
            if (random.Next(10) > 0)
                return;
            Coord hare;
            int loop = 100;
            do
                hare = RandomCoord();
            while (!IsEmpty(hare) && --loop > 0);
            if (loop > 0)
                if (random.Next(10) == 0)
                    PutScreen(hare, ConsoleColor.Cyan, aDead);
                else
                    PutScreen(hare, ConsoleColor.Cyan, aHare);
        }

        public static Coord RandomCoord()
        {
            return new Coord(random.Next(0, size.x),
                             random.Next(0, size.y));
        }

        public static bool IsEmpty(Coord coord)
        {
            char c = screen[coord.x, coord.y];
            return (c == aNone || c == aHare || c== aDead);
        }

        public char Screen(Coord coord)
        {
            if (!OnScreen(coord))
                return aWall;
            return screen[coord.x, coord.y];
        }

        public static bool OnScreen(Coord coord)
        {
            return (coord.x >= 0 && coord.x < size.x &&
                    coord.y >= 0 && coord.y < size.y);
        }

        public enum Arrow
        {
            L,
            R,
            U,
            D
        };

        Coord head;
        Arrow arrow;
        Coord step;
        ConsoleColor color;
        Queue<Coord> body;
        bool dead;
        int grow;
        int nr;

        public static Python Create()
        {
            Coord start;
            int loop = 100;
            do
                start = RandomCoord();
            while (!IsEmpty(start) && --loop > 0);
            if (loop <= 0)
                return null;
            Python python = new Python(start);
            python.nr = number;
            number++;
            return python;
        }

        private Python(Coord start)
        {
            this.head = start;
            this.body = new Queue<Coord>();
            body.Enqueue(head);
            this.color = (ConsoleColor)random.Next(1, 15);
            TurnTo(Arrow.R);
            grow = 0;
            dead = false;
        }

        private void TurnTo(Arrow arrow)
        {
            if (this.arrow == arrow)
                return;
            this.arrow = arrow;
            step.x = 0;
            step.y = 0;
            switch(arrow)
            {
                case Arrow.L: step.x = -1; break;
                case Arrow.R: step.x = +1; break;
                case Arrow.U: step.y = -1; break;
                case Arrow.D:
                default:    step.y = +1; break;
            }
        }

        private void Turn()
        {
            if (random.Next(10) > 0)
                if (IsEmpty(head.Plus(step)))
                    return;
            for (int j = 0; j < 10; j++)
            {
                TurnTo((Arrow)random.Next(0, 4));
                if (IsEmpty(head.Plus(step)))
                    return;
            }
        }

        public void Step()
        {
            Turn();
            Coord nextHead = head + step;
            if (IsEmpty(nextHead) && !dead)
                body.Enqueue(nextHead);
            else
                nextHead = head;
            if (Screen(nextHead) == aHare)
                grow++;
            if (Screen(nextHead) == aDead)
                dead = true;
            Coord none = new Coord(-1, -1);
            if(body.Count > 1)
            {
                if (grow > 0)
                    grow--;
                else
                    none = body.Dequeue();
            }
            ShowMe(nextHead, head,none);
            head = nextHead;
        }

        public void ShowMe(Coord chead, Coord cBody, Coord cNone)
        {
            PutScreen(cBody, color, aBody);
            PutScreen(chead, color, aHead[(int)arrow]);
            PutScreen(cNone, color, aNone);

        }

        private void Info()
        {
            lock(block)
            {
                Console.SetCursorPosition(size.x + 2, nr);
                Console.ForegroundColor = color;
                Console.Write("#"+Thread.CurrentThread.ManagedThreadId +"\t"+ body.Count);
            }

        }

        public void Run()
        {

            while(true)
            {
                try
                {
                    while(true)
                    {
                        Step();
                        AddHare();
                        Info();
                        Thread.Sleep(20);
                        if (dead && body.Count <= 1)
                            return;
                    }
                }
                catch(ThreadAbortException ex)
                {
                    dead = true;
                    Thread.ResetAbort();
                }
                //if (random.Next(100) == 0)
                //    break;
            }
            PutScreen(head, color, aNone);
        }
    }
}
