using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Weather
{
    public interface IDoubleMutable
    {
        double D { get; set; }
        INFO Info { get; }
    }

    public class INFO
    {
        public string Name { get; }
        public string Description { get; }

        public INFO(CharSequence name, CharSequence desc)
        {
            Name = name.ToString();
            Description = desc.ToString();
        }
    }

    public class WeatherThing : IDoubleMutable
    {
        static LinkedList<WeatherThing> all = new LinkedList<WeatherThing>();
        private double d;
        public readonly INFO info;

        public WeatherThing(CharSequence name, CharSequence desc)
        {
            this.info = new INFO(name, desc);
            all.Add(this);
        }

        public double D
        {
            get => d;
            set => d = Math.Clamp(value, 0.0, 1.0);
        }

        public INFO Info => info;

        protected void Save(FilePutter file)
        {
            file.WriteDouble(d);
        }

        protected void Load(FileGetter file)
        {
            d = file.ReadDouble();
        }

        protected void Init()
        {
        }

        void Update(double ds)
        {
        }

        protected double AdjustTowards(double current, double speed, double target)
        {
            if (current >= target)
            {
                current -= speed;
                if (current < target)
                    current = target;
            }
            else if (current < target)
            {
                current += speed;
                if (current > target)
                    current = target;
            }
            return current;
        }
    }

    public interface CharSequence
    {
        string ToString();
    }

    public class FilePutter
    {
        private readonly BinaryWriter writer;

        public FilePutter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void WriteDouble(double value)
        {
            writer.Write(value);
        }
    }

    public class FileGetter
    {
        private readonly BinaryReader reader;

        public FileGetter(BinaryReader reader)
        {
            this.reader = reader;
        }

        public double ReadDouble()
        {
            return reader.ReadDouble();
        }
    }

    public class LinkedList<T> : List<T>
    {
    }
}