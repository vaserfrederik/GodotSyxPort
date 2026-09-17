using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Init.Race.Home
{
    public sealed class RaceHomeSheet
    {
        private readonly int[][] needed;
        private Sheets[] ani;

        public RaceHomeSheet() : this(new List<RES_AMOUNT>(), null, string.Empty, null)
        {
        }

        public RaceHomeSheet(List<RES_AMOUNT> resources, JObject json, string key, SheetType it) : this()
        {
            if (json == null || !json.ContainsKey(key))
            {
                needed = Alloc.i2(0, 0);
                ani = new Sheets[0];
            }
            else
            {
                JArray jsons = (JArray)json[key];
                needed = Alloc.i2(jsons.Count, resources.Count);
                ani = new Sheets[jsons.Count];

                for (int i = 0; i < jsons.Count; i++)
                {
                    JObject j = (JObject)jsons[i];
                    AddResource(resources, j, i, needed);
                    ani[i] = new Sheets(it, j);
                }
            }
        }

        private static void AddResource(List<RES_AMOUNT> resources, JObject json, int i, int[][] needed)
        {
            JObject j = (JObject)json[RESOURCES.KEYS];
            foreach (var k in j.Properties())
            {
                RESOURCE res = RESOURCES.Map().TryGet(k.Name);
                if (res != null)
                {
                    for (int ri = 0; ri < resources.Count; ri++)
                    {
                        if (resources[ri].Resource() == res)
                        {
                            needed[i][ri] = (int)j[k.Name];
                        }
                    }
                }
            }
        }

        public Sheets Get(HOME data)
        {
            outer:
            for (int ai = ani.Length - 1; ai >= 0; ai--)
            {
                int[] amounts = needed[ai];
                for (int i = 0; i < amounts.Length; i++)
                {
                    if (data.ResourceAm(i) < amounts[i])
                        continue outer;
                }
                return ani[ai];
            }
            return null;
        }
    }
}