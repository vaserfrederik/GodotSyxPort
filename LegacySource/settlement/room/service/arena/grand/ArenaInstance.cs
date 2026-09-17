using System;
using settlement.main;
using game;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.service.arena.grand
{
    [Serializable]
    internal sealed class ArenaInstance : RoomInstance, ROOM_SERVICER
    {
        private const long serialVersionUID = 1L;
        public const int CHEER_TIME = TIME.secondsPerDay() / 128;
        private readonly RoomServiceInstance service;
        private readonly RECTANGLE arena;
        private byte executions = 0;
        private int cheerTime;
        private bool cheer;

        protected ArenaInstance(ROOM_ARENA b, TmpArea area, RoomInit init, RECTANGLE aa)
            : base(b, area, init)
        {
            arena = aa;

            int ww = 0;
            int ss = 0;

            GAME.Notify("here");

            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    if (b.constructor.util.tile(c.x(), c.y()) == b.constructor.util.iArena)
                        ww++;
                    if (b.constructor.util.service(c.x(), c.y()))
                        ss++;
                }
            }

            service = new RoomServiceInstance(ss, blueprintI().data);
            employees().maxSet(ww / 6);
            employees().neededSet(ww / 6);

            activate();
        }

        protected override void activateAction()
        {
            blueprintI().executions -= executions;
            blueprintI().executionsMax += 4;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().ser.get(c.x(), c.y()) != null)
                {
                    blueprintI().ser.findableReserveCancel();
                }
            }
        }

        protected override void deactivateAction()
        {
            blueprintI().executions += executions;
            blueprintI().executionsMax -= 4;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().ser.get(c.x(), c.y()) != null && blueprintI().ser.findableReservedCanBe())
                {
                    blueprintI().ser.findableReserve();
                }
            }
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
            {
                service.updateDay();
            }
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, (double)employees().employed() / employees().max());
        }

        protected override void dispose()
        {
        }

        public ROOM_ARENA blueprintI()
        {
            return (ROOM_ARENA)blueprint();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            i.lit();
            FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
            return s.sprite.render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(), false);
        }

        protected override bool renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
            s.sprite.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
            return false;
        }

        protected override bool renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
            s.sprite.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
            return false;
        }

        protected override AVAILABILITY getAvailability(int tile)
        {
            FurnisherItemTile it = blueprintI().constructor.util.tile(tile);
            if (it == null)
                return AVAILABILITY.ROOM;
            return it.availability;
        }
    }
}