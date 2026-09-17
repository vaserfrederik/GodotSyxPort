using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Snake2D
{
    class VboLightPoint : VboAbsExt
    {
        private bool specialLayer;
        private readonly Shader shader;

        public VboLightPoint(SETTINGS sett)
            : base(PrimitiveType.Points, 10000 * 3,
                  new VboAttribute(3, VertexAttribPointerType.Float, false, 4), // direction/centre position 3*4
                  new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // coo 4
                  new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // coo2 4
                  new VboAttribute(4, VertexAttribPointerType.Float, false, 4), // colour 4*4
                  new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1), // coo intensity 4
                  new VboAttribute(1, VertexAttribPointerType.Float, false, 4), // radius 4
                  new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1)) // depth + 3 padding 4
        {
            shader = new Shader(sett.GetNativeWidth(), sett.GetNativeHeight(), "LightPoint", "LightPoint", "LightPoint");

            shader.SetUniform1i("Tdiffuse", 2);
            shader.SetUniform1i("Tnormal", 3);
        }

        public void SetNew()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        public void SetNewButKeepLight()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = vFrom[current - 1];
        }

        public void SetNewFinal()
        {
            specialLayer = true;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        public void Flush()
        {
            BindAndUpload();

            GlHelper.SetBlendAdditative();
            GlHelper.EnableDepthTest(true);
            GlHelper.SetDepthTestLess();
            shader.Bind();
            int i = 0;
            vTo[current] = count;
            while (i <= current)
            {
                if (specialLayer && i == current)
                {
                    GlHelper.Stencil.SetLEQUALKeepOnFail(i);
                }
                else
                {
                    GlHelper.Stencil.SetEQUALKeepOnFail(i);
                }
                Flush(vFrom[i], vTo[i]);
                i++;
            }
            GL.UseProgram(0);
            GlHelper.EnableDepthTest(false);
            GlHelper.SetBlendNormal();
            Clear();
        }

        public void Render(LIGHT_POINT l, float x, float y, float z, int radius, int x1, int x2, int y1, int y2, byte ne, byte se,
                          byte sw, byte nw, byte depth)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }

            float d = l.GetRadius();
            d /= radius;

            buffer.PutFloat(x).PutFloat(y).PutFloat(l.Cz());
            buffer.PutShort((short)x1).PutShort((short)y1).PutShort((short)x2).PutShort((short)y2);

            buffer.PutFloat(l.GetRed()).PutFloat(l.GetGreen()).PutFloat(l.GetBlue()).PutFloat(l.GetFalloff() * d);
            buffer.Put(nw).Put(ne).Put(se).Put(sw);
            buffer.PutFloat(radius);
            buffer.Put(depth);
            buffer.Put(Byte.MaxValue).Put(Byte.MaxValue).Put(Byte.MaxValue);

            count++;
        }

        public override void Dis()
        {
            shader.Dis();
            base.Dis();
        }

        public override void Clear()
        {
            current = 0;
            specialLayer = false;
            base.Clear();
        }
    }
}