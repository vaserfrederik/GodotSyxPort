using System;
using System.Collections.Generic;
using util.text;
using view.keyboard;
using init.sprite;
using snake2d.util.map;
using snake2d.util.sets;

namespace view.tool
{
    public abstract class PLACER_TYPE
    {
        static PLACER_TYPE()
        {
            D.gInit(typeof(PLACER_TYPE));
        }

        public static readonly PLACER_TYPE SQUARE = new PLACER_TYPE(true, false, D.g("rectangle")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_rec;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                int x11 = Math.Min(x1, x2);
                int x22 = Math.Max(x1, x2);
                int y11 = Math.Min(y1, y2);
                int y22 = Math.Max(y1, y2);
                for (int y = y11; y <= y22; y++)
                    for (int x = x11; x <= x22; x++)
                        area.set(x, y);
            }
        };

        public static readonly PLACER_TYPE SQUARE_HOLLOW = new PLACER_TYPE(true, true, D.g("hollow rectangle")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_rec_hollow;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                int x11 = Math.Min(x1, x2);
                int x22 = Math.Max(x1, x2);
                int y11 = Math.Min(y1, y2);
                int y22 = Math.Max(y1, y2);

                while (size >= 0 && x11 <= x22 && y11 <= y22) {
                    outline(x11, y11, x22, y22, area);
                    x11++;
                    x22--;
                    y11++;
                    y22--;
                    size--;
                }
            }

            private void outline(int x1, int y1, int x2, int y2, MAP_SETTER area) {
                for (int y = y1; y <= y2; y++) {
                    if (y == y1 || y == y2) {
                        for (int x = x1; x <= x2; x++) {
                            area.set(x, y);
                        }
                    } else {
                        area.set(x1, y);
                        area.set(x2, y);
                    }
                }
            }
        };

        public static readonly PLACER_TYPE BRUSH = new PLACER_TYPE(false, true, D.g("brush")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_brush;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                size += 1;
                int min = size / 2;

                double s = size / 2.0;
                double s2 = s * s;
                double d = (size & 1) == 0 ? 0.5 : 0;

                for (int dy = -min; dy <= min; dy++) {
                    for (int dx = -min; dx <= min; dx++) {
                        double dist = (dx + d) * (dx + d) + (dy + d) * (dy + d);
                        if (dist <= s2) {
                            area.set(x1 + dx, y1 + dy);
                        }
                    }
                }
            }
        };

        public static readonly PLACER_TYPE LINE = new PLACER_TYPE(true, true, D.g("line")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_line;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                if (x1 == x2 && y1 == y2) {
                    area.set(x1, y1);
                    return;
                }

                int dx = x2 - x1;
                int dy = y2 - y1;
                dx = dx < 0 ? -1 : (dx > 0 ? 1 : 0);
                dy = dy < 0 ? -1 : (dy > 0 ? 1 : 0);
                bool startX = dy * dx >= 0;

                int newX = -dy;
                int newY = dx;
                dx = newX;
                dy = newY;

                int offX1 = 0;
                int offY1 = 0;

                for (int i = 1; i <= size / 2; i += 2) {
                    if ((i & 1) == 1) {
                        x1 -= dx;
                        x2 -= dx;
                        y1 -= dy;
                        y2 -= dy;
                    }
                }

                for (int i = 0; i <= size; i++) {
                    drawLine(x1 + offX1, y1 + offY1, x2 + offX1, y2 + offY1, area, (i & 1) == 0);
                    if (startX) {
                        offX1 += dx;
                    } else {
                        offY1 += dy;
                    }
                    startX = !startX;
                }
            }

            private void drawLine(int x1, int y1, int x2, int y2, MAP_SETTER area, bool first) {
                int dx = x2 - x1;
                int dy = y2 - y1;
                dx = dx < 0 ? -1 : (dx > 0 ? 1 : 0);
                dy = dy < 0 ? -1 : (dy > 0 ? 1 : 0);
                if (first)
                    area.set(x1, y1);
                while (x1 != x2 || y1 != y2) {
                    if (x1 != x2) {
                        x1 += dx;
                    }
                    if (y1 != y2) {
                        y1 += dy;
                    }
                    area.set(x1, y1);
                }
            }
        };

        public static readonly PLACER_TYPE OVAL = new PLACER_TYPE(true, true, D.g("oval")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_oval;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                int x11 = Math.Min(x1, x2);
                int x22 = Math.Max(x1, x2);
                int y11 = Math.Min(y1, y2);
                int y22 = Math.Max(y1, y2);

                double width = x22 - x11;
                double height = y22 - y11;

                if (KEYS.MAIN().MOD.isPressed()) {
                    width = Math.Max(width, height);
                    height = Math.Max(width, height);
                }

                double divisor = Math.Max(width, height);
                double r2 = Math.Max(width, height);

                r2 *= r2;
                for (double y = -height; y <= height; y++) {
                    for (double x = -width; x <= width; x++) {
                        double distX = x * (divisor / width);
                        double distY = y * (divisor / height);
                        double r = distX * distX + distY * distY;
                        if (r <= r2) {
                            area.set((int)(x11 + x), (int)(y11 + y));
                        }
                    }
                }
            }
        };

        public static readonly PLACER_TYPE HEXAGON_HOLLOW = new PLACER_TYPE(true, true, D.g("hollow hexagon")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_hex_hollow;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                paintHexagon(x1, y1, x2, y2, size, true, area);
            }
        };

        public static readonly PLACER_TYPE HEXAGON = new PLACER_TYPE(true, true, D.g("hexagon")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_hex;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) {
                paintHexagon(x1, y1, x2, y2, size, false, area);
            }
        };

        public static readonly PLACER_TYPE FILL = new PLACER_TYPE(false, true, D.g("fill")) {
            protected override Icon icon() {
                return SPRITES.icons().m.place_fill;
            }

            protected override void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area) { }
        };

        public static readonly LIST<PLACER_TYPE> all = new ArrayList<PLACER_TYPE>(SQUARE, SQUARE_HOLLOW, BRUSH, LINE, FILL, OVAL, HEXAGON_HOLLOW, HEXAGON);

        private readonly bool drag;
        private readonly bool usesSize;
        private readonly CharSequence name;

        protected PLACER_TYPE(bool drag, bool usesSize, CharSequence name) {
            this.drag = drag;
            this.name = name;
            this.usesSize = usesSize;
        }

        protected abstract void paint(int x1, int y1, int x2, int y2, int size, MAP_SETTER area);

        protected abstract Icon icon();

        private void paintHexagon(int x1, int y1, int x2, int y2, int size, bool hollow, MAP_SETTER area) {
            // Implementation for painting a hexagon
        }
    }
}