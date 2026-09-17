using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace snake2d
{
    class VboLightPointUni : VboAbsExt
    {
        public const int MAX_ELEMENTS = 65536 / 4;
        private float[] lights = new float[255 * 5];
        private readonly float inv = 1f / 255f;
        private readonly Shader shader;
        private bool specialLayer;

        private readonly int uColor;
        private readonly int uShaded;
        private readonly int uFalloff;

        public VboLightPointUni(SETTINGS sett) : base(GL_POINTS, MAX_ELEMENTS, new VboAttribute(4, All.Short, false, 2)) //centre
        {
            shader = new Shader(sett.getNativeWidth(), sett.getNativeHeight(), "LightPointUni", "LightPointUni", "LightPointUni");

            shader.setUniform1i("Tdiffuse", 2);
            shader.setUniform1i("Tnormal", 3);
            uColor = shader.getUniformLocation("u_color");
            uShaded = shader.getUniformLocation("u_shaded");
            uFalloff = shader.getUniformLocation("falloff");
        }

        public void upload(float r, float g, float b, float shaded, float falloff)
        {
            shader.setUniform(uShaded, shaded);
            shader.setUniform(uColor, r, g, b);
            shader.setUniform(uFalloff, falloff);
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
            bindAndUpload();

            GlHelper.setBlendAdditative();
            GlHelper.enableDepthTest(true);
            GlHelper.setDepthTestLess();
            shader.bind();
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
                    GlHelper.Stencil.setLEQUALKeepOnFail(i);
                }
                else
                {
                    GlHelper.Stencil.setEQUALKeepOnFail(i);
                }
                int k = i * 5;
                upload(lights[k], lights[k + 1], lights[k + 2], lights[k + 3], lights[k + 4]);
                flush(vFrom[i], vTo[i]);
                i++;
            }
            GL.UseProgram(0);
            GlHelper.enableDepthTest(false);
            GlHelper.setBlendNormal();
            clear();
        }

        public void render(short x, short y, short z, short radius)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }
            buffer.put(x).put(y).put(z).put(radius);

            count++;
        }

        public void setLight(float radius, float red, float green, float blue, float falloff, byte depth)
        {
            int i = current * 5;
            lights[i] = red;
            lights[i + 1] = green;
            lights[i + 2] = blue;
            lights[i + 3] = (depth & 0x0FF) * inv;
            lights[i + 4] = falloff;
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
            base.clear();
        }
    }
}