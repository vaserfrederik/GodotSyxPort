using System;
using OpenTK.Graphics.OpenGL;
using snake2d.util.file;
using snake2d.util.sprite;

namespace snake2d
{
    class VboStencilMaxSetter : VboAbsExt
    {
        private readonly int[] fromStencil = Alloc.ii(255);
        private readonly Shader shader;

        public VboStencilMaxSetter(SETTINGS sett)
            : base(BeginMode.Points, 1 << 16,
                  new VboAttribute(2, All.Short, false, 2), // position upper left         4
                  new VboAttribute(2, All.Short, false, 2), // position lower right        4
                  new VboAttribute(2, All.UnsignedShort, 2), // texture coords1           4
                  new VboAttribute(2, All.UnsignedShort, 2), // texture coords2           4
                  new VboAttribute(4, All.UnsignedByte, true, 1) // d + depth + 2padding     4
                 )
        {
            this.shader = new Shader(sett.getNativeWidth(), sett.getNativeHeight(), "Shadow", "Shadow", "Shadow");
            shader.setUniform1i("sampler1", 0);
        }

        public int setNewFinalOverride(int fromStencil)
        {
            vTo[current] = count;
            current++;
            vFrom[current] = count;
            this.fromStencil[current] = fromStencil;
            return current;
        }

        public void flush()
        {
            if (count != 0)
            {
                bindAndUpload();
                GlHelper.enableDepthTest(false);
                GlHelper.setDepthTestAlways();
                GL.ColorMask(false, false, false, false);
                shader.bind();
                int i = 0;
                vTo[current] = count;
                while (i <= current)
                {
                    GL.StencilFunc(StencilFunction.Lequal, fromStencil[i], ~0);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Invert);

                    flush(vFrom[i], vTo[i]);
                    i++;
                }
                GL.UseProgram(0);
                GL.ColorMask(true, true, true, true);
            }
            clear();
        }

        public void render(TextureCoords t, int x1, int y1, int x2, int y2, int stencil)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }

            buffer.Put((short)x1).Put((short)y1);
            buffer.Put((short)x2).Put((short)y2);
            buffer.Put((short)t.x1).Put((short)t.y1);
            buffer.Put((short)t.x2).Put((short)t.y2);
            buffer.Put(byte.MaxValue);
            buffer.Put(byte.MaxValue);
            buffer.Put(byte.MaxValue);
            buffer.Put(byte.MaxValue);
            count++;
        }

        public override void dis()
        {
            shader.dis();
            base.dis();
        }
    }
}