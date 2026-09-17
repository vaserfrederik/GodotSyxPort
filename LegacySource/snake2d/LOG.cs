using System;

namespace Snake2D
{
    public sealed class LOG
    {
        private LOG()
        {
        }

        private static string lasth;
        private static int li = 0;
        private static int lln = 0;
        private static string tab = "      ";

        public static void Ln()
        {
            Console.WriteLine();
        }

        private static void Header(TextWriter o)
        {
            StackTraceElement[] ee = new StackTrace().GetStackTrace();
            string s = ee[2].ClassName;
            int ln = 0;
            if (s.Contains("."))
            {
                string[] ss = s.Split('.');
                s = ss[ss.Length - 1];
                ln = ee[2].GetFileLineNumber();
            }
            if (!s.Equals(lasth) || li > 10000)
            {
                o.WriteLine();
                o.Write("[GAME]");
                //o.Write(ee[2]);
                string cl = ee[2].ClassName;
                if (cl.IndexOf('$') > 0)
                    cl = cl.Substring(0, cl.IndexOf('$'));
                o.WriteLine(" (" + cl + ".cs:" + ln + ")");

                li = 0;
                lln = -1;
            }
            if (lln != ln)
            {
                lln = ln;
                string l = "[" + ln + "]";
                o.Write(l);
                for (int i = 0; i < tab.Length - l.Length; i++)
                    o.Write(" ");
            }
            else
            {
                o.Write(tab);
            }

            li++;
            lasth = s;
        }

        public static void Ln(object info)
        {
            Header(Console.Out);
            Console.WriteLine(info);
        }

        public static void Ln(object a, object b)
        {
            Ln(a);
            Ln(b);
        }

        public static void Ln(object a, object b, object c)
        {
            Ln(a);
            Ln(b);
            Ln(c);
        }

        public static void Ln(object[] info)
        {
            foreach (object oo in info)
            {
                Ln(oo);
            }
        }

        public static void Err(object info)
        {
            Header(Console.Error);
            Console.Error.WriteLine(info);
        }

        //public static void Err(object[] info)
        //{
        //    Header(Console.Error);
        //    foreach (object oo in info)
        //    {
        //        Console.Error.WriteLine(oo);
        //    }
        //}

        public static string Bits(long l)
        {
            string s = "";
            int sp = 0;
            for (int bi = 0; bi < 64; bi++)
            {
                if ((sp == 8))
                {
                    s += "_";
                    sp = 0;
                }
                long m = 1L << (63 - bi);

                if ((l & m) != 0)
                {
                    s += "1";
                }
                else
                {
                    s += "0";
                }
                sp++;
            }
            return s;
        }

        public static string WS(int spaces)
        {
            string s = "";
            for (int i = 0; i < spaces; i++)
                s += " ";
            return s;
        }

        public static string NL()
        {
            return Environment.NewLine;
        }
    }
}