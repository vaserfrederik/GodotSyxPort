using System;
using System.Collections.Generic;
using System.IO;

using Game;
using Init.Race.Appearance;
using Init.Value;
using Settlement.Stats;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Sets;

namespace Init.Race.Appearance
{
    internal sealed class RPortraitFrame
    {
        private readonly LIST<RaceFrameRaw> frames;
        private readonly int random;
        internal readonly int occurence;

        private int dx, dy, dxr, dyr;

        private readonly ColorCollection color;
        private readonly int opacity;
        private static readonly OpacityImp op = new OpacityImp(0);

        public readonly bool stains;

        public readonly Lockable<Induvidual> cons = GVALUES.INDU.LOCK.Push();

        private static KeyMap<string> keepClean = new KeyMap<string>();
        static RPortraitFrame()
        {
            string[] keep = new string[] {
                "FRAMES",
                "FRAME_RANDOM",
                "FRAME_OCCURENCE",
                "OFF_X",
                "OFF_Y",
                "OFF_X_RANDOM",
                "OFF_Y_RANDOM",
                "CONDITIONS",
                "COLOR",
                "OPACITY",
                "STAINS",
            };

            foreach (string s in keep)
            {
                keepClean.Put(s, s);
            }
        }

        public RPortraitFrame(RaceFrameMaker fm, RColors colors, Json json, int i) : base()
        {
            foreach (string s in json.Keys())
            {
                if (!keepClean.ContainsKey(s))
                {
                    GAME.Warn(json.ErrorGet(s + " is not a valid modifier, available:  " + keepClean.KeysString(), s));
                }
            }

            frames = fm.Read(json);
            random = json.Has("FRAME_RANDOM") ? json.I("FRAME_RANDOM", 0, 16) : i % 16;
            occurence = (int)(0x010 * (json.Has("FRAME_OCCURRENCE") ? json.D("FRAME_OCCURRENCE", 0, 1) : 1.0));
            dx = json.Has("OFF_X") ? json.I("OFF_X", -40, 40) : 0;
            dy = json.Has("OFF_Y") ? json.I("OFF_Y", -48, 48) : 0;
            dxr = json.Has("OFF_X_RANDOM") ? json.I("OFF_X_RANDOM", 0, 40) : 0;
            dyr = json.Has("OFF_Y_RANDOM") ? json.I("OFF_Y_RANDOM", 0, 48) : 0;
            cons.Push("CONDITIONS", json);

            color = json.Has("COLOR") ? colors.collection.Get(json.Value("COLOR"), json) : RColors.dummy;
            opacity = json.Has("OPACITY") ? json.I("OPACITY", 0, 256) : 255;
            stains = json.Has("STAINS") ? json.Bool("STAINS") : true;
        }

        public void Render(SPRITE_RENDERER r, int x1, int y1, Induvidual indu, int scale)
        {
            if (frames.Size() == 0)
                return;

            if (!cons.Passes(indu))
                return;

            int ran = (int)((STATS.RAN().Get(indu, (random * 16))) & 0x0FF);
            if (occurence <= (ran & 0x0F))
                return;

            COLOR col = color.Get(indu, STATS.APPEARANCE().dead.indu().Get(indu) == 1);

            //double grayAt = STATS.POP().age.dage.GetD(indu);

            //if (color.turnsGrayWhenOld && grayAt > 0.7) {
            //    double d = grayAt - 0.7;
            //    d /= 0.2;
            //    d = CLAMP.d(d, 0, 1);
            //    col = ColorImp.TMP.Interpolate(col, RColors.grey, d);
            //}

            col = ColorImp.TMP.Set(col).ShadeSelf(1.2);

            col.Bind();
            op.Set(opacity);
            op.Bind();

            x1 = (int)(x1 + (dx + (ran / 15.0) * dxr) * scale);
            y1 = (int)(y1 + (dy + ((ran >> 4) / 15.0) * dyr) * scale);

            int var = ran % frames.Size();

            frames.Get(var).Render(r, x1, y1, scale);
            COLOR.Unbind();
            OPACITY.Unbind();

            if (stains)
            {
                frames.Get(var).RenderOverlay(r, x1, y1, scale, STATS.NEEDS().INJURIES.COUNT.indu().GetD(indu), STATS.NEEDS().grime(indu), indu.Race().Appearance().Colors.blood);
            }
        }
    }
}