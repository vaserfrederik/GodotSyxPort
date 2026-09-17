using System;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.rnd;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.interrupter;
using view.main;
using world;

namespace view.battle.editor
{
    class Inter : Interrupter
    {
        public ArmySide player = new ArmySide();
        public ArmySide enemy = new ArmySide();

        private Army current = new Army(player, enemy);
        private Placer placer = new Placer(player, enemy);

        public Inter(InterManager m) : base()
        {
            Pin();
            PersistantSet();
            Show(m);

            double pow = Config.battle().MEN_PER_DIVISION * Config.battle().DIVISIONS_PER_ARMY * (0.1 + RND.rFloat() * 1.2);

            player.Generate(pow);
            enemy.Generate(pow);

            {
                GuiSection buttons = new GuiSection();

                buttons.Add(new GButt.ButtPanel(Dic.¤¤OK)
                {
                    protected override void ClickA()
                    {
                        VIEW.b().editor.tools.place(placer);
                        if (!WORLD.GEN().hasGeneratedTerrain)
                        {
                            placer.generate.exe();
                        }
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(player.divs.size() > 0 && enemy.divs.size() > 0);
                    }
                });

                buttons.AddRelBody(8, DIR.E, new GButt.ButtPanel(Dic.¤¤Clear)
                {
                    protected override void ClickA()
                    {
                        player.Clear();
                        enemy.Clear();
                    }
                });

                current.AddRelBody(8, DIR.S, buttons);
            }

            GPanel pan = new GPanel();
            pan.SetBig();
            pan.inner().SetDim(current.body().width(), current.body().height());
            pan.body.centerIn(current);
            pan.setTitle(Army.¤¤name);
            current.Add(pan);
            current.MoveLastToBack();
            current.body().moveCY(C.HEIGHT() / 2);
            current.body().moveCX(C.WIDTH() / 2);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            if (!VIEW.b().editor.tools.placer.isActivated())
                return current.hover(mCoo);
            return false;
        }

        protected override void mouseClick(MButt button)
        {
            if (!VIEW.b().editor.tools.placer.isActivated() && button == MButt.LEFT)
                current.click();
        }

        protected override void hoverTimer(GBox text)
        {
            if (!VIEW.b().editor.tools.placer.isActivated())
                current.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            if (!VIEW.b().editor.tools.placer.isActivated())
                current.render(r, ds);
            return true;
        }

        protected override bool update(float ds)
        {
            VIEW.b().editor.tools.placer.isActivated();
            return false;
        }
    }
}