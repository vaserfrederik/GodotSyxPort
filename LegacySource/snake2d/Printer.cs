using System;
using System.Collections.Generic;

namespace Snake2D
{
    public class Printer
    {
        private const string pre = "[SNAKE2D] ";
        private const string fin = "[SNAKE2D] ------------------------";

        private Printer()
        {
        }

        public static void Ln()
        {
            Console.WriteLine();
        }

        public static void Ln(object info)
        {
            Console.WriteLine(pre + info);
        }

        public static void Pr(object info)
        {
            Console.Write(info);
        }

        public static void Err(object info)
        {
            Console.Error.WriteLine(pre + info);
        }

        public static void Ln(string title, params string[] info)
        {
            Console.Write(pre + title + ": ");
            for (int i = 0; i < info.Length; i++)
            {
                Console.Write(info[i] + (i < info.Length - 1 ? ", " : ""));
            }
            Console.WriteLine();
        }

        public static void Ln(string title, IEnumerable<string> info)
        {
            Console.Write(pre + title + ": ");
            foreach (var s in info)
            {
                Console.Write(s + ", ");
            }
            Console.WriteLine();
        }

        public static void Fin()
        {
            Console.WriteLine(fin);
            Console.WriteLine();
        }
    }
}