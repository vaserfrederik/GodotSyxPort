using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using game.boosting;
using game.faction;
using init.type;
using init.value;
using settlement.room.main.category;
using settlement.room.main.employment;
using settlement.room.main.util;
using snake2d;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.text;

namespace settlement.room.main
{
    public abstract class RoomBlueprintIns<T> : RoomBlueprintImp where T : RoomInstance
    {
        static ArrayListGrower<RoomBlueprintIns<T>> INS = new ArrayListGrower<RoomBlueprintIns<T>>();
        static RoomBlueprintIns()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    INS.Clear();
                }
            };
        }

        private readonly ArrayListResize<T> all = new ArrayListResize<T>(128);

        private int totalArea = 0;
        int upgrades = 0;
        int averageDegrade = 0;
        private readonly RoomEmploymentSimple employment;
        int roomNameI = 1;
        private long[] stats = new long[32];
        private static long statL = 1000;
        private static readonly CharSequence ¤¤Desc = "¤Production speed of: {0}";

        static RoomBlueprintIns()
        {
            D.ts(typeof(RoomBlueprintIns<T>));
        }

        protected RoomBlueprintIns(int typeIndex, RoomInitData data, string key, RoomCategorySub cat, ACTION wiki) : base(data, typeIndex, key, cat, wiki)
        {
            if (data.data().has("WORK"))
                employment = new RoomEmployment(this, data);
            else if (data.data().has("EMPLOYMENT"))
                employment = new RoomEmploymentSimple("EMPLOYMENT", this, data);
            else
                employment = null;
            INS.add(this);
            string vKey = ("ROOM_" + key).Replace("__", "_");

            GVALUES.FACTION.push(vKey + "_AMOUNT", Dic.¤¤Amount + ": " + info.names, iconBig(), new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return instancesSize();
                }
            }, false);

            GVALUES.FACTION.push(vKey + "_AREA", Dic.¤¤Area + ": " + info.names, iconBig(), new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return totalArea;
                }
            }, false);
        }

        protected RoomBlueprintIns(int typeIndex, RoomInitData data, string key, RoomCategorySub cat) : this(typeIndex, data, key, cat, null)
        {
        }

        protected Boostable pushBo(Json json, CharSequence name, CharSequence desc, string type, bool upgrades)
        {
            return pushBo(json, name, desc, type, upgrades, 1.0);
        }

        protected Boostable pushBo(Json json, CharSequence name, CharSequence desc, string type, bool upgrades, double value)
        {
            if (bonus != null)
                throw new RuntimeException();
            bonus = BOOSTING.push(key, value, name, desc, icon, BOOSTABLES.ROOMS());

            if (json.has("BONUS"))
            {
                json = json.json("BONUS");
                CLIMATES.pushBonuses(json, bonus);
            }
            if (upgrades)
                this.upgrades().pushBonus(this, bonus);

            return bonus;
        }

        protected Boostable pushBo(Json json, string type, bool upgrades)
        {
            string desc = "" + new Str(¤¤Desc).insert(0, info.names);
            return pushBo(json, info.names, desc, type, upgrades);
        }

        protected void removeInstance(RoomInstance rem)
        {
            totalArea -= rem.area();
            if (degrades())
                averageDegrade -= (int)Math.Ceiling(100 * rem.getDegrade());
            upgrades -= rem.upgrade() * rem.area();
            all.removeOrdered((T)rem);
            for (int i = 0; i < constructor().stats().size(); i++)
            {
                this.stats[i] -= (long)(rem.stat(i) * statL);
            }
        }

        protected void addInstance(RoomInstance t)
        {
            totalArea += t.area();
            if (degrades())
                averageDegrade += (int)Math.Ceiling(100 * t.getDegrade());
            upgrades += t.upgrade() * t.area();
            all.add((T)t);
            for (int i = 0; i < constructor().stats().size(); i++)
            {
                this.stats[i] += (long)(t.stat(i) * statL);
            }
        }

        protected override void save(FilePutter saveFile)
        {
            saveFile.@object(all);
            saveFile.i(roomNameI);
            saveFile.i(totalArea);
            saveFile.i(averageDegrade);
            saveFile.i(upgrades);
            saveFile.ls(stats);
            int pos = saveFile.getPosition();
            saveFile.i(0);
            saveP(saveFile);
            saveFile.setAtPosition(pos, saveFile.getPosition() - pos - 4);
        }

        protected void load(FileGetter saveFile) throws IOException
        {
            all.clear();
            object a = saveFile.@object(true);
            roomNameI = saveFile.i();
            totalArea = saveFile.i();
            averageDegrade = saveFile.i();
            upgrades = saveFile.i();
            saveFile.ls(stats);

            if (a != null)
                all.add((ArrayListResize<T>)a);
            else
                clear();

            int le = saveFile.i();
            int pos = saveFile.getPosition();
            loadP(saveFile);
            if (saveFile.getPosition() - le != pos)
            {
                LOG.ln("room save corrupt in pLoad: " + key);
                saveFile.setPosition(pos + le);
                clearP();
            }
        }

        protected override void clear()
        {
            roomNameI = 1;
            totalArea = 0;
            averageDegrade = 0;
            upgrades = 0;
            Array.Fill(stats, 0);
            all.clear();
            clearP();
        }

        protected abstract void saveP(FilePutter f);

        protected abstract void loadP(FileGetter f) throws IOException;

        protected abstract void clearP();

        public override T get(int tx, int ty)
        {
            Room r = ROOMS().map.get(tx, ty);
            if (r != null && r.blueprint() == this)
                return (T)ROOMS().map.get(tx, ty);
            return null;
        }

        public readonly MAP_OBJECT<T> getter = new MAP_OBJECT<T>()
        {
            public T get(int tile)
            {
                Room r = ROOMS().map.get(tile);
                if (r != null && r.blueprint() == RoomBlueprintIns<T>.this)
                    return (T)r;
                return null;
            }

            public T get(int tx, int ty)
            {
                if (IN_BOUNDS(tx, ty))
                    return get(tx + ty * TWIDTH);
                return null;
            }
        };

        public T getInstance(int nr)
        {
            return all.get(nr);
        }

        public int instancesSize()
        {
            return all.size();
        }

        public LIST<T> all()
        {
            return all;
        }

        public RoomEmployment employmentExtra()
        {
            if (employment is RoomEmployment)
                return (RoomEmployment)employment;
            return null;
        }

        public override RoomEmploymentSimple employment()
        {
            return employment;
        }

        public bool degrades()
        {
            return true;
        }

        public int totalArea()
        {
            return totalArea;
        }

        public double degradeAverage()
        {
            if (instancesSize() == 0)
                return 0;
            return averageDegrade / (100.0 * instancesSize());
        }

        public double averageUpgrade()
        {
            if (totalArea == 0)
                return 0;
            return (double)upgrades / totalArea;
        }

        public override bool isAvailable(CLIMATE c)
        {
            return true;
        }

        public double getStat(int statIndex)
        {
            if (instancesSize() == 0)
            {
                this.stats[statIndex] = 0;
                return 0;
            }
            return this.stats[statIndex] / ((double)statL * instancesSize());
        }
    }
}