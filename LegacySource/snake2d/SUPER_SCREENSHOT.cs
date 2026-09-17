using System;

namespace snake2d
{
    public abstract class SUPER_SCREENSHOT
    {
        private readonly int scale;
        private string fName;

        public SUPER_SCREENSHOT(int scale)
        {
            this.scale = scale;
        }

        public abstract int GetWidth();
        public abstract int GetHeight();
        public abstract bool RenderAndHasNext();
        public abstract void Init();
        // public abstract void ReceiveResult(SnakeImage image);

        public void Perform(string filePath)
        {
            this.fName = filePath;
            job.Perform();
        }

        private readonly GlJob job = new GlJob()
        {
            DoJob = () =>
            {
                SnakeImage image = new SnakeImage(GetWidth() / scale, GetHeight() / scale);

                Init();

                double x1 = 0;
                double y1 = 0;

                while (RenderAndHasNext())
                {
                    if (y1 >= GetHeight())
                        throw new RuntimeException();
                    CORE.Graphics.FlushRenderer();
                    CORE.Graphics.CopyFB(image, (int)Math.Round(x1 / scale), (int)Math.Round(y1 / scale), scale);
                    CORE.Graphics.PollEvents();
                    x1 += CORE.Graphics.NativeWidth;
                    if (x1 >= GetWidth())
                    {
                        y1 += CORE.Graphics.NativeHeight;
                        x1 = 0;
                    }
                }

                CORE.Graphics.PollEvents();

                image.SaveJpg(fName);
                image.Dispose();
                GC.Collect();
                CORE.Input.ClearAllInput();
            }
        };

        public double FileSizeMB()
        {
            double a = GetWidth() * GetHeight() / (scale * 1000000.0);
            a *= 0.2;
            return a;
        }
    }
}