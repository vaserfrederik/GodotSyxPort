using System;

namespace Init.Resources
{
    public static class Meal
    {
        private Meal()
        {
        }

        public static int Make(ResG e, int amount, double pref)
        {
            return (e.Index() << 16) | (amount << 8) | Clamp((int)(255 * pref), 0, 255);
        }

        public static ResG Get(int data)
        {
            return RESOURCES.EDI().All().Get((data >> 16) & 0x0FF);
        }

        public static int Amount(int data)
        {
            return (data >> 8) & 0x0FF;
        }

        public static double Pref(int data)
        {
            return (data & 0x0FF) / 255.0;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
    }
}