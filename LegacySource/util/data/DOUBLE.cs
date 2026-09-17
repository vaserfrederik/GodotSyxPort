using System;
using System.IO;

namespace Util.Data
{
    public interface IDouble
    {
        double GetD();
    }

    public interface IDoubleMutable : IDouble
    {
        IDoubleMutable IncD(double d);
        IDoubleMutable SetD(double d);
    }

    public static class DoubleExtensions
    {
        public static INFO Info(this IDouble @this)
        {
            return null;
        }
    }

    public abstract class DoubleI : IDouble
    {
        private readonly INFO info;
        public readonly SPRITE Icon;

        public DoubleI(ICharSequence name, ICharSequence desc)
        {
            info = new INFO(name, desc);
            Icon = UI.Icons().S.Cancel;
        }

        public DoubleI(ICharSequence name, ICharSequence desc, SPRITE icon)
        {
            info = new INFO(name, desc);
            Icon = icon;
        }

        public INFO Info()
        {
            return info;
        }
    }

    public class DoubleImp : IDoubleMutable, SAVABLE
    {
        private double d;
        public INFO Info;

        public double GetD()
        {
            return d;
        }

        public void Save(FilePutter file)
        {
            file.D(d);
        }

        public void Load(FileGetter file)
        {
            d = file.D();
        }

        public void Clear()
        {
            d = 0;
        }

        public IDoubleMutable SetD(double d)
        {
            this.d = d;
            return this;
        }

        public INFO Info()
        {
            return Info;
        }
    }
}