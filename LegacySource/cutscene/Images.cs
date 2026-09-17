using System;
using System.IO;
using Newtonsoft.Json.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;

namespace cutscene
{
    class Images : RENDEROBJ.RenderImp
    {
        private readonly SPRITE[] ims;
        private readonly double[] delays;
        private double time = 0;

        public Images(JToken json) : base()
        {
            JToken[] js = json["IMAGES"].ToArray();

            ims = new SPRITE[js.Length];
            delays = new double[js.Length];

            for (int i = 0; i < ims.Length; i++)
            {
                ims[i] = UI.image().get(js[i]);
                delays[i] = js[i]["DELAY"].Value<double>();
            }

            int w = 0;
            int h = 0;

            foreach (SPRITE s in ims)
            {
                w = Math.Max(s.width(), w);
                h = Math.Max(s.height(), h);
            }

            body.setDim(w, h);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            SPRITE im = image();
            if (im != null)
                im.renderC(r, body);
            double op = 1.0 - op();

            OpacityImp.TMP.set(op);
            OpacityImp.TMP.bind();
            COLOR.BLACK.render(r, body);
            OPACITY.unbind();
            time += ds;
        }

        private SPRITE image()
        {
            for (int i = 0; i < delays.Length; i++)
            {
                if (delays[i] > time)
                {
                    if (i == 0)
                        return null;
                    return ims[i - 1];
                }
            }
            return ims[ims.Length - 1];
        }

        private double op()
        {
            if (time < 1)
                return time;

            for (int i = 0; i < delays.Length; i++)
            {
                double d = delays[i] - time;

                if (d > 0 && d < 1)
                    return d;
                else if (d > -1 && d < 0)
                {
                    return -d;
                }
            }
            return 1;
        }
    }
}