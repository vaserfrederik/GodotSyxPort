using System;
using game.raiding;
using init.race.appearence;
using settlement.stats;
using snake2d;

namespace game.raiding
{
    public class RaiderPortrait : SPRITE.Imp
    {
        private Induvidual raider;
        private bool dead;

        public RaiderPortrait(int scale) : base(RPortrait.P_WIDTH * scale, RPortrait.P_HEIGHT * scale)
        {
        }

        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            if (raider() == null)
                return;
            int scale = (X2 - X1) / RPortrait.P_WIDTH;
            render(r, X1, Y1, scale, raider(), dead);
        }

        public RaiderPortrait set(Induvidual indu)
        {
            this.raider = indu;
            return this;
        }

        public RaiderPortrait set(Raider indu)
        {
            this.raider = indu.indu;
            return this;
        }

        public RaiderPortrait dead(bool dead)
        {
            this.dead = dead;
            return this;
        }

        public static void render(SPRITE_RENDERER r, int X1, int Y1, int scale, Induvidual indu, bool dead)
        {
            int dd = STATS.APPEARANCE().dead.indu().get(indu);
            if (dead)
                STATS.APPEARANCE().dead.indu().set(indu, 1);
            STATS.APPEARANCE().portraitRender(r, indu, X1, Y1, scale);
            indu.race().appearance().crown.raiders().getC(STATS.RAN().get(indu, 9)).renderScaled(r, X1, Y1 + 8 * scale, scale);
            STATS.APPEARANCE().dead.indu().set(indu, dd);
        }

        protected Induvidual raider()
        {
            return raider;
        }
    }
}