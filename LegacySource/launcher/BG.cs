using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite;

namespace launcher
{
    class BG
    {
        private Rec quadBounds;
        private SPRITE map;
        private readonly SPRITE[] sprites;
        private Cloud[] clouds;
        private BigCloud[] bigClouds;
        private float cloudTimer;

        public BG(RES res)
        {
            map = res.bg;

            quadBounds = new Rec(0, Sett.WIDTH, 0, Sett.HEIGHT);

            sprites = res.clouds;
            clouds = new Cloud[15];
            for (int i = 0; i < clouds.Length; i++)
                clouds[i] = new Cloud(RND.rFloat() + 1f);
            bigClouds = new BigCloud[25];
            for (int i = 0; i < bigClouds.Length; i++)
                bigClouds[i] = new BigCloud(RND.rFloat() * 3 + 3f);
            cloudTimer = RND.rInt(50) + 50;
        }

        float s = 0;

        public void update(float ms)
        {
            s += ms;

            foreach (Cloud cloud in clouds)
                if (!cloud.update(ms))
                    cloud.reIni();

            releaseTheClouds(ms);

            foreach (BigCloud cloud in bigClouds)
                cloud.update(ms);
        }

        public void render(SPRITE_RENDERER r, float ds)
        {
            map.render(r, quadBounds);

            foreach (Cloud cloud in clouds)
                cloud.renderShadow(r, ds);
            foreach (Cloud cloud in clouds)
                cloud.render(r, ds);
        }

        public void renderClouds(SPRITE_RENDERER r, float ds)
        {
            foreach (BigCloud cloud in bigClouds)
                cloud.render(r, ds);
        }

        private void releaseTheClouds(float ds)
        {
            cloudTimer -= ds;
            if (cloudTimer < 0)
            {
                cloudTimer += RND.rInt(50) + 50;
                foreach (BigCloud c in bigClouds)
                    c.reIni();
            }
            //cloudRelease.play();
        }

        class Cloud : LSprite
        {
            private static readonly float ySpeed = -30f;
            private static readonly float xSpeed = 28f;
            private readonly float scale;
            private readonly Rec shadowBounds;
            private OpacityImp shadowOp;

            public Cloud(float scale) : base(sprites[RND.rInt(sprites.Length)], 0, 0)
            {
                this.scale = scale;
                body().scale(scale, scale);
                shadowBounds = new Rec(0, body().width() * scale, 0, body().height() * scale);
                shadowBounds.moveX1(-quadBounds.width() + 2 * RND.rInt(quadBounds.width()));
                shadowBounds.moveY1(RND.rInt((int)quadBounds.y2()));
                getOpacity().set(RND.rInt(255));
                shadowOp = new OpacityImp((int)(Byte.ToUInt16(getOpacity().get()) * 0.5));
                update(0);
            }

            public bool update(float ms)
            {
                shadowBounds.incrY(ySpeed * scale * ms);
                shadowBounds.incrX(xSpeed * scale * ms);
                body().moveX1(shadowBounds.x1());
                body().moveY1(shadowBounds.y1());
                return shadowBounds.touches(quadBounds);
            }

            public void reIni()
            {
                shadowBounds.moveX1(-quadBounds.width() + 2 * RND.rInt(quadBounds.width()));

                shadowBounds.moveY1(quadBounds.y2());
            }

            private void renderShadow(SPRITE_RENDERER r, float ds)
            {
                shadowOp.bind();
                COLOR.BLACK.bind();
                this.sprite.render(r, shadowBounds.x1(), shadowBounds.x2(), shadowBounds.y1(), shadowBounds.y2());
                OPACITY.unbind();
                COLOR.unbind();
            }
        }

        class BigCloud : LSprite
        {
            private static readonly float ySpeed = -55f;
            private static readonly float xSpeed = 38f;
            private readonly float scale;

            public BigCloud(float scale) : base(sprites[RND.rInt(sprites.Length)], 0, 0)
            {
                body().scale(scale, scale);
                this.scale = scale;
                reIni();
            }

            public void update(float ms)
            {
                body().incrY(ySpeed * scale * scale * ms);
                body().incrX(xSpeed * scale * ms);
            }

            public void reIni()
            {
                body().moveX1(-quadBounds.width() + 2 * RND.rInt((int)quadBounds.width()));
                body().moveY1(quadBounds.y2());
                getOpacity().set(RND.rInt(255));
            }
        }
    }
}