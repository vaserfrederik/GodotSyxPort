using System;
using System.IO;
using System.Linq;
using game.save;
using game.time;
using init.paths;
using init.sprite;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.sprite.text;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using util.info;
using util.text;
using view.interrupter;
using view.main;

namespace util.gui.common
{
    public class SuperSc : GuiSection
    {
        public static string ¤¤name = "Super screenshot";
        private static string ¤¤desc = "A super screenshot generates a large image of the current view. It takes some time. Screenshots are saved in your local files. You can find them through the game launcher.";
        private static string ¤¤time = "Generate a screenshot every {0} in-game day. Older screenshots will be overwritten by new based on how many screenshots you keep";
        private static string ¤¤keep = "Save and keep {0} screenshot-files. If file amount is exceeded, they fill be overwritten.";
        private static string ¤¤sc = "¤Super Screenshot";

        private readonly string fn;
        private readonly SUPER_SCREENSHOT[] shot;

        static SuperSc()
        {
            D.ts(typeof(SuperSc));
        }

        private readonly double[] day = new double[]
        {
            -1,
            16,
            8,
            4,
            2,
            1,
            0.5,
        };

        private readonly IntImp iday;
        private readonly IntImp saved;
        private readonly IntImp quality;

        private double old = 0;

        public SuperSc(string fn, SUPER_SCREENSHOT[] shot, string saveKey)
        {
            this.fn = fn;
            this.shot = shot;
            Add(new GHeader(¤¤name));
            AddRelBody(4, DIR.S, new GText(UI.FONT().M, ¤¤desc).SetMaxWidth(400).r(DIR.N));

            AddRelBody(4, DIR.S, new GButt.ButtPanel(Dic.¤¤Generate + " 1")
            {
                protected override void ClickA()
                {
                    take();
                }
            });

            iday = new II(saveKey, "DAY", 0, day.Length - 1, 0);
            saved = new II(saveKey, "SAVED", 0, 400, 100);
            quality = new II(saveKey, "QUALITY", 0, shot.Length - 1, (shot.Length - 1) / 2);

            {
                AddRelBody(16, DIR.S, new GHeader(Dic.¤¤Quality));
                GuiSection ss = new GuiSection();
                GSliderInt sl = new GSliderInt(quality, 100, false);
                ss.Add(sl);
                ss.AddRightC(40, new GStat()
                {
                    public override void Update(GText text)
                    {
                        double s = shot[quality.Get()].FileSizeMB();
                        text.Add('~');
                        GFORMAT.f(text, s, 1);
                        text.Add('M').Add('b');
                    }
                });
                AddRelBody(3, DIR.S, ss);
            }

            if (saveKey != null)
            {
                AddRelBody(16, DIR.S, new GHeader(Dic.¤¤Timer));

                GuiSection sl = new GuiSection()
                {
                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        if (iday.Get() > 0)
                        {
                            Str.TMP.Clear().Add(¤¤time).Insert(0, day[iday.Get()], 1);
                            b.Text(Str.TMP);
                        }
                        else
                        {
                            b.Text(Dic.¤¤Deactivated);
                        }
                    }
                };
                sl.Add(UI.icons().s.clock, 0, 0);
                sl.AddRightC(8, new GSliderInt(iday, 200, true));
                AddRelBody(4, DIR.S, sl);

                saved.Set(100);

                sl = new GuiSection()
                {
                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        Str.TMP.Clear().Add(¤¤keep).Insert(0, saved.Get());
                        b.Text(Str.TMP);
                    }
                };
                sl.Add(UI.icons().s.storage, 0, 0);
                sl.AddRightC(8, new GSliderInt(saved, 200, false));
                AddDown(2, sl);

                Interrupter inr = new Interrupter(true, true)
                {
                    protected override bool Update(float ds)
                    {
                        double dday = day[iday.Get()];
                        if (dday < 0)
                            return true;

                        double d = TIME.secondsPerDay() * dday;

                        double day = (TIME.currentSecond() / d) % 1.0;
                        if (old < 0.5 && day >= 0.5)
                        {
                            take();
                        }
                        old = day;

                        return true;
                    }

                    protected override bool Render(Renderer r, float ds)
                    {
                        return true;
                    }

                    protected override void MouseClick(MButt button)
                    {
                        // TODO Auto-generated method stub
                    }

                    protected override void HoverTimer(GBox text)
                    {
                        // TODO Auto-generated method stub
                    }

                    protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
                    {
                        return false;
                    }
                };

                VIEW.inters().manager.Add(inr);
            }
        }

        private class II : IntImp
        {
            private readonly string key;

            public II(string key, string k2, int min, int max, int def)
                : base(min, max)
            {
                if (key != null)
                    key = "SUPER_SCREENSHOT_" + key + "_" + k2;
                this.key = key;
                if (this.key != null)
                {
                    def = PROP.propI(key, def);
                }
                base.Set(def);
            }

            public override void Set(int t)
            {
                if (key != null && t != Get())
                {
                    PROP.propISet(key, t);
                }

                base.Set(t);
                double dday = day[iday.Get()];
                double d = TIME.secondsPerDay() * dday;
                old = (TIME.currentSecond() / d) % 1.0;
            }
        }

        private void take()
        {
            SPRITES.loader().init();
            SPRITES.loader().print(¤¤sc);

            string smallest = null;
            int am = 0;
            long lastM = long.MaxValue;
            PATH p = PATHS.local().SCREENSHOT_S;
            foreach (string s in p.GetFiles())
            {
                if (s.StartsWith(fn))
                {
                    am++;
                    long m = p.get(s).toFile().LastWriteTimeUtc.Ticks;
                    if (m < lastM)
                    {
                        lastM = m;
                        smallest = s;
                    }
                }
            }
            if (am >= saved.Get())
            {
                p.delete(smallest);
            }

            string f = $"{p.get().FullName}{Path.DirectorySeparatorChar}{fn}";
            f = FileManager.NAME.timeStampString(f) + ".jpg";

            shot[quality.Get()].perform(f);
        }
    }
}