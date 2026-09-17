using System.Collections.Generic;
using snake2d;
using util.colors;
using util.gui.misc;
using util.info;
using view.interrupter;
using view.keyboard;
using view.main;
using view.ui;

namespace view.ui.manage
{
    public class IManager
    {
        public static readonly int TOP_HEIGHT = 48;

        private readonly GuiSection top = new GuiSection();
        private IFullView current;
        private readonly Inter inter = new Inter();

        public IManager(UIView view)
        {
            List<IFullView> all = new List<IFullView>
            {
                view.goods,
                view.economy,
                view.tourists,
                view.tech,
                view.raider,
                view.level,
                view.profile
            };

            foreach (IFullView w in all)
            {
                GButt.ButtPanel b = new GButt.ButtPanel(w.icon)
                {
                    ClickA = () => show(w),
                    RenAction = () => selectedSet(w == current)
                };
                b.hoverInfoSet(w.title);
                b.pad(16, 2);
                top.addRightC(0, b);
            }

            top.body().centerX(C.DIM());
            CLICKABLE exit = new GButt.ButtPanel(SPRITES.icons().m.exit)
            {
                ClickA = () => inter.hide(),
                HoverInfoGet = text => text.title(Dic.¤¤Close)
            };
            exit = KeyButt.wrap(exit, KEYS.MAIN().SWAP);
            exit.body().moveX2(C.WIDTH() - 8);
            exit.body().centerY(top);
            top.add(exit);

            top.body().centerY(0, TOP_HEIGHT);
        }

        public void show(IFullView view)
        {
            current = view;
            current.section.body().moveY1(IFullView.TOP_HEIGHT);
            current.section.body().moveX1(16);
            current.init();
            inter.activate();
        }

        public void show()
        {
            show(current == null ? VIEW.UI().goods : current);
        }

        public void close()
        {
            inter.hide();
        }

        public bool open()
        {
            return inter.isActivated();
        }

        private class Inter : Interrupter
        {
            public Inter()
            {
            }

            protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                current.section.hover(mCoo);
                top.hover(mCoo);
                return true;
            }

            protected override void mouseClick(MButt button)
            {
                if (button == MButt.RIGHT)
                {
                    if (!current.back())
                        hide();
                }
                else if (button == MButt.LEFT)
                {
                    current.section.click();
                    top.click();
                }
            }

            protected override void hoverTimer(GBox text)
            {
                current.section.hoverInfoGet(text);
                top.hoverInfoGet(text);
            }

            protected override bool update(float ds)
            {
                GAME.SPEED.tmpPause();
                return false;
            }

            protected override bool render(Renderer r, float ds)
            {
                GCOLOR.UI().bg().render(r, C.DIM());
                current.section.render(r, ds);

                UI.PANEL().butt.render(r, 0, C.WIDTH(), 0, TOP_HEIGHT, 0, DIR.S.mask());
                top.render(r, ds);
                return false;
            }

            public void activate()
            {
                base.show(VIEW.inters().manager);
            }
        }

        public CLICKABLE butt()
        {
            GuiSection s = new GuiSection();
            int i = 0;

            bAdd(s, i++, VIEW.UI().goods, UI.icons().s.storage, new GStat
            {
                Update = text => GFORMAT.perc(text, (SETT.ROOMS().STOCKPILE.tally().amountTotal(null) + 1.0) / (SETT.ROOMS().STOCKPILE.tally().space.total(null) + 1.0), 0)
            });

            bAdd(s, i++, VIEW.UI().economy, UI.icons().s.money, new GStat
            {
                Update = text =>
                {
                    if ((GAME.updateI() & 0x11) == 0)
                    {
                        if (ri >= TR.ALL().size())
                        {
                            probL = prob;
                            prob = 0;
                            ri = 0;
                        }
                        else
                        {
                            TRADABLE res = TR.ALL().get(ri);

                            if (prob < 2)
                            {
                                if (FACTIONS.player().buyer(res).importing() && FACTIONS.player().buyer(res).problem() != null)
                                    prob = 2;
                                if (FACTIONS.player().seller(res).exporting() == null && FACTIONS.player().seller(res).problem() != null)
                                    prob = 2;
                            }

                            if (prob < 1)
                            {
                                if (FACTIONS.player().buyer(res).importing() && FACTIONS.player().buyer(res).warning() != null)
                                    prob = 1;
                                if (FACTIONS.player().seller(res).exporting() == null && FACTIONS.player().seller(res).warning() != null)
                                    prob = 1;
                            }
                            ri++;
                        }
                    }

                    GFORMAT.i(text, (int)FACTIONS.player().credits().credits());

                    if (probL == 0)
                        text.normalify();
                    else if (probL == 1)
                        text.warnify();
                    else
                        text.errorify();
                }
            });

            bAdd(s, i++, VIEW.UI().tourists, UI.icons().s.camera, new GStat
            {
                Update = text => GFORMAT.i(text, STATS.POP().pop(HTYPES.TOURIST()))
            });

            bAdd(s, i++, VIEW.UI().tech, UI.icons().s.vial, new GStat
            {
                Update = text =>
                {
                    int am = 0;
                    foreach (TechCurr c in GAME.player().tech.currs())
                        am += c.available();
                    GFORMAT.i(text, am);
                }
            });

            bAdd(s, i++, VIEW.UI().raider, UI.icons().s.death, new GStat
            {
                Update = text =>
                {
                    GFORMAT.i(text, GAME.raiders().active().size());
                    text.errorify();
                }
            });

            {
                GuiSection ss = new GuiSection();
                ss.addRight(0, bb(VIEW.UI().level, UI.icons().s.arrowUp, null));
                ss.addRight(0, bb(VIEW.UI().profile, UI.icons().s.menu, null));
                bAdd(s, i++, ss);
            }

            return s;
        }

        private void bAdd(GuiSection s, int i, IFullView v, SPRITE icon, SPRITE vv)
        {
            CLICKABLE p = bb(v, icon, vv);
            bAdd(s, i, p);
        }

        private CLICKABLE bb(IFullView v, SPRITE icon, SPRITE vv)
        {
            CLICKABLE p = new CLICKABLE.ClickableAbs(74 / (vv == null ? 2 : 1), 24)
            {
                Render = (r, ds, isActive, isSelected, isHovered) =>
                {
                    GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                    GButt.ButtPanel.renderFrame(r, isActive, isSelected, isHovered, body);

                    if (vv == null)
                        icon.renderC(r, body.cX(), body.cY());
                    else
                    {
                        icon.renderCY(r, body.x1() + 4, body.cY());
                        vv.renderCY(r, body.x1() + 20, body.cY());
                    }
                },
                ClickA = () => show(v),
                HoverInfoGet = text => v.hoverInfoGet(text)
            };
            p.hoverInfoSet(v.title);
            return p;
        }

        private void bAdd(GuiSection s, int i, RENDEROBJ ren)
        {
            s.add(ren, (i / 2) * 74, 24 * ((i % 2)));
        }
    }
}