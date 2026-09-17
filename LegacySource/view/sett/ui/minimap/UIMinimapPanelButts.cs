using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util.colors;
using util.data.GETTER;
using util.gui.common;
using util.gui.misc;
using util.text;

namespace view.sett.ui.minimap
{
    public class UIMinimapPanelButts
    {
        private readonly GuiSection section = new GuiSection
        {
            render = (r, ds) =>
            {
                GCOLOR.UI().panBG.render(r, body());
                base.render(r, ds);
                GCOLOR.UI().borderH(r, body(), 0);
            }
        };

        private readonly GuiSection pbuttons = new GuiSection();

        private static readonly CharSequence ¤¤HideUI = "¤Cinematic mode + Hide UI. Cancel by right click or ESC.";
        private static readonly CharSequence ¤¤ToggleOverlay = "¤Toggle Overlay: ";

        static UIMinimapPanelButts()
        {
            D.ts(typeof(UIMinimapPanelButts));
        }

        public UIMinimapPanelButts(UIMiniMapSettView view, UIMinimapPanel panel, GameWindow w)
        {
            section.body().setDim(panel.body().width(), 36);
            pbuttons.body().centerIn(section.body());
            section.add(pbuttons);
            section.body().moveX2(C.WIDTH());
            section.body().moveY1(panel.body().y2());

            GButt b;

            b = new Butt(SPRITES.icons().s.minimap)
            {
                clickA = () => view.show()
            };
            padd(KeyButt.wrap(b, KEYS.MAIN().MINIMAP));

            b = new Butt(SPRITES.icons().s.minifier)
            {
                clickA = () =>
                {
                    if (w.zoomout() < w.zoomoutmax())
                        w.setZoomout(w.zoomout() + 1);
                },
                renAction = () => activeSet(w.zoomout() < w.zoomoutmax())
            };
            padd(KeyButt.wrap(b, KEYS.MAIN().ZOOM_OUT));

            b = new Butt(SPRITES.icons().s.magnifier)
            {
                clickA = () =>
                {
                    if (w.zoomout() > 0)
                        w.setZoomout(w.zoomout() - 1);
                },
                renAction = () => activeSet(w.zoomout() > 0)
            };
            padd(KeyButt.wrap(b, KEYS.MAIN().ZOOM_IN));
        }

        public void addScreenshot(string savekey)
        {
            GButt b;

            b = new Butt(SPRITES.icons().s.camera)
            {
                clickA = () => CORE.getGraphics().makeScreenShot()
            };
            padd(KeyButt.wrap(b, KEYS.MAIN().SCREENSHOT));

            b = new Butt(SPRITES.icons().s.cameraBig)
            {
                sst = new SuperSc("SUPER_CITY", new SUPER_SCREENSHOT[] { new Shot(4, 2), new Shot(2, 2), new Shot(2, 1) }, savekey),
                clickA = () => VIEW.inters().popup.show(sst, this, true)
            };
            b.hoverInfoSet(SuperSc.¤¤name);
            padd(b);

            b = new Butt(SPRITES.icons().s.cancel)
            {
                clickA = () => VIEW.hide()
            };
            b.hoverInfoSet(¤¤HideUI);
            padd(b);
        }

        public GETTER_IMP<Addable> addOverlays()
        {
            GuiSection s = new GuiSection();

            GETTER_IMP<Addable> thing = new GETTER_IMP<Addable>();

            int i = 0;
            List<Addable> aa = new List<Addable>(SETT.OVERLAY().all());
            aa.Sort((arg0, arg1) => ("" + arg0.name).CompareTo("" + arg1.name));
            foreach (Addable a in aa)
            {
                if (a.key != null)
                {
                    CLICKABLE cc = ontop(a, thing);
                    s.add(cc, (i % 2) * cc.body().width(), (i / 2) * cc.body().height());
                    i++;
                }
            }

            CLICKABLE c = new Butt(SPRITES.icons().s.eye)
            {
                clickA = () => VIEW.inters().popup.show(s, this),
                renAction = () =>
                {
                    if (hoveredIs() && MButt.RIGHT.consumeClick())
                        thing.set(null);

                    if (thing.get() != null)
                        thing.get().add();

                    selectedSet(thing.get() != null);
                }
            }.hoverInfoSet(Dic.¤¤Overlays);

            padd(c);
            return thing;
        }

        private CLICKABLE ontop(Addable add, GETTER_IMP<Addable> thing)
        {
            ACTION a = new ACTION
            {
                exe = () =>
                {
                    if (thing.get() == add)
                        thing.set(null);
                    else
                        thing.set(add);
                }
            };

            GButt.ButtPanel c = new GButt.ButtPanel(UI.FONT().H2.getText(add.name))
            {
                clickA = () => a.exe(),
                renAction = () => selectedSet(thing.get() == add)
            };
            c.setDim(250, 30).align(DIR.W).hoverTitleSet(add.name).hoverInfoSet(add.desc);
            c.icon(add.icon.resized(IconS.L));

            if (add == SETT.OVERLAY().ROOM_PROBLEM)
            {
                return KeyButt.wrap(a, c, KEYS.SETT(), "TOGGLE_OVERLAY_" + add.key, add.name, ¤¤ToggleOverlay + " " + add.name, KEYCODES.KEY_SPACE, -1);
            }

            return KeyButt.wrap(a, c, KEYS.SETT(), "TOGGLE_OVERLAY_" + add.key, add.name, ¤¤ToggleOverlay + " " + add.name);
        }

        private class Shot : SUPER_SCREENSHOT
        {
            private readonly int zoomout;
            private readonly int winW;
            private readonly int winH;
            private Rec current;

            public Shot(int scale, int zoomout) : base(scale)
            {
                this.zoomout = zoomout;
                winW = (C.WIDTH()) << zoomout;
                winH = (C.HEIGHT()) << zoomout;
                current = new Rec(winW, winH);
            }

            public override bool renderAndHasNext()
            {
                if (current.y1() >= SETT.PHEIGHT)
                    return false;

                GAME.s().render(CORE.renderer(), 0, zoomout, current, 0, 0, UIMinimapSettConfig.NORMAL);
                current.incrX(winW);
                if (current.x1() >= SETT.PWIDTH)
                {
                    current.incrY(winH);
                    current.moveX1(0);
                }
                return true;
            }

            public override int getWidth() => SETT.PWIDTH >> zoomout;

            public override int getHeight() => SETT.PHEIGHT >> zoomout;

            public override void init() => current.set(0, winW, 0, winH);
        }

        private void padd(CLICKABLE cl)
        {
            pbuttons.addRelBody(0, DIR.W, cl);
            pbuttons.body().moveX1(section.body().x1() + 4);
            pbuttons.body().moveCY(section.body().cY());
        }
    }
}