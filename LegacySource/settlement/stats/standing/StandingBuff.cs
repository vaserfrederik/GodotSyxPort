using System;
using System.IO;
using System.Linq;

namespace Settlement.Stats.Standing
{
    public class StandingBuff
    {
        private static readonly string ¤¤Emmergency = "Emergency";

        static StandingBuff()
        {
            D.ts(typeof(StandingBuff));
        }

        public readonly double Time = TIME.SecondsPerDay() * 8;
        public readonly double TimerI = 1.0 / Time;
        public readonly double Add = 1;
        private double[] Timer = new double[HCLASSES.ALL().Count()];

        public StandingBuff()
        {
            BValue v = new BValue
            {
                vGet = (FactionNPC f) => 0,
                vGetP = (Player f) => vGet(HCLASS_RACE.clP()),
                vGetDiv = (Div div) => 0,
                vGetIndu = (Induvidual indu) => Math.Clamp(Timer[indu.HType().CLASS.Index()] * TimerI, 0, 1),
                vGetReg = (Region reg) => 0,
                vGetHCR = (HCLASS_RACE t) => t.cl != null ? Math.Clamp(Timer[t.cl.Index()] * TimerI, 0, 1) : 0
            };

            new BoosterValue(v, new BSourceInfo(¤¤Emmergency, UI.Icons().S.Alert), Add, false).AddRet(BOOSTABLES.BEHAVIOUR().LOYALTY).Add(BOOSTABLES.BEHAVIOUR().SUBMISSION);
        }

        public void Update(double ds)
        {
            for (int i = 0; i < HCLASSES.ALL().Count(); i++)
            {
                Timer[i] = Math.Clamp(Timer[i] - ds, 0, Time * 4);
            }
        }

        public void Execute(HCLASS cl, double time)
        {
            Timer[cl.Index()] = Math.Max(Timer[cl.Index()], time);
        }

        private readonly SAVABLE Saver = new SAVABLE
        {
            Save = (FilePutter file) => file.DsE(Timer),
            Load = (FileGetter file) => Timer = file.DsE(new double[HCLASSES.ALL().Count()]),
            Clear = () => Timer = new double[HCLASSES.ALL().Count()]
        };

        public static void FakeLoad(FileGetter file)
        {
            file.DsE(new double[HCLASSES.ALL().Count()]);
        }
    }
}