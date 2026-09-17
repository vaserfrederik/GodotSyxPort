using System;
using System.Collections.Generic;
using init.race;
using init.type;
using settlement.main;
using settlement.room.water.pool;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.stat;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;

namespace view.sett.ui.standing.Cats
{
    final class CatEnv : Cat
    {
        public CatEnv(HCLASS cl, GETTER<Race> race) : base(new StatCollection[] { STATS.ENV(), STATS.ACCESS(), STATS.ACCESS().ACCESS, STATS.ACCESS().MONUMENTS, STATS.BATTLE(), STATS.STORED() })
        {
            titleSet(cs[0].info.name);
            
            LinkedList<RENDEROBJ> rens = new LinkedList<RENDEROBJ>();
            
            foreach (StatCollection c in cs)
            {
                if (c.all().size() == 0)
                    continue;
                if (c == STATS.ACCESS().MONUMENTS)
                {
                    rens.add(new StatRow.Title(c.info));
                    foreach (StatMonument s in STATS.ACCESS().MONUMENTS.ALL())
                    {
                        rens.add(new StatRow(s, cl, race));
//                        if (s.statUpgrade != null)
//                            rens.add(new StatRow(s.statUpgrade, cl, race));
                    }
                }
                else if (c != STATS.STORED())
                {
                    rens.add(new StatRow.Title(c.info));
                    foreach (STAT s in c.all())
                    {
                        if (s == STATS.ENV().BUILDING_PREF)
                        {
                            rens.add(new StatRow(s, cl, race)
                            {
                                
                                public override void hoverInfoGet(GUI_BOX text)
                                {
                                    
                                    base.hoverInfoGet(text);
                                    if (race.get() != null)
                                    {
                                        GBox b = (GBox)text;
                                        b.NL(8);
                                        foreach (BUILDING_PREF p in BUILDING_PREFS.ALL())
                                        {
                                            b.add(p.icon());
                                            b.add(GFORMAT.perc(b.text(), race.get().pref().structure(p)));
                                        }
                                    }
                                }
                                
                            });
                        }
                        else if (s == STATS.ENV().POOL_PREF)
                        {
                            rens.add(new StatRow(s, cl, race)
                            {
                                
                                public override void hoverInfoGet(GUI_BOX text)
                                {
                                    
                                    base.hoverInfoGet(text);
                                    if (race.get() != null)
                                    {
                                        GBox b = (GBox)text;
                                        b.NL(8);
                                        foreach (ROOM_POOL p in SETT.ROOMS().POOLS)
                                        {
                                            b.add(p.icon);
                                            b.add(GFORMAT.perc(b.text(), race.get().pref().pool(p)));
                                        }
                                    }
                                }
                                
                            });
                            
                            
                        }
                        else if (s.info().matters())
                        {
                            rens.add(new StatRow(s, cl, race));
                        }
                    }
                }
            }
            
            rens.add(new StatRow.Title(STATS.STORED().info));
            
            foreach (STAT s in STATS.STORED().createTheOnesThatMatter(cl))
            {
                rens.add(new StatRow(s, cl, race));
            }

            section.addDown(4, new GScrollRows(rens, HEIGHT, 0).view());
            
        }
    }
}