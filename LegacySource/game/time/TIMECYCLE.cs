using System;
using System.Text;

namespace Game.Time
{
    public abstract class TIMECYCLE
    {
        private readonly int bits;
        private readonly double secondsPerBit;
        private readonly double secondsPerCycle;
        private double partOfBit;
        private double partIfBitCircular;
        private readonly string postFix;
        private readonly string names;
        private int bitsSinceStart;
        private int bitsOfCycle;
        private double bitsOfDay;
        private double bitsOfSeason;
        private double bitsOfYear;
        private double secondOfBit;

        protected TIMECYCLE(int seconds, int amount, string postFix, string names)
        {
            secondsPerBit = seconds;
            this.bits = amount;

            secondsPerCycle = secondsPerBit * amount;
            this.postFix = postFix;
            this.names = names;
        }

        protected void Update(double currentSecond)
        {
            bitsSinceStart = (int)(currentSecond / secondsPerBit);
            secondOfBit = currentSecond % secondsPerBit;
            bitsOfCycle = bitsSinceStart % bits;

            partOfBit = (currentSecond % secondsPerBit) / secondsPerBit;
            if (partOfBit <= 0.5)
                partIfBitCircular = partOfBit * 2.0;
            else
                partIfBitCircular = 1.0 - (partOfBit - 0.5) * 2;

            bitsOfDay = (currentSecond % TIME.Days().BitSeconds()) / secondsPerBit;
            bitsOfSeason = (currentSecond % TIME.Seasons().BitSeconds()) / secondsPerBit;
            bitsOfYear = (currentSecond % TIME.Years().BitSeconds()) / secondsPerBit;
        }

        /**
         * 
         * @return part of the progression of the current bit. 0->1 
         */
        public double BitPartOf()
        {
            return partOfBit;
        }

        /**
         * 
         * @return circular part of progression of current bit 0->1->0
         */
        public double BitPartOfC()
        {
            return partIfBitCircular;
        }

        /**
         * 
         * @return the length of a bit in seconds
         */
        public double BitSeconds()
        {
            return secondsPerBit;
        }

        public double SecondOfBit()
        {
            return secondOfBit;
        }

        /**
         * 
         * @return amount o bits in a cycle
         */
        public int BitsPerCycle()
        {
            return bits;
        }

        public double BitConversion(TIMECYCLE toBits)
        {
            return secondsPerBit / toBits.secondsPerBit;
        }

        /**
         * 
         * @return nr of bits that have passed since year 0, age 0
         */
        public int BitsSinceStart()
        {
            return bitsSinceStart;
        }

        /**
         * 
         * @return the index of the current bit in this cycle
         */
        public int BitCurrent()
        {
            return bitsOfCycle;
        }

        public string BitNameCurrent()
        {
            return BitName(bitsOfCycle);
        }

        public double BitOfDay()
        {
            return bitsOfDay;
        }

        public double BitOfSeason()
        {
            return bitsOfSeason;
        }

        public double BitOfYear()
        {
            return bitsOfYear;
        }

        public abstract string BitName(int bit);

        public string CycleName()
        {
            return postFix;
        }

        public string CycleNames()
        {
            return names;
        }

        /**
         * 
         * @return total seconds per cycle
         */
        public double CycleSeconds()
        {
            return secondsPerCycle;
        }

        public static class Hours : TIMECYCLE
        {
            private readonly string[] names;

            public Hours(int seconds, int amount) : base(seconds, amount, "hour", "hours")
            {
                names = new string[amount];
                for (int i = 0; i < amount; i++)
                    names[i] = Numbers.GetSuffix(i + 1);
            }

            public override string BitName(int bit)
            {
                return names[bit];
            }
        }

        public static class Days : TIMECYCLE
        {
            private readonly string[] names;
            public readonly double dayShiftStart = 0.25;
            public readonly double dayShiftEnd = 0.75;
            private bool dayShift;
            private double partOfShift;

            public Days(int seconds, int amount) : base(seconds, amount, DicTime.¤¤Day, DicTime.¤¤Days)
            {
                names = new string[amount];
                for (int i = 0; i < amount; i++)
                    names[i] = Numbers.GetSuffix(i + 1);
            }

            protected override void Update(double currentSecond)
            {
                base.Update(currentSecond);
                dayShift = BitPartOf() >= dayShiftStart && BitPartOf() < dayShiftEnd;
                if (dayShift)
                {
                    partOfShift = (BitPartOf() - dayShiftStart) * 2.0;
                }
                else if (BitPartOf() < dayShiftStart)
                {
                    partOfShift = 0.5 + BitPartOf() * 2.0;
                }
                else
                {
                    partOfShift = (BitPartOf() - dayShiftEnd) * 2.0;
                }
            }

            public bool DayShift()
            {
                return dayShift;
            }

            public bool IsNightShift()
            {
                return !dayShift;
            }

            public double ShiftPartOf()
            {
                return partOfShift;
            }

            public override string BitName(int bit)
            {
                return names[bit];
            }
        }

        public static class Years : TIMECYCLE
        {
            private readonly string[] names;

            public Years(int seconds, int amount) : base(seconds, amount, DicTime.¤¤Year, DicTime.¤¤Years)
            {
                names = new string[amount];
                for (int i = 0; i < amount; i++)
                    names[i] = Numbers.GetSuffix(i + 1);
            }

            public override string BitName(int bit)
            {
                return names[bit];
            }
        }

        public static class Ages : TIMECYCLE
        {
            private readonly string[] names;

            public Ages(int seconds, Json jData, Json jText) : base(seconds, jData.I("AGES"), DicTime.¤¤Age, DicTime.¤¤Ages)
            {
                names = jText.Texts("AGES", jData.I("AGES", 1, 1000), 1000);
            }

            public override string BitName(int bit)
            {
                return names[bit];
            }
        }
    }
}