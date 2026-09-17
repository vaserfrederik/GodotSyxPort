using System;
using System.Numerics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace snake2d
{
    class VboLightAmbient : VboAbsExt
    {
        private bool specialLayer;
        private readonly Shader shader;
        private readonly int[] sBuff;
        private readonly VboSorter sorter;

        public VboLightAmbient(SETTINGS sett) : base(PrimitiveType.Triangles, 1024 * 2 * 2,
            new VboAttribute(3, VertexAttribPointerType.Float, false, 4), // direction/centre position 3*4
            new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // coo 4
            new VboAttribute(3, VertexAttribPointerType.Float, false, 4), // colour 3*4
            new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1) // shaded + 3 padding
        )
        {
            shader = new Shader(sett.getNativeWidth(), sett.getNativeHeight(), "LightAmbient", null, "LightAmbient");
            shader.setUniform1i("Tdiffuse", 2);
            shader.setUniform1i("Tnormal", 3);
            sBuff = new int[buffer.Capacity / 4];
            sorter = new VboSorter(32 * MAX_ELEMENTS);
        }

        public void setNew()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        public void setNewButKeepLight()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = vFrom[current - 1];
        }

        public void setNewFinal()
        {
            specialLayer = true;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        public void flush()
        {
            sorter.fill(sBuff);
            buffer.position(sBuff.Length * 4);

            bind();
            upload();
            GlHelper.setBlendAdditative();
            GlHelper.enableDepthTest(true);
            GlHelper.setDepthTestLess();
            shader.bind();
            vTo[current] = count;
            for (int i = 0; i <= current; i++)
            {
                int fromI = vFrom[i];
                int toI = vTo[i];
                if (specialLayer && i == current)
                {
                    GlHelper.Stencil.setLEQUALKeepOnFail(i);
                }
                else
                {
                    GlHelper.Stencil.setEQUALKeepOnFail(i);
                }
                if (toI > fromI)
                {
                    flush(fromI, toI);
                }
            }

            GL.UseProgram(0);
            GlHelper.enableDepthTest(false);
            GlHelper.setBlendNormal();
            clear();
        }

        public void render(LIGHT_AMBIENT l, int x1, int x2, int y1, int y2, byte depth)
        {
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.x()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.y()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.z()), 0));
            sorter.add(current, ((y2) << 16) | ((x1 & 0x0FFFF)));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.r()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.g()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.b()), 0));
            sorter.add(current, (depth & 0x0FF) | 0xEFEFEF00);

            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.x()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.y()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.z()), 0));
            sorter.add(current, ((y2) << 16) | ((x2 & 0x0FFFF)));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.r()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.g()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.b()), 0));
            sorter.add(current, (depth & 0x0FF) | 0xEFEFEF00);

            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.x()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.y()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.z()), 0));
            sorter.add(current, ((y1) << 16) | ((x1 & 0x0FFFF)));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.r()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.g()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.b()), 0));
            sorter.add(current, (depth & 0x0FF) | 0xEFEFEF00);

            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.x()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.y()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes(l.z()), 0));
            sorter.add(current, ((y1) << 16) | ((x2 & 0x0FFFF)));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.r()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.g()), 0));
            sorter.add(current, BitConverter.ToInt32(BitConverter.GetBytes((float)l.b()), 0));
            sorter.add(current, (depth & 0x0FF) | 0xEFEFEF00);

            count++;
        }

        public override void dis()
        {
            shader.dis();
            base.dis();
        }

        public override void clear()
        {
            current = 0;
            specialLayer = false;
            sorter.clear();
            base.clear();
        }
    }
}