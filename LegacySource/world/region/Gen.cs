using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using world.WORLD;
using world.map.pathing;
using world.map.regions;

namespace world.region
{
    class Gen
    {
        private readonly WRegFinder rr = new WRegFinder();
        private const int aveSize = 8;
        private Bitmap1D handsOff = new Bitmap1D(WREGIONS.MAX, false);

        public Gen(RDInit init, ACTION loadprint)
        {
            loadprint.exe();

            if (!WORLD.REGIONS().player.active())
                return;

            WORLD.RD().saver().clear();

            while (FACTIONS.NPCs().size() > 0)
            {
                FACTIONS.remove(FACTIONS.NPCs().get(0), false);
            }

            RD.PROSPECT().generate();
            loadprint.exe();
            generatePlayer(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy());
            generateKingdoms();
        }

        private void generatePlayer(int playerX, int playerY)
        {
            Region r = WORLD.REGIONS().map.get(playerX, playerY);
            r.fationSet(FACTIONS.player(), false);
            r.setCapitol();
            r.info.name().clear().add(FACTIONS.player().name);

            {
                LIST<RegDist> dd = rr.all(r, treaty, WRegSel.DUMMY(r));
                if (dd.size() > 2)
                {
                    handsOff.set(dd.rnd().reg.index(), true);
                }
            }

            int[] size = new int[] { 1, 3, 5 };

            foreach (int s in size)
            {
                LIST<RegDist> ddd = rr.all(r, treaty, WRegSel.DUMMY(r));

                int am = 0;
                foreach (RegDist d in ddd)
                {
                    if (d.reg.faction() != null)
                        continue;
                    if (handsOff.get(d.reg.index()))
                        continue;
                    if (!create(d.reg))
                    {
                        break;
                    }
                    ((FactionNPC)d.reg.faction()).sanctified = true;
                    spread(d.reg, RND.rInt(s));
                    am++;
                    if (am >= 2 + s / 2)
                        break;
                }
            }
        }

        private bool create(Region reg)
        {
            FactionNPC f = FACTIONS.activateNext(reg, null, false);
            return f != null;
        }

        private void generateKingdoms()
        {
            SPRITES.loader().init();
            ArrayList<Region> regs = new ArrayList<>(WORLD.REGIONS().active());
            regs.shuffle();

            int amount = 3 * regs.size() / 4;

            while (amount > 0 && regs.size() > 0)
            {
                Region r = regs.removeLast();
                if (r.faction() != null)
                    continue;
                if (handsOff.get(r.index()))
                    continue;
                FactionNPC f = FACTIONS.activateNext(r, null, false);
                if (f == null)
                {
                    break;
                }
                amount -= spread(r);
            }
        }

        private int spread(Region home)
        {
            int amount = RND.rInt(aveSize * 2);
            home.fationSet(home.faction(), false);
            LIST<RegDist> ddd = rr.all(home, treaty, WRegSel.DUMMY(home));

            int k = 1;
            for (int i = 0; i < amount && i < ddd.size(); i++)
            {
                ddd.get(i).reg.fationSet(home.faction(), false);
                k++;
            }
            return k;
        }

        private int spread(Region home, int amount)
        {
            home.fationSet(home.faction(), false);
            LIST<RegDist> ddd = rr.all(home, treaty, WRegSel.DUMMY(home));
            int k = 1;
            for (int i = 0; i < amount && i < ddd.size(); i++)
            {
                ddd.get(i).reg.fationSet(home.faction(), false);
                k++;
            }
            return k;
        }

        private readonly Treaty treaty = new Treaty()
        {
            public bool can(Region origin, Region prevReg, Region to, int tx, int ty, double dist)
            {
                if (to == null)
                    return true;
                if (to.faction() == null)
                    return true;
                if (to != null && to.faction() != origin.faction())
                    return false;
                if (handsOff.get(to.index()))
                    return false;
                return true;
            }
        };
    }
}