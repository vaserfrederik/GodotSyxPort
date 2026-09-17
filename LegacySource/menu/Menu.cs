using System;
using game.faction.player;
using init.constant;
using init.settings;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.light;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.text;

namespace menu
{
    public class Menu : CORE_STATE
    {
        private readonly ScMain main;
        private readonly ScOptions options;
        private readonly ScLoad load;
        private readonly ScLoad[] loads;
        private readonly ScRandom sandbox2;
        private readonly ScRandomSettings sandboxSettings;
        private readonly ScCredits credits;
        private readonly ScCampaign campaigns;
        private SC current;

        private Coo mCoo = new Coo();

        private readonly Background bg;

        private readonly PointLight mouseLight;

        private readonly Logo logo;
        private static bool hasLogo = true;

        private readonly Intro intro;
        private static bool hasIntro = true;
        private float fadeLight = 0;

        public readonly RESOURCES res;

        public static void Start()
        {
            CORE.create(S.Get().Make());
            CORE.Input.GetMouse().ShowCursor(false);
            CORE.Start(new CORE_STATE.Constructor
            {
                State = make
            });
        }

        public static Menu Make()
        {
            Menu menu = new Menu();
            PreLoader.Exit();
            return menu;
        }

        public static Menu MakeCampaign()
        {
            Menu menu = new Menu();
            PreLoader.Exit();
            menu.SwitchScreen(menu.campaigns);
            return menu;
        }

        private Menu()
        {
            res = new RESOURCES();
            var bounds = new Rec(0, C.MIN_WIDTH, 0, 2 * 256);
            bounds.CenterIn(C.DIM());

            GUI.Init(bounds);

            bg = new Background(this, bounds);

            options = new ScOptions(this);
            sandbox2 = new ScRandom(this);
            sandboxSettings = new ScRandomSettings(this);
            load = ScLoad.Load(this);

            loads = new ScLoad[] { ScLoad.Scenarios(this), ScLoad.Battle(this), ScLoad.Showcase(this) };
            credits = new ScCredits(this);
            main = new ScMain(this);
            campaigns = new ScCampaign(this);
            current = main;

            mouseLight = new PointLight(1.0f, 1.0f, 1.3f, 0, 0, 15);
            mouseLight.SetFalloff(1);

            intro = new Intro(main, bg);

            logo = new Logo(this);

            S.Get().ApplyRuntimeConfigs();
            PTitles.Achieve();
        }

        private void Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            this.mCoo.Set(mCoo);
            if (hasIntro || hasLogo)
                return;

            mouseLight.Set(mCoo);
            current.Hover(mCoo);
        }

        public override void Update(float ds, double slow)
        {
            Hover(CORE.Input.GetMouse().GetCoo(), false);
            if (hasLogo)
            {
                hasLogo = logo.Update(ds);
                return;
            }

            res.sound().Play();

            hasIntro = hasIntro && intro.Update(ds);
            if (!hasIntro && fadeLight < 1)
            {
                fadeLight += ds;
                if (fadeLight > 1)
                    fadeLight = 1;
            }
        }

        public override void Render(Renderer r, float ds)
        {
            CORE.Renderer().ShadeLight(true);
            CORE.Renderer().ShadowDepthDefault();
            if (hasLogo)
            {
                logo.Render(r, ds);
                return;
            }

            if (hasIntro)
            {
                intro.Render(r, ds);
                return;
            }

            AmbientLight.Strongmoonlight.Register(C.DIM());
            UI.decor().mouse.Render(r, mCoo.x(), mCoo.y());
            r.NewLayer(true, 0);

            mouseLight.SetRed(fadeLight);
            mouseLight.SetGreen(fadeLight);
            mouseLight.SetBlue(fadeLight * 1.3);

            mouseLight.Register();
            current.Render(rr, ds);

            r.NewLayer(false, 0);

            current.RenderBackground(bg, ds, mCoo);
        }

        public override void MouseClick(MButt button)
        {
            if (hasLogo)
            {
                hasLogo = false;
                return;
            }

            if (hasIntro)
            {
                hasIntro = false;
                return;
            }

            if (button == MButt.LEFT)
            {
                current.Click();
            }
            if (button == MButt.RIGHT)
                current.Back(this);
        }

        void SwitchScreen(SC screen)
        {
            current = screen;
            current.Hover(mCoo);
        }

        SC Screen()
        {
            return current;
        }

        Coo GetMCoo()
        {
            return mCoo;
        }

        void Start(Constructor state)
        {
            CORE.Renderer().Clear();

            var s = new GuiSection();
            GUI.AddTitleText(s, Dic.¤¤loading);
            s.body().CenterIn(C.DIM());
            s.Render(rr, 0);
            AmbientLight.Strongmoonlight.Register(C.DIM());
            CORE.Renderer().NewLayer(false, 0);
            bg.Render(CORE.Renderer(), 0);

            CORE.SwapAndPoll();
            CORE.SetCurrentState(state);
        }

        protected override void KeyPush(LIST<KeyEvent> keys, bool hasCleared)
        {
            for (int i = 0; i < keys.Size(); i++)
            {
                KeyEvent key = keys.Get(i);
                if (hasLogo)
                {
                    hasLogo = false;
                    return;
                }

                if (hasIntro)
                {
                    hasIntro = false;
                    return;
                }
                if (key.Code() == KEYCODES.KEY_ESCAPE)
                {
                    if (!current.Back(this))
                    {
                        return;
                    }
                    break;
                }
                else
                {
                    current.Poll(key);
                }

            }

        }

        private readonly SPRITE_RENDERER rr = new SPRITE_RENDERER()
        {
            private readonly int ss = 4;
            private readonly int si = 8;

            public void RenderSprite(int x1, int x2, int y1, int y2, TextureCoords texture)
            {
                CORE.Renderer().RenderSprite(x1, x2, y1, y2, texture);
                for (int i = 0; i < si; i++)
                    CORE.Renderer().RenderShadow(x1 + ss + i, x2 + ss + i, y1 - ss - i, y2 - ss - i, texture, (byte)0);
            }
        };
    }
}