using System;
using System.Collections.Generic;
using game;
using game.raiding;
using init.sprite.UI;
using snake2d.util.datatypes;
using util.text;
using view.ui.manage;

namespace view.ui.raider
{
    public sealed class UIRaiding : IFullView
    {
        static bool debug;
        public static string ¤¤name = "Raiders";

        static UIRaiding()
        {
            D.ts(typeof(UIRaiding));
        }

        //final Army army = new Army();

        public UIRaiding() : base(Dic.¤¤Raiding, UI.icons().l.rebel)
        {
            section.body().setWidth(WIDTH);
            section.body().setHeight(1);

            section.addRelBody(2, DIR.S, new Info());

            Current c = new Current(HEIGHT - section.getLastY2() - 8);
            List l = new List(c, HEIGHT - section.getLastY2() - 8);
            l.addRelBody(8, DIR.E, c);

            section.addRelBody(16, DIR.S, l);
        }

        public override void activate()
        {
            base.activate();
        }

        private static int ci = -1;
        private static int vi = -1;

        static bool statsVisible(Raider r)
        {
            if (r.defeated)
                return true;
            if (r.raids > 0)
                return true;
            if (UIRaiding.debug)
                return true;
            if (r.isScared())
                return true;
            if (r.hasInterrest())
                return true;
            if (ci != GAME.updateI())
            {
                ci = GAME.updateI();
                foreach (Raider rr in GAME.raiders().ALL())
                {
                    if (!rr.defeated && !rr.isScared() && !rr.hasInterrest())
                    {
                        vi = rr.bounty;
                        break;
                    }
                }
            }
            return r.bounty == vi;
        }

        static bool portVisible(Raider r)
        {
            if (r.defeated)
                return true;
            if (r.raids > 0)
                return true;
            if (UIRaiding.debug)
                return true;
            return false;
        }
    }
}