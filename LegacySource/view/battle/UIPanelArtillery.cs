using System.Collections.Generic;
using game;
using game.battle;
using init.sprite;
using settlement.room.military.artillery;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;
using view.keyboard;
using view.main;

namespace view.battle
{
    public class UIPanelArtillery : ISidePanel
    {
        private readonly CatSelection selection;
        private readonly ArrayListResize<ArtilleryInstance> all = new ArrayListResize<ArtilleryInstance>(256);

        private static int width = 160;

        public UIPanelArtillery(Army army, CatSelection selection)
        {
            titleSet(Dic.¤¤Artillery);
            this.selection = selection;
            GTableBuilder builder = new GTableBuilder
            {
                upI = -1,

                nrOFEntries = () =>
                {
                    if (upI != GAME.updateI())
                    {
                        upI = GAME.updateI();
                        all.clearSoft();
                        foreach (ArtilleryInstance bb in selection.all())
                        {
                            if (bb.army() == army)
                            {
                                all.add(bb);
                            }
                        }
                    }
                    return all.size();
                }
            };

            builder.column(null, width, new GRowBuilder
            {
                build = ier =>
                {
                    return new CatButton(ier, selection);
                }
            });

            section.add(builder.createHeight(HEIGHT, false));
        }

        private static int lastClicked;

        class CatButton : ClickableAbs
        {
            private readonly GETTER<int> ier;

            public CatButton(GETTER<int> ier, CatSelection selection)
            {
                body.setDim(width, 28);
                this.ier = ier;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                ArtilleryInstance ins = get();
                if (ins == null)
                    return;

                isSelected = ins.selected;
                isHovered = ins.hovered;
                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                GButt.ButtPanel.renderFrame(r, isActive, isSelected, isHovered, body);
                ins.blueprintI().iconBig().medium.render(r, body().x1() + 3, body().y1() + 3);

                if (ins.mustered())
                {
                    if (ins.targetDivGet() != null || ins.targetCooGet() != null)
                    {
                        SPRITES.icons().s.bow.render(r, body().x2() - 40, body().y1() + 6);
                    }
                    if (ins.menMustering() == 0)
                        GCOLOR.T().IBAD.bind();
                    else if (ins.menMustering() == 1)
                        GCOLOR.T().IGREAT.bind();
                    else
                        GCOLOR.T().IGOOD.bind();
                    SPRITES.icons().s.human.render(r, body().x2() - 20, body().y1() + 6);
                    COLOR.unbind();
                }
            }

            protected override void clickA()
            {
                ArtilleryInstance ins = get();
                if (ins == null)
                    return;

                if (KEYS.MAIN().MOD.isPressed())
                {
                    selection.toggle(ins);
                    lastClicked = -1;
                }
                else if (KEYS.MAIN().UNDO.isPressed() && lastClicked != -1)
                {
                    int s = lastClicked;
                    int e = ier.get();

                    if (e < s)
                    {
                        int k = s;
                        s = e;
                        e = k;
                    }
                    for (; s <= e; s++)
                    {
                        if (s >= 0 && s < selection.all().size())
                            selection.select(selection.all().get(s));
                    }
                }
                else
                {
                    selection.clear();
                    selection.toggle(ins);
                    lastClicked = ier.get();
                }

                if (MButt.LEFT.isDouble())
                {
                    VIEW.s().battle.getWindow().centererTile.set(ins.body().cX(), ins.body().cY());
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                ArtilleryInstance ins = get();
                if (ins == null)
                    return;
                ins.hover(text);
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    ArtilleryInstance ins = get();
                    if (ins != null)
                        ins.hovered = true;
                    return true;
                }
                return false;
            }

            private ArtilleryInstance get()
            {
                int i = ier.get();
                if (i < 0 || i >= all.size())
                    return null;
                return all.get(i);
            }
        }
    }
}