using System;
using System.Collections.Generic;
using Snake2D.Util.Misc;
using Snake2D.Util.Color;
using Snake2D.Util.Light;
using Snake2D.Util.Sets;
using Snake2D.Util.Sprite;
using Snake2D.Util.Sprite.Text;

namespace Snake2D.Util.Misc
{
    public static class StatsDebugger
    {
        public abstract class Formatter
        {
            private Formatter()
            {
            }

            abstract char[] GetFormat(double v);

            public static readonly Formatter PERCENTAGE = new Formatter
            {
                chars = new char[7],
                GetFormat = v =>
                {
                    int d = (int)(v * 100);

                    chars[5] = (char)('0' + d % 10);
                    d /= 10;
                    chars[4] = (char)('0' + d % 10);
                    d /= 10;
                    chars[2] = (char)('0' + d % 10);
                    d /= 10;
                    chars[1] = (char)('0' + d % 10);
                    d /= 10;
                    chars[0] = (char)('0' + d % 10);

                    return chars;
                }
            };

            public static readonly Formatter Amount = new Formatter
            {
                chars = new char[11],
                GetFormat = v =>
                {
                    int d = (int)v;

                    chars[10] = (char)('0' + d % 10); d /= 10;
                    chars[9] = (char)('0' + d % 10); d /= 10;
                    chars[8] = (char)('0' + d % 10); d /= 10;
                    chars[7] = '.';
                    chars[6] = (char)('0' + d % 10); d /= 10;
                    chars[5] = (char)('0' + d % 10); d /= 10;
                    chars[4] = (char)('0' + d % 10); d /= 10;
                    chars[3] = '.';
                    chars[2] = (char)('0' + d % 10); d /= 10;
                    chars[1] = (char)('0' + d % 10); d /= 10;
                    chars[0] = (char)('0' + d % 10); d /= 10;

                    return chars;
                }
            };

            private char[] chars;
        }

        private readonly Font font;
        private readonly int size;
        private bool show = false;
        private readonly ArrayList<Value> values = new ArrayList<Value>(150);

        public StatsDebugger(Font font)
        {
            this.font = font;
            size = font.Height;

            values.Add(GetCoreValue(CoreStats.FPS, 0, Formatter.PERCENTAGE));
            values.Add(GetCoreValue(CoreStats.coreTotal, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.coreFlush, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.corePoll, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.coreSound, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.coreSleep, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.coreFinish, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.swapPercentage, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.totalPercentage, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.renderPercentage, 25, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.updatePercentage, 25, Formatter.Amount));

            values.Add(GetCoreValue(CoreStats.smallUpdates, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.droppedTicks, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.heap, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.usedHeap, 0, Formatter.Amount));
            values.Add(GetCoreValue(CoreStats.heapGrowth, 0, Formatter.Amount));
            values.Add(new Value("sprites", 0, Formatter.Amount)
            {
                GetValue = () => CORE.Renderer.GetSpritesProcessed()
            });
            values.Add(new Value("shadows", 0, Formatter.Amount)
            {
                GetValue = () => CORE.Renderer.GetShadowsRendered()
            });
            values.Add(new Value("lights", 0, Formatter.Amount)
            {
                GetValue = () => CORE.Renderer.GetLightsProcessed()
            });
            values.Add(new Value("particles", 0, Formatter.Amount)
            {
                GetValue = () => CORE.Renderer.GetParticlesProcessed()
            });
        }

        public void Add(Value value)
        {
            values.Add(value);
        }

        public void Flush()
        {
            if (!show)
                return;

            AmbientLight.Full.Register(0, CORE.Graphics.NativeWidth, 0, CORE.Graphics.NativeHeight);

            int y1 = size;
            int x1 = size;

            foreach (var v in values)
            {
                v.Render(CORE.Renderer, x1, y1);
                y1 += size;
                if (y1 + size >= CORE.Graphics.NativeHeight)
                {
                    y1 = size;
                    x1 += 300;
                }
            }

            CORE.Renderer.NewLayer(false, 0);
        }

        public void Toggle()
        {
            show ^= true;
        }

        public bool IsToggled()
        {
            return show;
        }

        public void Show()
        {
            show = true;
        }

        public void Hide()
        {
            show = false;
        }

        private Value GetCoreValue(CoreStats.Value v, int off, Formatter f)
        {
            return new Value(v.Label, off, f)
            {
                GetValue = () => v.Ave
            };
        }

        public abstract class Value
        {
            private double last = -1;
            private readonly SPRITE label;
            private readonly Text value = new Text(font, 16);
            private readonly int off;
            private readonly Formatter format;

            public Value(string label, int off, Formatter f)
            {
                this.label = font.GetText(label);
                this.off = off;
                format = f;
            }

            Value(CoreStats.Value v, Formatter f) : this(v.Label, 0, f)
            {
                SetValue(v.Ave);
            }

            void Render(SPRITE_RENDERER r, int x, int y)
            {
                double val = GetValue();
                if (val != last)
                {
                    SetValue(val);
                }
                COLOR.WHITE65.Bind();
                label.Render(r, x + off, y);
                COLOR.Unbind();
                value.Render(r, x + off + 150, y);
            }

            private void SetValue(double val)
            {
                last = val;

                char[] d = format.GetFormat(val);
                value.Clear();
                for (int i = 0; i < d.Length; i++)
                {
                    value.Add(d[i]);
                }
                value.AdjustWidth();
            }

            protected abstract double GetValue();
        }
    }
}