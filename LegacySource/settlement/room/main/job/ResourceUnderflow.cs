using System;
using System.IO;
using System.Linq;

namespace Settlement.Room.Main.Job
{
    public class ResourceUnderflow : SAVABLE
    {
        public int[] underflow = Alloc.Ii(RESOURCES.ALL().Count());

        public override void Save(FilePutter file)
        {
            RESOURCES.Map().Saver().Save(underflow, file);
        }

        public override void Load(FileGetter file)
        {
            RESOURCES.Map().Loader().Load(underflow, file, 0);
        }

        public override void Clear()
        {
            underflow = underflow.Select(x => 0).ToArray();
        }

        public int Withdraw(RESOURCE res, int target, int max)
        {
            if (target > max)
            {
                underflow[res.Index()] += target - max;
                return max;
            }
            return target;
        }

        public int Deposit(RESOURCE res, int amount)
        {
            int u = underflow[res.Index()];
            if (u > 0)
            {
                int a = Math.Min(amount, u);
                underflow[res.Index()] -= a;
                return amount - a;
            }
            return amount;
        }
    }
}