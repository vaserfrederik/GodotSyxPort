using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Law.Guard
{
    public class GuardPower : SAVABLE
    {
        private Div d;
        private readonly DIV_SPEC spec = new DIV_SPEC();

        public GuardPower()
        {
            spec.Race = () => d.Race();
            spec.Training = (StatTraining tr) => tr.Stat.Div().GetD(d);
            spec.Equip = (EquipBattle e) => Math.Clamp((double)e.Stat().Div().Get(d) / STATS.POP().Pop(HTYPES.SOLDIER(), d), 0, 1);
            spec.Men = () => STATS.POP().Pop(HTYPES.GUARD(), d);
            spec.Faction = () => FACTIONS.Player();
            spec.Experience = () => STATS.BATTLE().COMBAT_EXPERIENCE.Div().GetD(d);
            spec.Name = () => null;
            spec.BannerI = () => 0;
        }

        private double res = 0;
        private double resD = 0;
        private int upI = -1;
        private int DI = 0;

        public double Get()
        {
            if (upI == GAME.UpdateI())
                return res;
            upI = GAME.UpdateI();
            if (DI >= GAME.ARMIES().Player().Divisions().Count)
            {
                DI = 0;
                res = resD;
                resD = 0;
            }

            Div d = GAME.ARMIES().Player().Divisions()[DI];
            if (SETT.ROOMS().GUARD.ActiveDuty.Is(d) && STATS.POP().Pop(HTYPES.GUARD(), d) > 0)
            {
                this.d = d;
                resD += GAME.Battle().Power.Get(spec);
            }
            DI++;

            return res;
        }

        public void Save(FilePutter file)
        {
            file.d(res);
        }

        public void Load(FileGetter file)
        {
            res = file.d();
        }

        public void Clear()
        {
            res = 0;
            resD = 0;
        }
    }
}