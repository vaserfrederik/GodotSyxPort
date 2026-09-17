using System;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.misc
{
    public class ParticleRenderer : SettResource
    {
        private const int MAX = 64;
        private const float MAG = 16 * C.SCALE;
        private const float vel = 120f;
        private const double iV = 1.0 / (20.0 * C.TILE_SIZE);

        private readonly float[] mags = new float[MAX];
        private readonly float[] dxs = new float[MAX];
        private readonly float[] dys = new float[MAX];
        private readonly COLOR color = new ColorImp(18, 14, 5);
        private bool touched = false;

        public ParticleRenderer() : base("PART", false)
        {
            for (int i = 0; i < MAX; i++)
            {
                mags[i] = RND.rFloat() * MAX;
                DEG.setRandom();
                dxs[i] = (float)DEG.getCurrentX();
                dys[i] = (float)DEG.getCurrentY();
            }
        }

        public void renderDust(int x, int y, double magnitude)
        {
            magnitude *= iV;
            int m = (int)(magnitude * MAX);

            if (m > MAX)
                m = MAX;
            if (m <= 0)
                return;

            color.bind();
            for (int i = 0; i < m; i++)
            {
                int dx = (int)(x + dxs[i] * mags[i]);
                int dy = (int)(y + dys[i] * mags[i]);
                CORE.renderer().renderParticle(dx, dy);
            }
            COLOR.unbind();
            touched = true;
        }

        protected override void postRender(float ds)
        {
            if (!touched)
                return;

            touched = false;
            for (int i = 0; i < MAX; i++)
            {
                mags[i] += ds * vel;
                if (mags[i] > MAG)
                {
                    mags[i] -= MAG;
                }
            }
        }
    }
}