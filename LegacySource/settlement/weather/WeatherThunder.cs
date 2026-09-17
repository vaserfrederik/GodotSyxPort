using System;
using game;
using game.audio;
using game.time;
using init.settings;
using snake2d.util.light;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherThunder : WeatherThing
    {
        private readonly AmbientLight flash = new AmbientLight();
        private double flashI;
        private bool flashIs;

        private const float thunderTimer = 4f;
        private const float thunderLength = 0.2f;
        private float timer1 = RND.rFloat() * thunderTimer;
        private float timer2 = RND.rFloat() * thunderLength;
        private double soundTimer = 1;
        private double target;

        private static readonly string ¤¤name = "Thunder";
        private static readonly string ¤¤desc = "The amount of thunder.";

        private static readonly double speed = 1.0 / (TIME.secondsPerHour());

        static WeatherThunder()
        {
            D.ts(typeof(WeatherThunder));
        }

        public WeatherThunder() : base(¤¤name, ¤¤desc)
        {
        }

        public override void update(double ds)
        {
            setD(adjustTowards(getD(), ds * speed, target));

            flashIs = false;

            if (timer1 > 0)
            {
                timer1 -= getD() * ds;
            }
            else if (timer2 > 0)
            {
                timer2 -= ds;
                flashIs = true;
            }
            else
            {
                timer2 = RND.rFloat() * thunderLength;
                if (RND.rInt(4) == 0)
                {
                    timer1 = RND.rFloat() * thunderTimer;
                    flash.setDir(RND.rInt(360));
                    flash.setTilt(RND.rInt(10));
                    flashI = 0.5 + RND.rFloat() * 8;
                }
                else
                {
                    flashI /= 1.5;
                }
            }

            target = 0;
        }

        public void setTarget(double target)
        {
            this.target = CLAMP.d(target, 0, 1);
        }

        public void makeSounds(double gain, float ds)
        {
            soundTimer -= ds * getD() / GAME.SPEED.speed();

            if (soundTimer < 0)
            {
                soundTimer = 1 + RND.rFloat();
                AUDIO.AMBI().thunder.streams.rnd().playOnce();
            }
        }

        public void apply(int x1, int x2, int y1, int y2)
        {
            if (flashIs && S.get().graphics.get() == 1)
            {
                flash.r(flashI).g(flashI).b(flashI);
                flash.register(x1, x2, y1, y2);
            }
        }

        protected override void init()
        {
            setD(0);
        }
    }
}