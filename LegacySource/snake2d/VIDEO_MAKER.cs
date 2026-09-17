using System;
using System.IO;
using System.Diagnostics;
using snake2d.CORE;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;

public abstract class VIDEO_MAKER
{
    private readonly RECTANGLE start;
    private readonly RECTANGLE end;
    private readonly Rec tmp = new Rec();
    private readonly VectorImp vec = new VectorImp();
    private readonly int frames;
    private static double FPS = 60;
    private readonly string path;

    public VIDEO_MAKER(RECTANGLE start, RECTANGLE end, int duration, string path)
    {
        this.start = start;
        this.end = end;
        this.path = path;
        if (duration > 20000)
            duration = 20000;

        frames = (int)(FPS * duration / 1000);

        new GlJob(() =>
        {
            for (int f = 0; f <= frames; f++)
            {
                make(f);
            }

            makeVideo(path);
        }).Perform();
    }

    private void make(int frame)
    {
        Rec bounds = frame((double)frame / frames);

        int fw = CORE.GetGraphics().NativeWidth;
        int fh = CORE.GetGraphics().NativeHeight;

        int scale = bounds.Width() / fw;
        scale = CLAMP.I(scale, 1, 4);

        SnakeImage frameBuffers = new SnakeImage(bounds.Width() / scale, bounds.Height() / scale);

        for (int dy = 0; dy < bounds.Height(); dy += fh)
        {
            for (int dx = 0; dx < bounds.Width(); dx += fw)
            {
                int x1 = bounds.X1() + dx;
                int y1 = bounds.Y1() + dy;
                int w = fw;
                int h = fh;
                if (x1 + w > bounds.X2())
                    w = bounds.X2() - x1;
                if (y1 + h > bounds.Y2())
                    h = bounds.Y2() - y1;

                Rec r = new Rec(w, h);
                r.MoveX1Y1(x1, y1);

                render(r);
                CORE.GetGraphics().FlushRenderer();
                CORE.GetGraphics().CopyFB(frameBuffers, dx / scale, dy / scale, scale);
                CORE.GetGraphics().PollEvents();
            }
        }
        renderProgress(frame, frames, 1.0 / FPS);
        CORE.GetGraphics().PollEvents();

        SnakeImage result = frameBuffers.Resized(fw, fh);
        frameBuffers.Dispose();

        string id = string.Format("{0:D5}", frame);

        string p = path + id + ".jpg";
        result.SaveJpg(p);
        Console.WriteLine("saving " + p);
        result.Dispose();
        GC.Collect();
        CORE.GetInput().ClearAllInput();
    }

    private static void makeVideo(string path)
    {
        string command = "C:\\Users\\jakob\\Desktop\\jakob\\syx\\ffmpeg-master-latest-win64-gpl-shared\\bin\\";
        command += "ffmpeg -r " + FPS + " ";
        command += "-f image2 ";
        command += "-s " + CORE.GetGraphics().NativeWidth + "x" + CORE.GetGraphics().NativeHeight + " ";
        command += "-i " + path + "%05d.jpg ";
        command += "-vcodec libx264 -crf 25  -pix_fmt yuv420p ";
        command += path + "video.mp4";

        Console.WriteLine(command);
        try
        {
            if (File.Exists(path + "video.mp4"))
                File.Delete(path + "video.mp4");
            Process.Start(new ProcessStartInfo(command) { RedirectStandardOutput = true, UseShellExecute = false }).WaitForExit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        Console.WriteLine(path + "video.mp4");
    }

    private Rec frame(double dd)
    {
        tmp.SetWidth(start.Width() + (end.Width() - start.Width()) * dd);
        tmp.SetHeight(start.Height() + (end.Height() - start.Height()) * dd);

        double l = vec.Set(start.CX(), start.CY(), end.CX(), end.CY());
        l *= dd;
        tmp.MoveC(start.CX() + l * vec.NX(), start.CY() + l * vec.NY());
        return tmp;
    }

    public abstract void Render(RECTANGLE gamebounds);

    public abstract void RenderProgress(int frame, int totFrames, double frameTime);
}