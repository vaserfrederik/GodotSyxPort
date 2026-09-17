using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using init.paths;
using snake2d.util.file;
using snake2d.util.sets;
using util.spritecomposer;

namespace init.sprite.UI
{
    public sealed class UIFonts
    {
        public readonly Font H2;
        public readonly Font H1;
        public readonly Font S;
        public readonly Font M;
        public readonly LIST<Font> all;

        public UIFonts() : base()
        {
            Json json = new Json(PATHS.CONFIG().init.gets("Charset"));
            string cs = json.text("CHARS");
            int trail = json.i("SPACING", 0, 32, 0);

            Font.setCharset(cs);
            PATH g = PATHS.SPRITE().getFolder("font");

            Dictionary<string, bool> map = new Dictionary<string, bool>();

            foreach (string s in g.getFiles())
            {
                map[s] = true;
            }

            S = get(g, "Small", 2 * trail / 3);
            M = get(g, "Medium", trail);

            if (map.ContainsKey("Header1"))
            {
                H1 = get(g, "Header1", trail);
            }
            else
            {
                H1 = M;
            }

            if (map.ContainsKey("Header2"))
            {
                H2 = get(g, "Header2", 2 * trail / 3);
            }
            else
            {
                H2 = M;
            }

            all = new ArrayList<Font>(new List<Font> { H2, H1, M, S });
        }

        private Font get(PATH g, string name, int trail)
        {
            if (g.exists(name))
                return new IFont(g.get(name))
                {
                    protected override Font init(ComposerUtil c, ComposerFonter f)
                    {
                        return f.save(0, 0, trail);
                    }
                }.get(trail);
            return M;
        }
    }
}