using System;
using System.Collections.Generic;
using System.IO;
using game.audio;
using init;
using init.constant;
using init.paths;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.light;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using util.text;

namespace cutscene
{
    public class CutScene : CORE_STATE
    {
        public static CORE_STATE.Constructor make(Json data, Json text, CORE_STATE.Constructor after)
        {
            return new CORE_STATE.Constructor
            {
                e = null,
                getState = () =>
                {
                    new GlJob
                    {
                        doJob = () =>
                        {
                            if (PATHS.CACHE_DATA().exists("cutscene"))
                                PATHS.CACHE_DATA().delete("cutscene");
                            new Initer
                            {
                                createAssets = () =>
                                {
                                    CORE.getSoundCore().stopAllSounds();
                                    CORE.getSoundCore().disposeSounds();
                                    e = new CutScene(data, text, after);
                                }
                            }.get("cutscene", PATHS.textureSize(), 0);
                        }
                    }.perform();
                    return e;
                }
            };
        }

        private static string ¤¤desc = "Hold left mouse button to skip.";
        private static string ¤¤loading = "Loading...";

        static
        {
            D.ts(typeof(CutScene));
        }

        private double timer = 0;
        private readonly SoundStream music;
        private readonly double ll;
        private double quitTimer = 0;
        private bool quit = false;

        private static double quitTime = 2.0;

        private readonly GuiSection section = new GuiSection();
        private readonly CORE_STATE.Constructor after;

        private readonly AmbientLight light;
        private readonly Fire fire = new Fire(4);
        private readonly PointLight fire2 = new PointLight(0.5f, 0.5f, 0.5f);
        private readonly SPRITE image;
        private readonly Text text;

        private CutScene(Json json, Json jtext, CORE_STATE.Constructor after) : base()
        {
            this.after = after;
            new INIT();

            light = new AmbientLight(1, 1, 1, 180, 45);
            CORE.getSoundCore().disposeSounds();
            MusicFactory sf = new MusicFactory();

            music = sf.read(json)[0];
            this.ll = music.getLengthInSeconds();

            image = UI.image().get(json);

            string tt = jtext.text("BODY");
            text = new Text(Math.Max(500, image.height()), tt);

            section.body().setDim(C.DIM().width(), Math.Max(image.height(), 500));

            GTextR title = new GTextR(new GText(UI.FONT().H2, jtext.text("TITLE")));
            title.text().lablify();
            title.body().centerX(C.DIM());
            title.body().moveY2(section.body().y1() - 32);
            section.add(title);

            GTextR desc = new GTextR(new GText(UI.FONT().S, ¤¤desc));
            desc.text().lablifySub();
            desc.body().centerX(C.DIM());
            desc.body().moveY1(section.body().y2() + 32);
            section.add(desc);
            section.body().centerIn(C.DIM());

            music.play();

            fire.setRadius(800);
            fire.setFlickerFactor(5);
            fire.setZ(100);
            fire2.setRadius(800);
            fire2.setZ(100);
        }


        protected override void update(float ds, double slowTheFuckDown)
        {
            timer += ds;
            fire.flicker(ds);
            if (timer > ll - quitTime)
                quit();

            if (quit)
            {
                quitTimer += ds;
            }
            else
            {
                if (MButt.LEFT.isDown())
                {
                    quitTimer += ds;
                }
                else
                {
                    quitTimer -= ds;
                    if (quitTimer < 0)
                        quitTimer = 0;
                }
            }
            if (quitTimer >= quitTime)
                finish();
        }

        protected override void keyPush(List<KeyEvent> keys, bool hasCleared)
        {
            foreach (var e in keys)
            {
                if (e.code() == KEYCODES.KEY_ESCAPE)
                    quit();
            }
        }

        protected override void mouseClick(MButt button)
        {
            // TODO Auto-generated method stub
        }

        protected override void render(Renderer r, float ds)
        {
            double blacken = 1.0;
            if (quitTimer > 0)
            {
                blacken = (quitTimer) / quitTime;
                blacken = CLAMP.d(blacken, 0, 1);
                blacken = 1.0 - blacken;
            }
            else if (timer < 2)
            {
                blacken = timer / 2;
            }
            music.setGain(blacken);
            AmbientLight.full.register(C.DIM());
            section.render(r, ds);
            if (blacken < 1)
            {
                OpacityImp.TMP.set(1.0 - blacken);
                OpacityImp.TMP.bind();
                COLOR.BLACK.render(r, C.DIM().x1(), C.DIM().x2(), 0, C.DIM().cY() - image.height() / 2);
                COLOR.BLACK.render(r, C.DIM().x1(), C.DIM().x2(), C.DIM().cY() + image.height() / 2, C.DIM().y2());
                OPACITY.unbind();
            }

            r.newLayer(false, 0);

            {
                int y1 = C.DIM().cY() - image.height() / 2;
                int x1 = (int)(C.DIM().cX() + 32);
                text.render(r, x1, y1, timer / ll, blacken);
            }

            r.newLayer(false, 0);
            fire.set(C.DIM().cX() - 350, C.DIM().cY() + image.height() / 2 + 50);
            fire.register();
            fire2.set(C.DIM().cX() - 400, C.DIM().cY() - image.height() / 2 - 50);
            fire2.register();

            {
                int y1 = C.DIM().cY() - image.height() / 2;
                int x1 = (int)(C.DIM().cX() - 800 - (image.width() - 400) * ((timer) / ll));
                image.render(r, x1, y1);
                if (blacken < 1)
                {
                    OpacityImp.TMP.set(1.0 - blacken);
                    OpacityImp.TMP.bind();
                    COLOR.BLACK.render(r, C.DIM());
                    OPACITY.unbind();
                }
            }
        }

        private void finish()
        {
            CORE.renderer().clear();
            light.register(C.DIM());
            GCOLOR.T().H1.bind();
            UI.FONT().H1.renderC(CORE.renderer(), C.DIM().cX(), C.DIM().cY(), ¤¤loading);
            COLOR.unbind();
            CORE.swapAndPoll();
            CORE.setCurrentState(after);
        }

        private void quit()
        {
            quit = true;
        }
    }
}