using System;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.misc;
using view.keyboard;
using view.main;
using view.subview;

namespace view.sett.ui.minimap
{
    public abstract class UIMinimapSettConfig
    {
        public static readonly COLOR colAnimal = new ColorImp(60, 60, 60);
        public static readonly COLOR colHostile = new ColorImp(200, 10, 10);
        public static readonly COLOR colHostileRout = new ColorImp(150, 150, 0);
        public static readonly COLOR colNormal = new ColorImp(0, 50, 255);
        public static readonly COLOR colMustered = new ColorImp(0, 180, 255);

        public abstract COLOR col(ENTITY e);
        public abstract bool halfEnts();
        public abstract bool room(RoomBlueprintIns<?> b);
        public abstract bool renderGrowable();
        public abstract bool renderMinable();
        public abstract bool renderPack();
        public abstract OPACITY shade();
        public abstract bool renderDivs();

        public void AddButtons(GuiSection sec, GameWindow w, UIMinimapSett s)
        {
            CLICKABLE c;

            c = new GButt.ButtPanel(SPRITES.icons().m.plus)
            {
                protected override void ClickA()
                {
                    w.zoomInc(-1);
                    if (w.zoomout() < 3)
                    {
                        VIEW.s().getWindow().centerAt(w.pixels().cX(), w.pixels().cY());
                        s.view.hide();
                    }
                }
            };
            sec.addRightC(0, c);

            c = new GButt.ButtPanel(SPRITES.icons().m.minus)
            {
                protected override void ClickA()
                {
                    w.zoomInc(1);
                }

                protected override void renAction()
                {
                    activeSet(w.zoomout() < w.zoomoutmax());
                }
            };
            sec.addRightC(0, c);
        }

        public static readonly UIMinimapSettConfig NORMAL = new UIMinimapSettConfig()
        {
            public override COLOR col(ENTITY e)
            {
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.indu().hostile())
                    {
                        if (STATS.BATTLE().ROUTING.indu().get(a.indu()) == 0)
                            return colHostile;
                        return colHostileRout;
                    }
                    else if (a.division() != null)
                    {
                        if (a.division().settings().mustering())
                            return colMustered;
                        return colNormal;
                    }
                    return colAnimal;
                }
                return null;
            }

            public override bool halfEnts()
            {
                return false;
            }

            public override bool room(RoomBlueprintIns<?> b)
            {
                return false;
            }

            public override bool renderGrowable()
            {
                return false;
            }

            public override bool renderMinable()
            {
                return false;
            }

            public override bool renderPack()
            {
                return false;
            }

            public override OPACITY shade()
            {
                return OPACITY.O25;
            }

            public override bool renderDivs()
            {
                return KEYS.BATTLE().SHOW_DIVISIONS.isPressed();
            }
        };

        public static readonly UIMinimapSettConfig ALL = new UIMinimapSettConfig()
        {
            public override COLOR col(ENTITY e)
            {
                return NORMAL.col(e);
            }

            public override bool halfEnts()
            {
                return false;
            }

            public override bool room(RoomBlueprintIns<?> b)
            {
                return false;
            }

            public override bool renderGrowable()
            {
                return true;
            }

            public override bool renderMinable()
            {
                return true;
            }

            public override bool renderPack()
            {
                return true;
            }

            public override OPACITY shade()
            {
                return OPACITY.O25;
            }

            public override bool renderDivs()
            {
                return KEYS.BATTLE().SHOW_DIVISIONS.isPressed();
            }
        };
    }
}