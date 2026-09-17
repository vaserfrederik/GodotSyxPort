using System;
using System.Collections.Generic;

namespace Settlement.Entity.Humanoid
{
    public enum HPoll
    {
        DEFENCE_SKILL,
        PARRY_SKILL,
        WILL_COLLIDE_WITH,
        COLLIDES,
        SCARE_ANIMAL_NOT,
        BATTLE_READY,
        IMPACT_DAMAGE,
        WORKING,
        IS_ENEMY,
        IS_SLAVE_READY_FOR_UPRISING,
        CAN_INTERRACT,
    }

    public static class HPollData
    {
        public HPoll Type { get; set; }
        public double Adx { get; set; }
        public double Ady { get; set; }
        public double FacingDot { get; set; }
        public ENTITY Other { get; set; }
        public ECollision Colli { get; set; }
        public ECollision Damage { get; set; }
        public bool IsEnemy { get; set; }

        private HPollData()
        {
        }
    }

    public static class Handler
    {
        private Handler()
        {
        }

        public static bool ScaresAnimal(Humanoid a)
        {
            HPollData.Poll.Type = HPoll.SCAR_ANIMAL_NOT;
            return a.ai.Poll(a, HPollData.Poll) == 0;
        }

        public static void CollideDamage(Humanoid a, AIManager ai, ECollision coll, ECollision damage)
        {
            bool isEnemy = IsEnemy(a, coll.Other);
            HPollData.Poll.Type = HPoll.IMPACT_DAMAGE;
            HPollData.Poll.Colli = coll;
            HPollData.Poll.Damage = damage;
            HPollData.Poll.IsEnemy = isEnemy;

            a.ai.Poll(a, HPollData.Poll);
        }

        public static bool Collides(Humanoid a, AIManager ai, ENTITY o)
        {
            HPollData.Poll.Other = o;
            HPollData.Poll.Type = HPoll.COLLIDES;

            return ai.Poll(a, HPollData.Poll) == 1;
        }

        public static bool WillCollideWith(Humanoid a, AIManager ai, ENTITY other)
        {
            HPollData.Poll.Type = HPoll.WILL_COLLIDE_WITH;
            HPollData.Poll.Other = other;

            return ai.Poll(a, HPollData.Poll) == 1;
        }

        public static double DefenseSkill(Humanoid a, double faceDot, double adx, double ady)
        {
            HPollData.Poll.FacingDot = faceDot;
            HPollData.Poll.Adx = adx;
            HPollData.Poll.Ady = ady;
            HPollData.Poll.Type = HPoll.DEFENCE_SKILL;
            return a.ai.Poll(a, HPollData.Poll);
        }

        public static double ParrySkill(Humanoid a, double dot, double adx, double ady)
        {
            HPollData.Poll.FacingDot = dot;
            HPollData.Poll.Adx = adx;
            HPollData.Poll.Ady = ady;
            HPollData.Poll.Type = HPoll.PARRY_SKILL;
            return a.ai.Poll(a, HPollData.Poll);
        }

        public static bool Works(Humanoid a)
        {
            HPollData.Poll.Type = HPoll.WORKING;
            return a.ai.Poll(a, HPollData.Poll) == 1;
        }

        public static int IsSlaveReadyForUprising(Humanoid a)
        {
            HPollData.Poll.Type = HPoll.IS_SLAVE_READY_FOR_UPRISING;
            return (int)a.ai.Poll(a, HPollData.Poll);
        }

        public static bool IsEnemy(Humanoid a, ENTITY other)
        {
            HPollData.Poll.Type = HPoll.IS_ENEMY;
            HPollData.Poll.Other = other;
            return a.ai.Poll(a, HPollData.Poll) == 1;
        }

        public static bool CanInteract(Humanoid a, ENTITY other)
        {
            HPollData.Poll.Type = HPoll.CAN_INTERRACT;
            HPollData.Poll.Other = other;
            return a.ai.Poll(a, HPollData.Poll) == 1;
        }
    }

    public static class HPollExtensions
    {
        private static readonly List<HPoll> _all = new List<HPoll>(Enum.GetValues(typeof(HPoll)));
        public static List<HPoll> All => _all;
    }
}