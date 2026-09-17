using System;
using System.IO;
using Newtonsoft.Json;

namespace Init.Race.Apppearance
{
    public sealed class RPortrait
    {
        private readonly RPortraitFrame[] frames;
        public const int P_WIDTH = 5 * 8;
        public const int P_HEIGHT = 8 * 8;

        public RPortrait(ExpandInit init, RColors colors, Json json) 
        {
            if (!json.Has("FACE"))
            {
                frames = Array.Empty<RPortraitFrame>();
                return;
            }

            Json[] js = json.Jsons("FACE");
            frames = new RPortraitFrame[js.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new RPortraitFrame(init.Fm, colors, js[i], i);
            }
        }

        public void Render(SPRITE_RENDERER r, int x1, int y1, Induvidual indu, int scale)
        {
            y1 += scale * 8;
            foreach (RPortraitFrame f in frames)
            {
                f.Render(r, x1, y1, indu, scale);
            }
        }
    }
}