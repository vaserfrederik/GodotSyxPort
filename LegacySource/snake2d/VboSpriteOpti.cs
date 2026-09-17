using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace snake2d
{
    final class VboSpriteOpti : VboAbsExt
    {
        private readonly Shader shader;
        private readonly int[] opti;
        private int optiI;
        private readonly int[] sBuff;

        static VboSpriteOpti GetDebug(SETTINGS sett)
        {
            Shader shader = new Shader(sett.GetNativeWidth(), sett.GetNativeHeight(), "SpritePoint", "SpritePoint", "SpritePoint_debug");
            shader.SetUniform1i("u_texture", 0);
            return new VboSpriteOpti(shader);
        }

        static VboSpriteOpti GetDeffered(SETTINGS sett)
        {
            Shader shader = new Shader(sett.GetNativeWidth(), sett.GetNativeHeight(), "SpritePoint", "SpritePoint", "SpritePoint");
            shader.SetUniform1i("sampler1", 0);
            shader.SetUniform1i("sampler2", 1);
            return new VboSpriteOpti(shader);
        }

        public VboSpriteOpti(Shader shader) : base(
            PrimitiveType.Points,
            1 << 17,
            new VboAttribute(2, All.Short, false, 2), // position upper left
            new VboAttribute(2, All.Short, false, 2), // position lower right
            new VboAttribute(2, All.UnsignedShort, 2), // texture coords1
            new VboAttribute(2, All.UnsignedShort, 2), // texture coords2
            new VboAttribute(2, All.UnsignedShort, 2), // texture coords width
            new VboAttribute(4, All.UnsignedByte, true, 1) // color
        )
        {
            this.shader = shader;
            opti = Alloc.Ii(MAX_ELEMENTS * 6);
            sBuff = new int[buffer.Length];
        }

        int SetNew()
        {
            vTo[current] = count;
            current++;
            vFrom[current] = count;
            return current;
        }

        void Flush()
        {
            if (count == 0)
            {
                Clear();
                return;
            }

            int off = 0;
            Bind();
            shader.Bind();
            vTo[current] = count;
            while (count > 0)
            {
                int am = count;
                if (am > 0x01000)
                    am = 0x01000;

                Array.Copy(opti, off * 6, sBuff, 0, am * 6);
                buffer.Position = sBuff.Length;
                sBuff.Clear();
                optiI = 0;
                Upload();
                shader.Bind();

                for (int i = 0; i <= current; i++)
                {
                    int f = vFrom[i] - off;
                    int t = vTo[i] - off;
                    if (t < 0)
                        continue;
                    if (f < 0)
                        f = 0;
                    if (t <= f)
                        continue;

                    GlHelper.Stencil.SetLEQUALreplaceOnPass(i);
                    Flush(f, t);
                }
                off += am;
                count -= am;
            }

            Clear();
            GL.UseProgram(0);
        }

        override public void Clear()
        {
            optiI = 0;
            base.Clear();
        }

        final void Render(TextureCoords t, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, COLOR color, OPACITY opacity)
        {
            Render(t, t, x1, x2, y1, y2, color, opacity);
        }

        final void Render(TextureCoords t, TextureCoords to, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }

            opti[optiI] = ((y1) << 16) | ((x1 & 0x0FFFF));
            opti[optiI + 1] = ((y2) << 16) | ((x2 & 0x0FFFF));

            opti[optiI + 2] = ((t.y1) << 16) | ((t.x1));
            opti[optiI + 3] = ((to.y1) << 16) | ((to.x1));

            opti[optiI + 4] = ((t.y2 - t.y1) << 16) | ((t.x2 - t.x1));

            opti[optiI + 5] = (((opacity.Get()) << 24) | ((color.Blue() & 0x0FF) << 16) | ((color.Green() & 0x0FF) << 8) | ((color.Red() & 0x0FF)));

            optiI += 6;
            count++;
        }

        override public void Dis()
        {
            shader.Dis();
            base.Dis();
        }
    }
}