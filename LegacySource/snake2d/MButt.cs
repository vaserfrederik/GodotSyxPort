using System;
using System.Collections.Generic;

namespace snake2d
{
    public enum MButt
    {
        LEFT, RIGHT, WHEEL, WHEEL_SPIN
    }

    public static class MButtExtensions
    {
        private static double delta;
        private static float wheelDy;
        private static Dictionary<MButt, bool> isDownMap = new Dictionary<MButt, bool>();
        private static Dictionary<MButt, long> nanoNowMap = new Dictionary<MButt, long>();
        private static Dictionary<MButt, bool> isDoubleMap = new Dictionary<MButt, bool>();
        private static Dictionary<MButt, int> clicksMap = new Dictionary<MButt, int>();

        static MButtExtensions()
        {
            foreach (MButt butt in Enum.GetValues(typeof(MButt)))
            {
                isDownMap[butt] = false;
                nanoNowMap[butt] = -1;
                isDoubleMap[butt] = false;
                clicksMap[butt] = 0;
            }
        }

        public static List<MButt> ALL => Enum.GetValues(typeof(MButt)).Cast<MButt>().ToList();

        public static float ClearWheelSpin()
        {
            float f = wheelDy;
            wheelDy = 0;
            return f;
        }

        public static float PeekWheel()
        {
            return wheelDy;
        }

        public static bool IsDown(this MButt butt)
        {
            return isDownMap[butt];
        }

        public static bool ConsumeClick(this MButt butt)
        {
            if (clicksMap[butt] > 0)
            {
                clicksMap[butt]--;
                return true;
            }
            return false;
        }

        public static bool ConsumeAllClick(this MButt butt)
        {
            if (clicksMap[butt] > 0)
            {
                clicksMap[butt] = 0;
                return true;
            }
            return false;
        }

        public static bool IsDouble(this MButt butt)
        {
            return isDoubleMap[butt];
        }
    }
}