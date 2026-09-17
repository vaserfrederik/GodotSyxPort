using OpenTK.Graphics.OpenGL;
using System;

namespace snake2d
{
    class VboTileLight : VboAbsExt
    {
        public const int MAX_ELEMENTS = 65536 / 4;
        private float[] lights = new float[255 * 7];
        private readonly float inv = 1f / 255f;
        private readonly Shader shader;
        private bool specialLayer;

        private readonly int uTilt;
        private readonly int uColor;
        private readonly int uDepth;

        public VboTileLight(SETTINGS sett)
            : base(PrimitiveType.Points, MAX_ELEMENTS, new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // position upper left //4
                  new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1), // corner intensity //4
                  new VboAttribute(2, VertexAttribPointerType.Short, false, 2) // dimension + 2 padding
                  )
        {
            shader = new Shader(sett.NativeWidth, sett.NativeHeight, "LightTile", "LightTile", "LightTile");

            shader.SetUniform1i("Tdiffuse", 2);
            shader.SetUniform1i("Tnormal", 3);

            uTilt = shader.GetUniformLocation("v_tilt");
            uColor = shader.GetUniformLocation("v_color");
            uDepth = shader.GetUniformLocation("u_depth");
        }

        void Upload(float r, float g, float b, float x, float y, float z, float depth)
        {
            shader.SetUniform(uColor, r, g, b);
            shader.SetUniform(uTilt, x, y, z);
            shader.SetUniform(uDepth, depth);
        }

        void SetNew()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        void SetNewButKeepLight()
        {
            if (specialLayer)
                return;
            vTo[current] = count;
            current++;
            vFrom[current] = vFrom[current - 1];
        }

        void SetNewFinal()
        {
            specialLayer = true;
            vTo[current] = count;
            current++;
            vFrom[current] = count;
        }

        void Flush()
        {
            BindAndUpload();

            GlHelper.SetBlendAdditive();
            GlHelper.EnableDepthTest(true);
            GlHelper.SetDepthTestLess();
            shader.Bind();
            int i = 0;
            vTo[current] = count;
            while (i <= current)
            {
                if (vFrom[i] == vTo[i])
                {
                    i++;
                    continue;
                }
                if (specialLayer && i == current)
                {
                    GlHelper.Stencil.SetLEQUALKeepOnFail(i);
                }
                else
                {
                    GlHelper.Stencil.SetEQUALKeepOnFail(i);
                }
                int k = i * 7;
                Upload(lights[k], lights[k + 1], lights[k + 2], lights[k + 3], lights[k + 4], lights[k + 5],
                      lights[k + 6]);
                Flush(vFrom[i], vTo[i]);
                i++;
            }
            GL.UseProgram(0);
            GlHelper.EnableDepthTest(false);
            GlHelper.SetBlendNormal();
            Clear();
        }

        public override void Clear()
        {
            base.Clear();
            specialLayer = false;
        }

        void Render(int x1, int y1, int dim, byte nw, byte ne, byte se, byte sw)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }

            buffer.Put((short)x1).Put((short)y1);
            buffer.Put(nw).Put(ne).Put(se).Put(sw);
            buffer.Put((short)dim);
            buffer.Put(sbyte.MaxValue).Put(sbyte.MaxValue);

            count++;
        }

        void SetLight(float red, float green, float blue, float x, float y, float z, byte depth)
        {
            int i = current * 7;
            lights[i] = red;
            lights[i + 1] = green;
            lights[i + 2] = blue;
            lights[i + 3] = x;
            lights[i + 4] = y;
            lights[i + 5] = z;
            lights[i + 6] = (depth & 0x0FF) * inv;
        }

        public override void Dis()
        {
            shader.Dis();
            base.Dis();
        }
    }
}