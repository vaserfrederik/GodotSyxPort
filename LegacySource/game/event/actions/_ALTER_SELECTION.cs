using System;
using game.event.engine;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.actions
{
    internal class _ALTER_SELECTION : EventActionConstructor
    {
        _ALTER_SELECTION() : base("ALTER_SELECTION")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public class Imp : EventAction
        {
            private readonly string name;
            private readonly int gender;
            private double age;
            private double dirtiness;

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                name = data.Text("NAME", null);
                gender = data.I("GENDER", 0, int.MaxValue, -1);
                dirtiness = data.DTry("DIRTINESS", 0, 1, -1);
                data.CheckUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                ENTITY[] ee = SETT.ENTITIES().GetAllEnts();

                for (int ie = 0; ie < ee.Length; ie++)
                {
                    ENTITY entity = ee[ie];
                    if (!(entity is Humanoid))
                        continue;

                    Humanoid humanoid = (Humanoid)entity;
                    if (STATS.EVENT().Has(humanoid.Indu()))
                    {
                        Induvidual indu = humanoid.Indu();
                        if (name != null)
                            STATS.APPEARANCE().SetCustomName(indu, "Hotam Greattusk");
                        if (gender >= 0)
                            STATS.APPEARANCE().Gender.Set(indu, Math.Min(gender, STATS.APPEARANCE().Gender.Max(indu)));
                        if (age >= 0)
                            STATS.POP().Age.Dage.SetD(indu, age);
                        if (dirtiness >= 0)
                            STATS.NEEDS().DIRTINESS.SetD(indu, dirtiness);
                    }
                }
            }
        }
    }
}