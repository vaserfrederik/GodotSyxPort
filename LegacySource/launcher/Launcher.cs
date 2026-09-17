using System;
using System.Collections.Generic;
using System.IO;
using game;
using init.paths;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using util.error;
using util.spritecomposer;
using util.text;

namespace launcher
{
    public class Launcher : CORE_STATE
    {
        static bool startGame = false;

        void reboot()
        {
            CORE.renderer().clear();
            CORE.swapAndPoll();
            CORE.setCurrentState(new Constructor()
            {
                public CORE_STATE getState()
                {
                    return new Launcher(false);
                }
            });
        }

        public static void Main(string[] args)
        {
            bool selectLang = !PATHS.local().SETTINGS.exists("LauncherSettings");
            LSettings settings = new LSettings();

            if (!PATHS_BASE.langs().existsFolder(settings.lang.get()))
            {
                selectLang = true;
            }
            CORE.init(new ErrorHandler());
            CORE.create(new Sett());
            final bool l = selectLang;
            CORE.start(new Constructor()
            {
                public CORE_STATE getState()
                {
                    PreLoader.exit();
                    return new Launcher(l);
                }
            });
            if (!startGame)
                Environment.Exit(1);
            Environment.Exit(0);
        }

        RES res;
        readonly GUI g;
        private readonly BG bg;
        private COORDINATE mCoo = new Coo();
        readonly LSettings s = new LSettings();

        private GuiSection current;
        private readonly ScreenMain main;
        private readonly ScreenSetting setts;
        private readonly ScreenMods mods;
        private readonly ScreenInfo info;
        private readonly ScreenLog log;
        private readonly ScreenLang lang;

        private Launcher(bool selectLang)
        {
            PATHS.init(new string[0], s.lang.get().Length > 0 ? s.lang.get() : null, false);
            D.init();
            new GlJob()
            {
                public void doJob()
                {
                    new Initer()
                    {
                        public void createAssets()
                        {
                            res = new RES();
                        }
                    }.get("launcher", 1024, 0);
                }
            }.perform();

            g = new GUI(res);

            bg = new BG(res);
            log = new ScreenLog(this);
            lang = new ScreenLang(this, true);
            main = new ScreenMain(this, lang);
            info = new ScreenInfo(this);
            mods = new ScreenMods(this);
            setts = new ScreenSetting(this);
            current = main;

            if (selectLang)
                current = new ScreenLang(this, false);
            else
            {
                if (s.version.get() != VERSION.VERSION)
                {
                    s.save();
                    current = log;
                }
            }
        }


        public override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                current.click();
            }
            else if (button == MButt.RIGHT)
                setMain();
        }

        public override void update(float ds, double slow)
        {
            bg.update(ds);
            hover(CORE.getInput().getMouse().getCoo(), false);
        }

        public override void render(Renderer r, float ds)
        {
            bg.render(r, ds);

            current.render(r, ds);
            bg.renderClouds(r, ds);
        }


        //functions for the buttons

        void setInfo()
        {
            current = info;
            current.hover(mCoo);
        }
        //	
        void setMain()
        {
            current = main;
            current.hover(mCoo);
        }
        //	
        void setSetts()
        {
            current = setts;
            current.hover(mCoo);
        }

        void setMods()
        {
            current = mods;
            current.hover(mCoo);
        }

        void setLang()
        {
            current = lang;
            current.hover(mCoo);
        }

        void setModWarning()
        {
            current = new ScreenModWarning(this);
            current.hover(mCoo);
        }
        //	
        void setLog()
        {
            current = log;
            current.hover(mCoo);
        }

        public void hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            this.mCoo = mCoo;
            current.hover(mCoo);
        }

        protected override void keyPush(LIST<KeyEvent> keys, bool hasCleared)
        {
            foreach (KeyEvent c in keys)
            {
                if (CORE.getInput().getKeyboard().isPressed(KEYCODES.KEY_LEFT_CONTROL) && c.code() == KEYCODES.KEY_X)
                    throw new RuntimeException("creating debugging info. Please send this to the developer if you're having issues");
                if (c.code() == KEYCODES.KEY_ESCAPE)
                {
                    startGame = false;
                    CORE.annihilate();
                    return;
                }
            }
        }
    }
}