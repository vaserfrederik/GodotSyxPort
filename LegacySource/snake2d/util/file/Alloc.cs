using System;

namespace Snake2D.Util.File
{
    public static class Alloc
    {
        public static bool Debug = false;

        public static int[] Ii(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new int[size];
        }

        public static int[][] I2(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new int[size][];
        }

        public static int[][] I2(int a, int b)
        {
            //catch this
            if (Debug)
            {
                new Exception((a * b).ToString()).StackTrace.ToString();
            }
            return new int[a][];
        }

        public static int[][][] I3(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new int[size][][]; // Corrected to match C# array initialization
        }

        public static byte[] Bb(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new byte[size];
        }

        public static byte[][] B2(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new byte[size][];
        }

        public static byte[][] B2(int a, int b)
        {
            //catch this
            if (Debug)
            {
                new Exception((a * b).ToString()).StackTrace.ToString();
            }
            return new byte[a][];
        }

        public static byte[][][] B3(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new byte[size][][]; // Corrected to match C# array initialization
        }

        public static char[] Cc(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new char[size];
        }

        public static char[][] C2(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new char[size][];
        }

        public static char[][] C2(int a, int b)
        {
            //catch this
            if (Debug)
            {
                new Exception((a * b).ToString()).StackTrace.ToString();
            }
            return new char[a][];
        }

        public static char[][][] C3(int size)
        {
            //catch this
            if (Debug)
            {
                new Exception(size.ToString()).StackTrace.ToString();
            }
            return new char[size][][]; // Corrected to match C# array initialization
        }
    }
}