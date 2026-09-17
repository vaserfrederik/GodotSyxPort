using System;
using OpenTK.Graphics.OpenGL;
using snake2d.util.color;
using snake2d.util.file;

class VboParticles : VboAbsExt
{
    private readonly byte byteZero = 0;
    private readonly byte byteFull = -1;
    private readonly int[] size = Alloc.ii(255);
    private readonly Shader shader;

    static VboParticles GetDebug(SETTINGS sett)
    {
        Shader shader = new Shader(sett.getNativeWidth(), sett.getNativeHeight(), "Particle_debug", null, "Particle_debug");
        return new VboParticles(shader);
    }

    static VboParticles GetForTexture(int width, int height)
    {
        Shader shader = new Shader(width - 0.5f, height + 0.5f, "Particle_texture", null, "Particle_texture");
        return new VboParticles(shader);
    }

    static VboParticles GetDeffered(SETTINGS sett)
    {
        Shader shader = new Shader(sett.getNativeWidth(), sett.getNativeHeight(), "Particle", null, "Particle");
        return new VboParticles(shader);
    }

    public VboParticles(Shader shader) : base(PrimitiveType.Points, 1 << 17,
            new VboAttribute(2, All.Short, false, 2), // position		4
            new VboAttribute(4, All.UnsignedByte, true, 1), // normal	4
            new VboAttribute(4, All.UnsignedByte, true, 1)) // color	4
    {
        this.shader = shader;
        size[0] = 1;
    }

    void SetNew(int pointSize)
    {
        vTo[current] = count;
        current++;
        vFrom[current] = count;
        size[current] = pointSize;
    }

    final void Flush(int pointSize)
    {
        BindAndUpload();
        shader.Bind();
        int i = 0;
        vTo[current] = count;
        while (i <= current)
        {
            if (vFrom[i] != vTo[i])
            {
                GlHelper.Stencil.SetLEQUALreplaceOnPass(i);
                Flush(vFrom[i], vTo[i], size[i]);
            }
            i++;
        }
        Clear(pointSize);
        GL.UseProgram(0); // puts an end to the goddamn nvidia errors
    }

    public int Count()
    {
        return buffer.Position;
    }

    private void Flush(int from, int to, int size)
    {
        if (size < 1)
            throw new RuntimeException();
        GL.PointSize(size);
        GL.DrawElements(BeginMode.Points, to - from, DrawElementsType.UnsignedInt, from * 4);
    }

    public void Clear(int pointSize)
    {
        base.Clear();
        size[current] = pointSize;
    }

    public void Render(short x, short y, byte nX, byte nY, byte nZ, byte nA, COLOR color, OPACITY opacity)
    {
        if (count >= MAX_ELEMENTS)
        {
            return;
        }

        buffer.Put(x).Put(y);
        buffer.Put(nX).Put(nY).Put(nZ).Put(nA);
        buffer.Put(color.Red()).Put(color.Green()).Put(color.Blue()).Put(opacity.Get());

        count++;
    }

    public void Render(short x, short y, byte red, byte green, byte blue)
    {
        if (count >= MAX_ELEMENTS)
        {
            return;
        }

        buffer.Put(x).Put(y);
        buffer.Put(byteZero).Put(byteZero).Put(byteZero).Put(byteZero);
        buffer.Put(red).Put(green).Put(blue).Put(byteFull);

        count++;
    }

    public override void Dis()
    {
        shader.Dis();
        base.Dis();
    }

    public void Dis(bool leaveIndexArrayTheFuckAlone)
    {
        shader.Dis();
        base.Dis();
    }
}