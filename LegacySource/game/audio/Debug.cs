using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using view.interrupter;
using view.main;

namespace game.audio
{
    final class Debug
    {
        Debug()
        {
            IDebugPanel.Add("AUDION: MUSIC SHUFFLE", new ACTION
            {
                public void exe()
                {
                    AUDIO.music().next();
                }
            });

            IDebugPanel.Add("AUDIO: AMBIENCE", new ACTION
            {
                public void exe()
                {
                    VIEW.inters().popup.show(Ambience(), null);
                }
            });

            IDebugPanel.Add("AUDIO: STREAMS", new ACTION
            {
                public void exe()
                {
                    VIEW.inters().popup.show(Streams(), null);
                }
            });
        }

        private GuiSection Ambience()
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
            double[] priority = new double[AUDIO.AMBI().all().Count];
            double[] gains = new double[AUDIO.AMBI().all().Count];
            Array.Fill(gains, 1.0);

            foreach (Ambiance a in AUDIO.AMBI().all())
            {
                GuiSection row = new GuiSection();
                row.add(new GHeader(a.key()));

                INTE prio = new INTE
                {
                    public int min() => 0,
                    public int max() => 200,
                    public int get() => (int)(priority[a.index()] * 100),
                    public void set(int t) => priority[a.index()] = t / 100.0
                };

                row.addRightCAbs(300, new GSliderInt(prio, 100, true));

                INTE gain = new INTE
                {
                    public int min() => 0,
                    public int max() => 200,
                    public int get() => (int)(gains[a.index()] * 100),
                    public void set(int t) => gains[a.index()] = t / 100.0
                };

                row.addRightC(16, new GSliderInt(gain, 100, true));

                rows.add(row);
            }

            GuiSection s = new GuiSection
            {
                public void render(SPRITE_RENDERER r, float ds)
                {
                    AUDIO.AMBI_UP().debugPrio = priority;
                    AUDIO.AMBI_UP().debugGain = gains;
                    base.render(r, ds);
                }
            };

            s.add(new GScrollRows(rows, C.HEIGHT() - 200).view());

            return s;
        }

        private GuiSection Streams()
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
            double[] priority = new double[AUDIO.AMBI().all().Count];
            double[] gains = new double[AUDIO.AMBI().all().Count];
            Array.Fill(gains, 1.0);

            foreach (string s in AUDIO.AMBI().factory.map().keysSorted())
            {
                SoundStream st = AUDIO.AMBI().factory.map().get(s);

                rows.add(new GButt.ButtPanel(s)
                {
                    protected override void clickA()
                    {
                        st.playOnce();
                        base.clickA();
                    }
                });
            }

            GuiSection s = new GuiSection
            {
                public void render(SPRITE_RENDERER r, float ds)
                {
                    AUDIO.AMBI_UP().debugPrio = priority;
                    AUDIO.AMBI_UP().debugGain = gains;
                    base.render(r, ds);
                }
            };

            s.add(new GScrollRows(rows, C.HEIGHT() - 200).view());

            return s;
        }
    }
}