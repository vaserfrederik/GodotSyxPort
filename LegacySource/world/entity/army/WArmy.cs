using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using snake2d;
using util;
using world.army;
using world.entity;
using world.map.pathing;
using world.map.regions;
using world.map.regions.centre;
using init.constant;
using init.trade;
using game;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty.opinion;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite.text;

namespace world.entity.army
{
    public sealed class WArmy : WEntity
    {
        int iteration = 0;
        short index = -1;
        public readonly Str name = new Str(24);

        private readonly ADDivs army = new ADDivs(this);
        private readonly Treaty treaty = new Treaty()
        {
            public override bool Can(Region origin, Region prevReg, Region to, int tx, int ty, double dist)
            {
                if (to == null)
                    return true;
                if (tx != to.cx() || ty != to.cy())
                    return true;
                if (to.faction() == faction())
                    return true;
                if (to.faction() == null || faction() == null)
                    return false;
                if (DIP.Get(to.faction(), faction()).ally)
                    return true;
                return false;
            }
        };
        private readonly WPath path = new P(treaty);
        static double speed = C.TILE_SIZE * 0.1;
        public static readonly int size = C.TILE_SIZE * 2;
        private byte state = 0;
        short stateShort;
        float stateFloat;

        public static int reinforceTiles = 4;
        private float upD = 0;

        void Init(int tx, int ty, Faction f)
        {
            Body().moveCX(tx * C.TILE_SIZE + C.TILE_SIZEH);
            Body().moveCY(ty * C.TILE_SIZE + C.TILE_SIZEH);
            state = 0;
            iteration++;
            army.Clear();
            path.Clear();
            army.Clear();
            Add();
            if (!Added())
                throw new RuntimeException();
            AD.AddOnlyToBeCalledFromAnArmy(this, f);
        }

        public WArmy() : base(size, size)
        {
        }

        protected override void Save(FilePutter file)
        {
            file.i(iteration);
            file.s(index);
            name.Save(file);
            army.Save(file);
            path.Save(file);
            file.f(stateFloat);
            file.s(stateShort);
            file.b(state);
            file.f(upD);
        }

        protected override WEntity Load(FileGetter file) throws IOException
        {
            iteration = file.i();
            index = file.s();
            name.Load(file);
            army.Load(file);
            path.Load(file);

            stateFloat = file.f();
            stateShort = file.s();
            state = file.b();
            upD = file.f();
            WORLD.ENTITIES().armies.Load(this);
            return this;
        }

        protected override void RemoveAction()
        {
            for (int i = army.Count - 1; i >= 0; i--)
            {
                army.Get(i).Remove();
            }
            path.Clear();
            if (Added())
            {
                Remove();
            }
        }

        protected override void Render(Renderer renderer, float alpha)
        {
            // Implement rendering logic here
        }

        protected override void Update(float delta)
        {
            // Implement update logic here
        }

        protected override void Dispose()
        {
            // Implement dispose logic here
        }

        public override WPath Path()
        {
            return path;
        }

        public WArmyState State()
        {
            return WArmyState.All().Get(state);
        }

        public Region Region()
        {
            return WORLD.REGIONS().map.Get(Body().cx(), Body().cy());
        }

        public double SupplyAmount()
        {
            Region region = Region();
            if (region != null && region.faction() == faction())
                return 1.0;
            return 1.0;
        }

        public bool Recruiting()
        {
            Region region = Region();
            if (region != null && region.faction() == faction())
                return State() == WArmyState.fortified;
            return false;
        }

        public double BesiegeTimer()
        {
            if (State() == WArmyState.besieging)
                return stateFloat;
            return 0;
        }

        public bool Besieging(Region reg)
        {
            return reg != null && reg == Besieging();
        }

        public Region Besieging()
        {
            if (State() != WArmyState.besieging)
                return null;
            Region reg = WORLD.REGIONS().GetByIndex(stateShort);
            if (reg == null)
                return null;

            if (!DIP.WAR().Is(faction(), reg.faction()))
                return null;
            if (AD.Men(null).Get(this) <= 0)
                return null;

            if (path.IsValid())
                return reg.IsBesiegeTile(path.destX(), path.destY()) ? reg : null;
            return reg.IsBesiegeTile(Body().cx(), Body().cy()) ? reg : null;
        }

        public override string ToString()
        {
            return "[" + index + "]" + name + " (" + Body().cx() + "," + Body().cy() + ")";
        }

        private class P : WPath
        {
            private readonly Treaty t;

            public P(Treaty t)
            {
                this.t = t;
            }

            public override Treaty Treaty()
            {
                return t;
            }
        }
    }
}