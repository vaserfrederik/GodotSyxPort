using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Events.Slave
{
    public class EventUprising : EventResource
    {
        private static readonly string ¤¤warningA = "¤Slave Submission!";
        private static readonly string ¤¤warningAD = "¤Slaves are acting a bit out of order lately and submission seems to be low. Perhaps it would be a good thing to free a few to raise their spirits.";

        private static readonly string ¤¤warning = "¤Slave Warning!";
        private static readonly string ¤¤warningD = "¤Rumour has it that our wretched slaves feel mistreated. Some battlegear has also mysteriously gone missing. Might be a good time to deploy our troops close to the throne, just in case they think of something...";

        private static readonly string ¤¤riot = "¤Slave Uprising!";
        private static readonly string ¤¤riotD = "¤May the gods help us, the slaves are rising up to their masters! They claim to have had enough of your mistreatment and are now bent on ending your rule. Should they reach the throne, they will part with a good chunk of our riches and resources, and we shall be forever disgraced. Call in the troops and smite them, while there still is time!";
        private static readonly string ¤¤amount = "¤{0} slaves have joined the uprising.";
        private static readonly string ¤¤OverD = "¤The slave uprising has been defeated and your people rejoice at your might. Time to acquire new ones.";
        private static readonly string ¤¤Over = "¤Uprising crushed!";

        private static readonly string ¤¤LooseD = "¤The filthy slaves have captured the throne through low cunning. The wretches have plundered our stores and our treasury before deserting their master. May the gods help us through this calamity.";
        private static readonly string ¤¤Loose = "¤Slaves triumph!";

        private readonly double updateD = 5;
        private readonly double speed = updateD / (TIME.SecondsPerDay() * 8);

        private double tt = 0;
        private double acc = 0;
        private int state;
        private int amountTotal;
        public readonly UprisingSpots spots = new UprisingSpots();

        static EventUprising()
        {
            D.ts(typeof(EventUprising));
        }

        public EventUprising()
        {
            base("SLAVES");
            IDebugPanelSett.Add("Slave Uprising", new ACTION
            {
                exe = () => riot()
            });

            Clear();
        }

        protected override void Save(FilePutter file)
        {
            file.Write(state);
            file.Write(amountTotal);
            file.Write(acc);
            spots.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            state = file.ReadI();
            amountTotal = file.ReadI();
            acc = file.ReadD();
            spots.Load(file);
        }

        protected override void Clear()
        {
            state = 0;
            amountTotal = 0;
            spots.Clear();
            acc = 0;
        }

        protected override void Update(double ds)
        {
            if (state == 0)
            {
                if (GAME.ARMIES().Enemy().Men > 0)
                    return;
                if (STATS.POP().Pop(HTYPES.SLAVE()) <= 0)
                {
                    acc = 0;
                    return;
                }
                tt -= ds;
                if (tt > 0)
                    return;
                tt += updateD;

                double chance = Math.Max(STANDINGS.SLAVE().Current, STANDINGS.SLAVE().Target);
                chance = Math.Max(0, chance);
                if (chance >= 0.8)
                {
                    acc -= speed * (chance - 0.8) / 0.2;
                    acc = Math.Clamp(acc, 0, 1);
                    return;
                }
                if (chance < 0)
                    chance = 0;

                chance /= 0.8;
                chance = 1.0 - chance;
                chance = Math.Pow(chance, 0.5);

                double old = acc;
                acc += speed * chance;

                if (acc > 1)
                {
                    acc = 0;
                    Riot();
                }
                else if (old < 0.8 && acc > 0.8)
                {
                    new MessageText(¤¤warningA, ¤¤warningAD).Send();
                }
            }
            else if (state == 1)
            {
                if (spots.Update(ds))
                {
                    state = 2;
                    var t = Str.TMP;
                    t.Clear();
                    t.Add(¤¤amount).Insert(0, amountTotal);
                    acc = 0;
                    new MessageText(¤¤riot, ¤¤riotD).Paragraph(t).Send();
                }
            }
            else if (state == 2)
            {
                spots.Update(ds);
                if (!spots.HasMore())
                {
                    state = 3;
                }
            }
            else if (state == 3)
            {
                if (GAME.ARMIES().Enemy().Men == 0)
                {
                    foreach (var e in SETT.ENTITIES().GetAllEnts())
                    {
                        if (e is Humanoid h)
                        {
                            if (h.Indu().HType == HTYPES.ENEMY())
                            {
                                h.HTypeSet(HTYPES.SLAVE(), null, null);
                            }
                        }
                    }
                    new MessageText(¤¤Over, ¤¤OverD).Send();
                    state = 0;
                    return;
                }

                var c = THRONE.Coo;
                bool enemy = false;
                bool player = false;
                for (int x = c.X - 3; x < c.X + 3; x++)
                {
                    for (int y = c.Y - 3; y < c.Y + 3; y++)
                    {
                        foreach (var e in SETT.ENTITIES().GetAtTile(x, y))
                        {
                            if (e is Humanoid h)
                            {
                                if (h.Indu().HType == HTYPES.ENEMY() && h.Division != null)
                                {
                                    enemy |= true;
                                    if (h.Indu().Clas == HCLASSES.CITIZEN() && h.Division != null)
                                        player |= true;
                                }
                            }
                        }
                    }
                }
                if (enemy && !player)
                    Loose();
                return;
            }
        }

        private void Loose()
        {
            acc = 0;
            double am = 0.25 + 0.05 * RND.rInt(6);

            RESOURCE.Remove(am, RBIT.ALL, RTYPE.SPOILS);

            int creds = FACTIONS.Player().Credits.Credits > 0 ? (int)(FACTIONS.Player().Credits.Credits * 0.75) : 0;
            FACTIONS.Player().Credits.Inc(-creds, CTYPE.MISC);
            state = 0;

            foreach (var e in SETT.ENTITIES().GetAllEnts())
            {
                if (e is Humanoid h)
                {
                    if (h.Indu().HType == HTYPES.ENEMY())
                    {
                        h.HelloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                    }
                }
            }
            new MessageText(¤¤Loose, ¤¤LooseD).Send();
        }

        private void Riot()
        {
            if (state != 0)
                return;

            double amd = 0.2 + RND.rFloat() * 0.8;
            int am = spots.Riot(amd);

            if (am == 0)
                return;

            amountTotal = am;
            state = 1;
            new MessageText(¤¤warning, ¤¤warningD).Send();
        }
    }
}