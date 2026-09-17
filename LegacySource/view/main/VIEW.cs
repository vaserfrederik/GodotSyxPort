using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.gui.misc;
using util.rendering;
using util.text;
using view.battle;
using view.interrupter;
using view.keyboard;
using view.sett;
using view.ui;
using view.world;
using world;

namespace view.main
{
    public class VIEW : CORE_STATE
    {
        private static VIEW i;

        static VIEW()
        {
            KEYS.init();
        }

        private KeyPoller keyPoller = KEYS.get();
        private readonly UIView ui;
        private readonly WorldView world;
        private readonly SettView sett;
        private readonly BattleView battle;
        private ViewSubSimple current;
        private ViewSub previous;

        private readonly Mouse mouse;

        private readonly Interrupters inters;
        private bool hideUI = false;
        private static double renderSecond;
        public static int renI;

        private readonly GAME game;

        public VIEW(GAME game)
        {
            i = this;
            this.game = game;
            ViewSub.all.Clear();
            mouse = new Mouse();
            inters = new Interrupters();
            ui = new UIView();
            world = new WorldView();
            sett = new SettView();
            battle = new BattleView();
            world.activate();
            KEYS.get().readSettings();
            setFirstView(world);

            GAME.saver().add(new Savable("VIEW")
            {
                protected override void save(FilePutter file)
                {
                    if (current is ViewSub)
                        file.i(((ViewSub)current).index);
                    else
                        file.i(-1);
                }

                protected override void load(FileGetter file) throws IOException
                {
                    int si = file.i();
                    ViewSub v = null;
                    if (si >= 0)
                        v = ViewSub.all.get(si);

                    KEYS.get().readSettings();
                    setFirstView(v);
                    current.activate();
                }
            });
        }

        private void setFirstView(ViewSubSimple prefered)
        {
            if (WORLD.GEN().isEditing)
                prefered = world.editor;
            else if (prefered == null || !WORLD.GEN().isDone)
                prefered = new WorldViewGenerator();

            prefered.activate();
            previous = null;
        }

        protected override void keyPush(LIST<KeyEvent> keys, bool hasCleared)
        {
            keyPoller.poll(keys);
            keyPoller = KEYS.get();
        }

        protected override void mouseClick(MButt button)
        {
            if (!inters.manager.click(button))
                return;

            if (inters.mouseMessage.close())
                return;

            GAME.script().callback.mouseClick(button);
            if (current.uiManager.click(button))
                current.mouseClick(button);
        }

        private double hoverTimer = 0;

        private void hover()
        {
            COORDINATE mCoo = CORE.getInput().getMouse().getCoo();
            int dx = mCoo.x() - mouse.x();
            int dy = mCoo.y() - mouse.y();
            int d = dx * dx + dy * dy;
            bool mouseHasMoved = d > 5;
            mouse.getCoo().set(mCoo);

            GAME.script().callback.hover(mCoo, mouseHasMoved);

            if (mouseHasMoved)
                hoverTimer = 0;

            if (inters.manager.hover(mCoo, mouseHasMoved))
                if (current.uiManager.hover(mCoo, mouseHasMoved))
                    current.hover(mCoo, mouseHasMoved);

            GAME.script().callback.hoverTimer(hoverTimer, inters.mouseMessage.get());
            if (!inters.mouseMessage.get().emptyIs())
                return;

            if (inters.manager.hoverTimer(hoverTimer, inters.mouseMessage.get()))
                if (current.uiManager.hoverTimer(hoverTimer, inters.mouseMessage.get()))
                    current.hoverTimer(hoverTimer, inters.mouseMessage.get());

            if (!inters.mouseMessage.get().emptyIs())
                mouse.setReplacement(UI.icons().m.questionmark);

            if (hoverTimer < 0.4)
                inters.mouseMessage.get().clear();
        }

        protected override void update(float ds, double slowDown)
        {
            game.afterTick();

            inters.manager.afterTick();
            current.uiManager.afterTick();
            current.afterTick();

            hover();
            hoverTimer += ds;

            if (KEYS.MAIN().DEBUGGER.consumeClick())
                GUTIL.debugger().toggle();
            if (hideUI)
            {
                if (KEYS.MAIN().ESCAPE.consumeClick() | MButt.RIGHT.consumeAllClick())
                {
                    hideUI = false;
                    inters.mouseMessage.get().clear();
                }
                else
                {
                    if (VIEW.s().isActive() && VIEW.s().ui.subjects.current() != null)
                    {
                        VIEW.s().getWindow().centerer.set(VIEW.s().ui.subjects.current().body().cX(), VIEW.s().ui.subjects.current().body().cY());
                    }
                }
            }

            inters.mouseMessage.update(mouse);
            if (inters.manager.update(ds) & current.uiManager.update(ds))
            {
                current.update(ds, true);
            }
            else
            {
                current.update(ds, false);
                ds = 0;
                slowDown = 1.0;
            }

            if (KEYS.MAIN().SWAP.consumeClick())
            {
                if (VIEW.UI().manager.open())
                {
                    VIEW.UI().manager.close();
                    VIEW.world().activate();
                }
                else if (VIEW.world().isActive())
                {
                    VIEW.s().activate();
                }
                else if (VIEW.s().isActive())
                {
                    VIEW.UI().manager.show();
                }
            }

            if (KEYS.MAIN().ESCAPE.consumeClick())
            {
                inters.menu.show();
            }

            if (KEYS.MAIN().QUICKSAVE.consumeClick() && canSave())
            {
                SPRITES.loader().minify(true, Dic.¤¤SAVING);
                GAME.saver().quicksave();
                SPRITES.loader().minify(false, Dic.¤¤SAVING);
            }

            if (KEYS.MAIN().QUICKLOAD.consumeClick() && GameLoader.quickload())
            {
                SPRITES.loader().minify(true, Dic.¤¤load);
                return;
            }

            game.update(ds, slowDown);
        }

        protected override void render(Renderer r, float ds)
        {
            renI++;
            renderSecond += ds;

            if (hideUI)
                ui.render(r, ds, true);
            else
                ui.render(r, ds, false);

            current.render(r, ds, hideUI);
        }

        public static abstract class ViewSubSimple
        {
            protected abstract void hoverTimer(double mouseTimer, GBox text);

            protected virtual bool canSave()
            {
                return true;
            }

            protected abstract bool update(float ds, bool shouldUpdate);
            protected abstract void render(Renderer r, float ds, bool hide);

            public virtual void renderBelowTerrain(Renderer r, ShadowBatch s, RenderData data)
            {
            }

            protected abstract void mouseClick(MButt button);
            protected abstract void hover(COORDINATE mCoo, bool mouseHasMoved);
            public readonly InterManager uiManager = new InterManager();

            public void activate()
            {
                if (i.current == this)
                    return;
                if (i.current != null)
                    i.current.deactivate();
                i.inters.mouseMessage.close();
                if (i.current is ViewSub)
                    i.previous = (ViewSub)i.current;
                i.current = this;
                hover(CORE.getInput().getMouse().getCoo(), true);
            }

            public virtual void deactivate()
            {
            }

            public bool isActive()
            {
                return this == VIEW.i.current;
            }

            protected virtual void afterTick()
            {
            }
        }

        public static abstract class ViewSub : ViewSubSimple
        {
            private static readonly ArrayList<ViewSub> all = new ArrayList<ViewSub>(20);
            private readonly int index = all.add(this);

            public int index()
            {
                return index;
            }
        }
    }
}