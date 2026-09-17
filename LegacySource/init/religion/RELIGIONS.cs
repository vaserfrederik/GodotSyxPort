using System;
using System.Collections.Generic;
using System.IO;
using init;
using init.paths;
using snake2d.util.sets;
using util.keymap;

namespace init.religion
{
    public sealed class RELIGIONS : InitResource
    {
        private static RELIGIONS self;
        private readonly ArrayListGrower<Religion> all = new ArrayListGrower<Religion>();
        private readonly RMAP<Religion> MAP;

        public RELIGIONS(INIT init) : base(init)
        {
            self = this;

            foreach (string k in PATHS.INIT().GetFolder("religion").GetFiles())
            {
                Religion r = new Religion(k, all.Size());
                all.Add(r);
            }

            MAP = new RMAP<Religion>("RELIGION", all);
            foreach (Religion r in all)
            {
                r.Init();
            }
        }

        public static LIST<Religion> ALL()
        {
            return self.all;
        }

        public static RMAP<Religion> MAP()
        {
            return self.MAP;
        }
    }
}