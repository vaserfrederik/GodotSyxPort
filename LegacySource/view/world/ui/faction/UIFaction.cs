using System;
using game.boosting;
using game.faction;
using game.faction.diplomacy.deal;
using game.faction.npc;
using init.settings;
using init.sprite.UI;
using init.value;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util;
using util.colors;
using util.data;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using view.ui.profile;
using view.ui.util;

namespace view.world.ui.faction
{
    internal sealed class UIFaction : ISidePanel
    {
        private readonly GETTER_IMP<FactionNPC> f;
        private readonly CLICKABLE.ClickSwitch sw;
        private readonly UIDiplomacy dip;

        public UIFaction(GETTER_IMP<FactionNPC> f, Deal deal, int WIDTH, int HEIGHT)
        {
            this.f = f;
            section = new GuiSection
            {
                Render = (SPRITE_RENDERER r, float ds) =>
                {
                    if (f.Get() == null)
                        return;
                    if (!f.Get().IsActive())
                    {
                        f.Set(null);
                        return;
                    }
                    base.Render(r, ds);
                }
            };

            section.Body().SetWidth(WIDTH).SetHeight(1);
            section.AddRelBody(0, DIR.S, new Banner(this.f, WIDTH));

            GuiSection butts = new GuiSection();

            {
                CLICKABLE c = new Court(f, WIDTH, HEIGHT - 40 - section.Body().Height());
                sw = new CLICKABLE.ClickSwitch(c);
                sw.SetD(DIR.N);
                butts.AddRightC(0, sb(Dic.¤¤Court, c));
            }

            int hi = HEIGHT - section.Body().Height() - butts.Body().Height() - 24;
            dip = new UIDiplomacy(f, deal, hi);
            butts.AddRightC(0, sb(Dic.¤¤Realm, new Realm(f, hi)));
            butts.AddRightC(0, sb(Dic.¤¤Goods, new Goods(f, hi)));

            GETTER<BOOSTABLE_O> bbb = new GETTER<BOOSTABLE_O>
            {
                Get = () => f.Get()
            };

            GETTER<Faction> fff = new GETTER<Faction>
            {
                Get = () => f.Get()
            };

            butts.AddRightC(0, sb(Dic.¤¤Boosts, new UIBonus(bbb, fff, hi)
            {
                protected override bool is(Boostable bo)
                {
                    return bo.cat == BOOSTABLES.BATTLE() || bo.cat == BOOSTABLES.ROOMS();
                }
            }));

            butts.AddRightC(0, sb(Dic.¤¤Diplomacy, dip));
            if (S.Get().developer)
            {
                butts.AddRightC(0, UIValues.butt(GVALUES.FACTION, fff));
                butts.AddRightC(0, new GButt.ButtPanel(UI.icons().s.cog)
                {
                    protected override void clickA()
                    {
                        RENDEROBJ sec = new DebuggerSection(800)
                        {
                            protected override void fill(Debugger d)
                            {
                                f.Get().Debug(d);
                            }
                        };
                        VIEW.inters().popup.Show(sec, this);
                        base.clickA();
                    }
                });
            }

            section.AddRelBody(8, DIR.S, butts);
            section.AddRelBody(0, DIR.S, new RENDEROBJ.RenderImp(WIDTH - 128, 16)
            {
                public override void Render(SPRITE_RENDERER r, float ds)
                {
                    GCOLOR.UI().border(r, body().x1(), body().x2(), body().y1() + 5, body().y1() + 8);
                }
            });
            section.AddRelBody(0, DIR.S, sw);
            section.Body().SetWidth(WIDTH).SetHeight(HEIGHT);
        }

        public void dip()
        {
            sw.Set(dip);
        }

        public bool dipIS()
        {
            return sw.Current() == dip;
        }

        private CLICKABLE sb(CharSequence name, CLICKABLE s)
        {
            GButt.ButtPanel b = new GButt.ButtPanel(name)
            {
                protected override void clickA()
                {
                    sw.Set(s);
                }

                protected override void renAction()
                {
                    selectedSet(sw.Current() == s);
                }
            };

            b.Body.SetWidth(140);
            return b;
        }

        public Faction f()
        {
            return f.Get();
        }
    }
}