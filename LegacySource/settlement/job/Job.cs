using System;
using System.Collections.Generic;
using settlement.main;
using game;
using init.resources;
using init.sprite;
using settlement.entity.humanoid;
using settlement.job.StateManager;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.keymap;
using util.rendering;
using util.text;
using view.tool;

namespace settlement.job
{
    public abstract class Job : SETT_JOB, MAPPED
    {
        public static bool Overwrite;

        static readonly int NOTHING = 0;
        static readonly ArrayListGrower<Job> all = new ArrayListGrower<Job>();
        static Job()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    all.Clear();
                    Overwrite = false;
                }
            };
        }

        readonly byte index = (byte)all.Add(this);
        readonly Coo coo = new Coo();
        int tile;
        readonly CharSequence name;
        readonly SPRITE icon;
        private readonly string key;

        protected Job(string key, CharSequence name, SPRITE icon)
        {
            this.name = name;
            this.icon = icon;
            this.key = "JOB_" + key;
        }

        protected bool Get(int tx, int ty)
        {
            coo.Set(tx, ty);
            tile = tx + ty * SETT.TWIDTH;
            return true;
        }

        public final int Tile()
        {
            return tile;
        }

        void Cancel(int tx, int ty)
        {
        }

        public final void JobReserve(RESOURCE r)
        {
            if (!JobReserveCanBe())
            {
                throw new RuntimeException();
            }
            if (r != ResourceCurrentlyNeeded())
                throw new RuntimeException(r + " " + ResourceCurrentlyNeeded());
            JOBS().state.Set(State.RESERVED, this);
        }

        public final bool JobReservedIs(RESOURCE r)
        {
            return JOBS().state.Is(coo, State.RESERVED) && r == ResourceCurrentlyNeeded();
        }

        public final void JobReserveCancel(RESOURCE r)
        {
            if (JOBS().state.Is(coo, State.RESERVED))
                JOBS().state.Set(State.RESERVABLE, this);
        }

        public final bool JobReserveCanBe()
        {
            return JOBS().state.Get(coo) == State.RESERVABLE;
        }

        public COORDINATE JobCoo()
        {
            return coo;
        }

        abstract protected void RenderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i, int state);

        abstract protected void RenderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty);

        protected void ExtraHovInfo(GBox box)
        {
        }

        abstract protected void Init(int tx, int ty);

        abstract protected bool BecomesSolidNext();

        public abstract RESOURCE ResourceCurrentlyNeeded();

        public abstract int ResAmount();

        public RESOURCE Res()
        {
            return null;
        }

        public final RBIT JobResourceBitToFetch()
        {
            if (ResourceCurrentlyNeeded() != null)
                return ResourceCurrentlyNeeded().bit;
            return null;
        }

        public int JobResourcesNeeded(Humanoid skill)
        {
            return 1;
        }

        protected CharSequence Problem(int tx, int ty, bool overwrite)
        {
            if (ROOMS().map.Is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;
            if (BecomesSolid())
            {
                if (SETT.PLACA().willBlock.Is(tx, ty))
                {
                    return PlacableMessages.¤¤BLOCK_WILL;
                }
            }
            if (!overwrite)
            {
                if (JOBS().getter.Is(tx, ty))
                {
                    return PlacableMessages.¤¤JOB_BLOCK;
                }
            }

            return null;
        }

        public CharSequence LockText()
        {
            return null;
        }

        public abstract PlacableMulti Placer();

        public ToolConfig Config()
        {
            return null;
        }

        public bool IsConstruction()
        {
            return false;
        }

        private static readonly CharSequence ¤¤claimed = "Is Claimed";
        private static readonly CharSequence ¤¤claimedNot = "Is Unclaimed";
        private static readonly CharSequence ¤¤resources = "¤This job needs {0} to complete, which is unobtainable in your city.";
        private static readonly CharSequence ¤¤dormant = "¤Job is inactive and needs to be manually activated before it will be performed.";
        private static readonly CharSequence ¤¤blocked = "¤An adjacent job is blocking this job and must be performed prior to this.";
        private static readonly CharSequence ¤¤unreachable = "¤Job is unreachable. It will eventually be performed, but it will be difficult for your subjects.";

        static Job()
        {
            D.Ts(typeof(Job));
        }

        public void Hover(GBox box)
        {
            if (icon != SPRITES.icons().m.questionmark)
            {
                box.Add(icon);
                box.Text(name);
                box.NL(8);
            }

            State state = JOBS().state.Get(tile);

            if (Res() != null)
            {
                int am = ResAmount();
                int n = am - JobResourcesNeeded(null);
                box.SetResource(Res(), n, am);
                box.NL();
            }

            if (state == State.RESERVED)
            {
                box.Add(box.Text().Normalify2().Add(¤¤claimed));
                box.NL();
            }
            else
            {
                box.Add(box.Text().Normalify2().Add(¤¤claimedNot));
                box.NL();
            }

            box.NL(8);
            if (!PATH().reachability.Is(coo))
            {
                box.Add(box.Text().Errorify().Add(¤¤unreachable));
                box.NL();
            }
            if (state == State.DORMANT)
            {
                box.Add(box.Text().Errorify().Add(¤¤dormant));
                box.NL();
            }
            if (state == State.BLOCKED)
            {
                box.Add(box.Text().Errorify().Add(¤¤blocked));
                box.NL();
            }

            RESOURCE res = ResourceCurrentlyNeeded();
            if (res != null && state != State.RESERVED && !PATH().finders.resource.normal.Has(res))
            {
                GText t = box.Text();
                t.Add(¤¤resources);
                t.Insert(0, res.names);
                t.Errorify();

                if (res.specialHelpText != null)
                {
                    t.S();
                    t.Add(res.specialHelpText);
                }
                box.Add(res.icon().big);
                box.Add(t);
                box.NL();
            }

            if (BecomesSolid())
            {
                box.Add(box.Text().Errorify().Add(¤¤unreachable));
                box.NL();
            }
        }

        public bool BecomesSolid()
        {
            return false;
        }

        public bool NeedsRipe()
        {
            return false;
        }

        public void DoSomethingExtraRender()
        {
        }

        public abstract TerrainTile Becomes(int tx, int ty);

        public string Key()
        {
            return key;
        }

        public int Index()
        {
            return index;
        }
    }
}