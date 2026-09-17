using System;
using System.Collections.Generic;
using game.battle.util;
using game.raiding;
using init.constant;
using init.race;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using world.army;

namespace view.battle.editor
{
    internal sealed class ArmySide
    {
        public ArrayList<DIV_SPEC> Divs = new ArrayList<DIV_SPEC>(Config.Battle().DivisionsPerArmy);
        public int[] Artillery = Alloc.Ii(AD.Supplies().Arts().Size());

        public void Generate(double power)
        {
            RaiderArmy p = new RaiderArmy(RACES.Playable().Rnd(), power, RND.rFloat());
            Divs.ClearSloppy();
            foreach (DIV_SPEC d in p.Sdivs)
                Divs.Add(d);

            for (int i = 0; i < Artillery.Length; i++)
                Artillery[i] = p.Artillery[i];
        }

        public void Clear()
        {
            Divs.ClearSloppy();
            for (int i = 0; i < Artillery.Length; i++)
                Artillery[i] = 0;
        }
    }
}