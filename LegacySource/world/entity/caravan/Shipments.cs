using System;
using System.Collections.Generic;
using System.IO;
using init.paths;
using init.sprite.UI;
using init.trade;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.spritecomposer;
using view.main;
using world;
using world.entity;

namespace world.entity.caravan
{
    public sealed class Shipments : WEntityConstructor<Shipment>
    {
        private readonly Stack<Shipment> free = new Stack<Shipment>(1024);
        public readonly SPRITE icon;

        private readonly TILE_SHEET caravan = new ITileSheet(PATHS.SPRITE().getFolder("world").getFolder("entity").get("Tribute"), 100, 224)
        {
            protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.singles.init(0, 0, 1, 12, 2, 1, d.s16);
                for (int i = 0; i < 12; i++)
                {
                    s.singles.setVar(i);
                    s.singles.paste(3, true);
                }
                return d.s16.saveGame();
            }
        }.get();

        public Shipments(LISTE<WEntityConstructor<?>> tot) : base(tot, true)
        {
            icon = new SPRITE.Imp(Icon.L, Icon.L)
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    int i = (int)(VIEW.renderSecond() * 2) % 3;
                    i *= 8;
                    i += 8 * 4;
                    COLOR.WHITE150.bind();
                    WORLD.ENTITIES().caravans.caravan.render(r, i + DIR.SE.id(), X1, X2, Y1, Y2);
                    COLOR.unbind();
                }
            };
        }

        public Shipment create(Region start, Region dest, TRADE_TYPE type)
        {
            COORDINATE cc = WORLD.PATH().rnd(start);
            if (cc != null)
            {
                return create(cc.x(), cc.y(), dest, type);
            }
            return null;
        }

        public Shipment create(int sx, int sy, Region dest, TRADE_TYPE type)
        {
            Shipment c = create();
            c.add(sx, sy, dest.faction(), type);
            if (c.added())
                return c;

            free.push(c);
            return null;
        }

        protected override Shipment create()
        {
            if (free.Count > 0)
            {
                return free.pop();
            }
            return new Shipment();
        }

        protected override void clear()
        {
            // TODO Auto-generated method stub
        }
    }
}