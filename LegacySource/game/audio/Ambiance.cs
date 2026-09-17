using System;
using System.Collections.Generic;

namespace Game.Audio
{
    public sealed class Ambiance : MAPPED
    {
        public AmbianceUpdater.Channel Channel { get; private set; }

        public readonly LIST<SoundStream> Streams;
        public double Priority { get; private set; }
        private double Gain { get; set; }
        public readonly string Key;
        public readonly int Index;

        public Ambiance(string key, LISTE<Ambiance> all, LIST<SoundStream> streams)
        {
            this.Streams = streams;
            Index = all.Add(this);
            Key = key;
        }

        public double GainValue()
        {
            return Gain;
        }

        public Ambiance GainSet(double gain)
        {
            Gain = gain;
            return this;
        }

        public double PriorityValue()
        {
            return Priority;
        }

        public Ambiance PrioritySet(double priority)
        {
            Priority = priority;
            return this;
        }

        public Ambiance PriorityInc(double priority)
        {
            Priority += priority;
            return this;
        }

        public int Index()
        {
            return Index;
        }

        public string Key()
        {
            return Key;
        }
    }
}