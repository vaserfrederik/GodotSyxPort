using System;
using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using settlement.main;
using settlement.job;
using settlement.entity.humanoid;
using settlement.tilemap.terrain;
using view.main;
using view.subview;
using view.tool;
using init.resources;
using util.rendering;
using game.audio;
using game.faction;
using init.sprite;

namespace settlement.job
{
    abstract class JobClear : Job
    {
        private readonly Placer placer;
        private readonly CharSequence names;

        JobClear(string key, CharSequence name, CharSequence desc, CharSequence verb, SPRITE icon) : base("CLEAR_" + key, name, icon)
        {
            this.placer = new Placer(this, desc)
            {
                private readonly string jobs = "Jobs: ";

                public override void placeInfo(GBox b, int okTiles, AREA a)
                {
                    base.placeInfo(b, okTiles, a);
                    if (okTiles > 0)
                    {
                        VIEW.hoverBox().add(VIEW.hoverBox().text().add(jobs).add(okTiles));
                    }
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return SETT.JOBS().clearss.butts;
                }

                public override void updateRegardless(GameWindow window, AREA selected)
                {
                    SETT.JOBS().clearss.currentOverlay = overlay();
                    if (overlay() != null && SETT.JOBS().clearss.overlay)
                    {
                        overlay().add();
                    }
                }
            };
            names = verb;
        }

        Addable overlay()
        {
            return null;
        }

        public override int resAmount()
        {
            return 0;
        }

        public override double jobPerformTime(Humanoid skill)
        {
            return 30;
        }

        public override void jobStartPerforming()
        {
        }

        public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm)
        {
            TerrainTile t = TERRAIN().get(coo);
            if (!t.clearing().can())
            {
                PlacerDelete.place(coo.x(), coo.y());
                return null;
            }
            RESOURCE res = t.clearing().clear1(coo.x(), coo.y());
            if (res != null)
            {
                GAME.player().res().inc(res, RTYPE.PRODUCED, 1);
            }
            if (t != TERRAIN().get(coo) || TERRAIN().NADA.is(coo))
            {
                PlacerDelete.place(coo.x(), coo.y());
            }
            else
            {
                JOBS().state.set(State.RESERVABLE, this);
            }

            return res;
        }

        public override CharSequence jobName()
        {
            return names;
        }

        public override bool jobUseTool()
        {
            return true;
        }

        public override SoundRace jobSound()
        {
            return TERRAIN().get(coo).clearing().sound(coo.x(), coo.y());
        }

        protected override void renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i, int state)
        {
        }

        protected override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
        {
            SPRITES.cons().ICO.clear.render(r, x, y);
        }

        protected override void init(int tx, int ty)
        {
            // TODO Auto-generated method stub
        }

        protected override bool becomesSolidNext()
        {
            return false;
        }

        public override PlacableMulti placer()
        {
            return placer;
        }

        public override RESOURCE resourceCurrentlyNeeded()
        {
            return null;
        }

        public override TerrainTile becomes(int tx, int ty)
        {
            return TERRAIN().NADA;
        }
    }
}