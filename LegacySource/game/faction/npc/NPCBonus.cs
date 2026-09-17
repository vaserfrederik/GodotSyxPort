using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Faction.NPC
{
    public class NPCBonus : NPCResource, DOUBLE_O<Boostable>
    {
        private const int MM = 128 - 1;
        private readonly double[] bos = new double[128];

        public readonly FactionNPC faction;

        public NPCBonus(FactionNPC faction, List<NPCResource> all) : base(all)
        {
            Randomize();
            this.faction = faction;
            if (faction.Index() == 5)
            {
                IDebugPanel.Add("Faction bonus max test", new ACTION
                {
                    Exe = () =>
                    {
                        double[] back = new double[128];
                        for (int i = 0; i < bos.Length; i++)
                        {
                            back[i] = bos[i];
                            bos[i] = 1.0;
                        }

                        foreach (Boostable b in BOOSTING.ALL())
                        {
                            LOG.Ln($"{b.Key} {b.Get(faction)}");
                        }

                        for (int i = 0; i < bos.Length; i++)
                        {
                            bos[i] = back[i];
                        }
                    }
                });
            }
        }

        public override double GetD(Boostable bo)
        {
            return Get(bo.Index());
        }

        public void Clear()
        {
            for (int i = 0; i < bos.Length; i++)
            {
                bos[i] = 0.0;
            }
        }

        public double Get(int ran)
        {
            int ii = ran & MM;
            if (faction.Court().King() == null || faction.Court().King().Roy() == null)
                return bos[ii] * 0.5;
            double c = (0.5 + 0.25 * BOOSTABLES.NOBLE().COMPETANCE.Get(faction.Court().King().Roy().Induvidual));
            return CLAMP.D(bos[ii] * c, 0, 1);
        }

        public void Randomize()
        {
            for (int i = 0; i < bos.Length; i++)
            {
                bos[i] = 0.1 + 0.9 * i / (bos.Length - 1);
                bos[i] = CLAMP.D(bos[i], 0, 1);
            }

            for (int i = 0; i < bos.Length; i++)
            {
                double d = bos[i];
                int k = RND.RInt(bos.Length);
                bos[i] = bos[k];
                bos[k] = d;
            }
        }

        protected override SAVABLE Saver()
        {
            return new SAVABLE
            {
                Save = file =>
                {
                    file.Ds(bos);
                },
                Load = file =>
                {
                    file.Ds(bos);
                },
                Clear = () =>
                {
                }
            };
        }

        protected override void Update(FactionNPC faction, double seconds)
        {
            // TODO Auto-generated method stub
        }

        protected override void Generate(RDRace race, FactionNPC faction, bool fromScratch)
        {
            Randomize();
        }
    }
}