using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Snake2D
{
    class VboSprite : VboAbs
    {
        private readonly Shader shader;
        private readonly int[] sBuff;
        private readonly VboSorter sorter;
        private int layer = 0;

        public static VboSprite GetDebug(SETTINGS sett)
        {
            Shader shader = new Shader(sett.NativeWidth, sett.NativeHeight, "SpritePoint", "SpritePoint", "SpritePoint_debug");
            shader.SetUniform1i("u_texture", 0);
            return new VboSprite(shader);
        }

        public static VboSprite GetDeferred(SETTINGS sett)
        {
            Shader shader = new Shader(sett.NativeWidth, sett.NativeHeight, "SpritePoint", "SpritePoint", "SpritePoint");
            shader.SetUniform1i("sampler1", 0);
            shader.SetUniform1i("sampler2", 1);
            return new VboSprite(shader);
        }

        public VboSprite(Shader shader) : base(
            PrimitiveType.Points,
            1 << 17,
            new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // position upper left
            new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // position lower right
            new VboAttribute(2, VertexAttribPointerType.UnsignedShort, 2), // texture coords1
            new VboAttribute(2, VertexAttribPointerType.UnsignedShort, 2), // texture coords2
            new VboAttribute(2, VertexAttribPointerType.UnsignedShort, 2), // texture coords width
            new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1) // color
        )
        {
            this.shader = shader;
            sorter = new VboSorter(MAX_ELEMENTS * 6);
            sBuff = new int[buffer.Length];
            Marshal.Copy(buffer, sBuff, 0, buffer.Length);
        }

        public int SetNew()
        {
            layer++;
            return layer;
        }

        public void Flush()
        {
            Bind();
            shader.Bind();
            buffer.Position(0);
            Counts ss = sorter.Fill(sBuff);
            buffer.Position(sBuff.Length);
            Upload();

            for (int i = 0; i <= layer; i++)
            {
                int fromI = ss.from[i];
                int toI = ss.to[i];
                if (toI > fromI)
                {
                    GlHelper.Stencil.SetLEQUALReplaceOnPass(i);
                    Flush(fromI / 6, toI / 6);
                }
            }

            Clear();
            GL.UseProgram(0);
        }

        public override void Clear()
        {
            sorter.Clear();
            layer = 0;
            base.Clear();
        }

        private void Render(TextureCoords t, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, COLOR color, OPACITY opacity)
        {
            Render(t, t, x1, x2, y1, y2, color, opacity);
        }

        private void Render(TextureCoords t, TextureCoords to, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            VboSorter sorter = this.sorter;
            sorter.Add(layer, ((y1) << 16) | ((x1 & 0x0FFFF)));
            sorter.Add(layer, ((y2) << 16) | ((x2 & 0x0FFFF)));
            sorter.Add(layer, ((t.y1) << 16) | ((t.x1)));
            sorter.Add(layer, ((to.y1) << 16) | ((to.x1)));
            sorter.Add(layer, ((t.y2 - t.y1) << 16) | ((t.x2 - t.x1)));
            sorter.Add(((opacity.Get()) << 24) | ((color.Blue & 0x0FF) << 16) | ((color.Green & 0x0FF) << 8) | ((color.Red & 0x0FF)));
        }

        public override void Dis()
        {
            shader.Dis();
            base.Dis();
        }
    }
}