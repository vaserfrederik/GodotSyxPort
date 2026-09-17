using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game.faction;
using game.faction.diplomacy;
using game.time;
using init.sprite.UI.Icons;
using init.sprite.UI;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;
using world;
using world.WORLD;

namespace world.log
{
    public sealed class WorldLog : WorldResource
    {
        private static readonly string ¤¤war = "The {0} declares war on {1}";

        static WorldLog()
        {
            D.ts(typeof(WorldLog));
        }

        public WorldLog() : base("log", "WLOGs")
        {
            new DipActivityListener
            {
                Change = (faction, other, old, nn) =>
                {
                    if (nn == DIP.WAR())
                    {
                        Str.TMP.Clear();
                        Str.TMP.Add(¤¤war);
                        Str.TMP.Insert(0, faction.name);
                        Str.TMP.Insert(1, other.name);
                        WORLD.LOG().log(faction, other, UI.icons().s.sword, Str.TMP, faction.cx(), faction.cy());
                    }
                }
            };
        }

        public const int MAX = 256;
        private readonly ArrayList<LogEntry> all = new ArrayList<LogEntry>(MAX);

        private LogEntry next()
        {
            if (!all.HasRoom())
            {
                LogEntry e = all.Get(0);
                all.ShiftLeft();
                return e;
            }
            return new LogEntry();
        }

        public void log(Faction a, Faction b, IconS icon, string message, int tx, int ty)
        {
            int day = TIME.days().bitsSinceStart();
            short fa = (short)(a == null ? -1 : a.index());
            short fb = (short)(b == null ? -1 : b.index());
            short ii = (short)(icon == null ? -1 : icon.index);

            for (int i = all.size() - 1; i >= 0; i--)
            {
                LogEntry o = all.Get(i);
                if (o.day != day)
                    break;
                if (o.ii == ii && o.fa == fa && o.fb == fb && o.message.Equals(message))
                    return;
            }

            LogEntry e = next();
            e.ii = ii;
            e.day = day;
            e.fa = fa;
            e.fb = fb;
            e.tx = (short)tx;
            e.ty = (short)ty;
            e.message.Clear().Add(message);
            all.Add(e);
        }

        public LIST<LogEntry> all()
        {
            return all;
        }

        private WorldResourceManager saver = new WorldResourceManager
        {
            Save = file =>
            {
                file.i(all.size());
                foreach (LogEntry e in all)
                {
                    e.save(file);
                }
            },

            Load = file =>
            {
                int am = file.i();
                all.Clear();
                for (int i = 0; i < am; i++)
                {
                    all.Add(new LogEntry(file));
                }
            },

            Clear = () => all.ClearSloppy()
        };

        public override WorldResourceManager saver()
        {
            return saver;
        }
    }
}