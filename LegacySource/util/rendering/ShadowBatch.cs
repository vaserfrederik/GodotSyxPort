using System;
using settlement.entity;
using snake2d;
using snake2d.util.file;
using snake2d.util.sprite;

namespace util.rendering
{
    public abstract class ShadowBatch : SPRITE_RENDERER
    {
        public abstract ShadowBatch setHeight(int height);
        public abstract ShadowBatch setHeightUI(double height);
        public abstract ShadowBatch setDistance2Ground(double height);
        public abstract ShadowBatch setDistance2GroundUI(double height);
        
        public abstract void set(Solid e);
        public abstract ShadowBatch setSoft();
        public abstract ShadowBatch setHard();
        public abstract ShadowBatch setPrev();
        
        public static class Real : ShadowBatch
        {
            protected double dD = 127;
            protected int iterations;
            
            protected int startX;
            protected int startY;
            protected double x;
            protected double y;
            protected double dx;
            protected double dy;
            protected bool bx;
            protected int lastHeight = -1;
            protected readonly byte SoftShadow = 127;
            protected readonly byte fullShadow = -1;
            protected byte streangth = SoftShadow;
            protected byte prev = streangth;
            protected int zoomout;
            
            protected double[] xs = new double[32];
            protected double[] ys = new double[32];
            protected double[] dds = new double[32];
            protected int[] iis = Alloc.ii(32);
            
            public void init(int zoomout, double dx, double dy)
            {
                x = dx;
                y = dy;
                
                bx = Math.Abs(x) > Math.Abs(y);
                this.zoomout = zoomout;
                
                for (int i = 0; i < iis.Length; i++)
                {
                    psetHeight(i);
                    xs[i] = this.dx;
                    ys[i] = this.dy;
                    iis[i] = iterations;
                    dds[i] = dD;
                }
            }
            
            private void psetHeight(int height)
            {
                lastHeight = height;
                
                
                dx = x * height;
                dy = y * height;
                
                if (bx)
                {
                    iterations = (int)Math.Abs(dx);
                }
                else
                {
                    iterations = (int)Math.Abs(dy);
                }
                
                iterations = (int)Math.Ceiling((double)iterations);
                //iterations /= C.SCALE;
                iterations = 1 + (iterations >> (zoomout));
                
                
                dx /= iterations;
                dy /= iterations;
                
                dD = (127.0 / iterations);
            }
            
            public override ShadowBatch setHeight(int height)
            {
                if (height == lastHeight)
                    return this;
                

                lastHeight = height;
                
                if (height < iis.Length)
                {
                    dx = xs[height];
                    dy = ys[height];
                    iterations = iis[height];
                    dD = dds[height];
                    return this;

                }
                
                psetHeight(height);

                return this;
            }
            
            public override ShadowBatch setHeightUI(double height)
            {
                dx = 0.5 * height;
                dy = 0.5 * height;
                
                if (bx)
                {
                    iterations = (int)Math.Abs(dx);
                }
                else
                {
                    iterations = (int)Math.Abs(dy);
                }
                
                iterations = (int)Math.Ceiling((double)height);
                //iterations /= C.SCALE;
                iterations = 1 + (iterations >> (zoomout));
                
                
                dx /= iterations;
                dy /= iterations;
                
                dD = (127.0 / iterations);
                return null;
            }
            
            public override ShadowBatch setDistance2Ground(double height)
            {
                startX = (int)(height * x);
                startY = (int)(height * y);
                return this;
            }
            
            public override void set(Solid e)
            {
                setHeight((int)e.getHeight());
                setDistance2Ground(e.getZ());
            }
            
            public override void renderSprite(int x1, int x2, int y1, int y2, TextureCoords texture)
            {
                x1 += startX;
                x2 += startX;
                y1 += startY;
                y2 += startY;
                
                int ix;
                int iy;
                double j = 0;
                if (startX + startY == 0)
                    j++;
                CORE.renderer().shadowDepthSet(streangth);
                while (j <= iterations)
                {
                    ix = (int)(dx * j);
                    iy = (int)(dy * j);
                    
                    CORE.renderer().renderShadow(x1 + ix, x2 + ix, y1 + iy, y2 + iy, texture, (byte)((j) * dD));
                    j++;
                }
            }

            public override ShadowBatch setSoft()
            {
                prev = streangth;
                streangth = SoftShadow;
                return this;
            }

            public override ShadowBatch setHard()
            {
                prev = streangth;
                streangth = fullShadow;
                return this;
            }

            public override ShadowBatch setDistance2GroundUI(double height)
            {
                startX = (int)(height);
                startY = (int)(height);
                return this;
            }
            
            public override ShadowBatch setPrev()
            {
                streangth = prev;
                CORE.renderer().shadowDepthSet(streangth);
                return this;
            }


            
        }
        

        public static readonly ShadowBatch DUMMY = new Dummy();
        
        public static class Dummy : ShadowBatch
        {
            public override ShadowBatch setHeight(int height)
            {
                // TODO Auto-generated method stub
                return this;
            }

            public override ShadowBatch setDistance2Ground(double height)
            {
                // TODO Auto-generated method stub
                return this;
                
            }

            public override void set(Solid e)
            {
                // TODO Auto-generated method stub
                
            }

            public override ShadowBatch setSoft()
            {
                // TODO Auto-generated method stub
                return this;
            }

            public override ShadowBatch setHard()
            {
                // TODO Auto-generated method stub
                return this;
            }

            public override ShadowBatch setDistance2GroundUI(double height)
            {
                // TODO Auto-generated method stub
                return null;
            }

            public override void renderSprite(int x1, int x2, int y1, int y2, TextureCoords texture)
            {
                // TODO Auto-generated method stub
                
            }

            public override ShadowBatch setPrev()
            {
                // TODO Auto-generated method stub
                return null;
            }

            public override ShadowBatch setHeightUI(double height)
            {
                // TODO Auto-generated method stub
                return null;
            }
            
        }
        
    }
    
}