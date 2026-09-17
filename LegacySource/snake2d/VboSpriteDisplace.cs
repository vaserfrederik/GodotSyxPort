using System;
using OpenTK.Graphics.OpenGL;
using snake2d.util.color;

namespace snake2d
{
    class VboSpriteDisplace : VboAbsExt
    {
        private readonly Shader shader;

        static VboSpriteDisplace GetDeffered(SETTINGS sett)
        {
            Shader shader = new Shader(sett.GetNativeWidth(), sett.GetNativeHeight(), "Displace", "Displace", "Displace");
            shader.SetUniform1i("sampler1", 0);
            shader.SetUniform1i("sampler2", 1);
            return new VboSpriteDisplace(shader);
        }

        public VboSpriteDisplace(Shader shader) : base(
            PrimitiveType.Points,
            1 << 15,
            new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // position upper left		4
            new VboAttribute(2, VertexAttribPointerType.Short, false, 2), // position lower right	4
            new VboAttribute(2, VertexAttribPointerType.Float, false, 4), // texture coords1		4
            new VboAttribute(2, VertexAttribPointerType.Float, false, 4), // texture coords2		4
            new VboAttribute(2, VertexAttribPointerType.Short, 2), // texture coords width	4

            new VboAttribute(4, VertexAttribPointerType.UnsignedByte, true, 1), // color				4
            new VboAttribute(1, VertexAttribPointerType.Float, false, 4) // scale
        )
        {
            this.shader = shader;
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
            BindAndUpload();
            shader.Bind();
            int i = 0;
            vTo[current] = count;
            while (i <= current)
            {
                if (vFrom[i] != vTo[i])
                {
                    GlHelper.Stencil.SetEQUALKeepOnFail(i);
                    Flush(vFrom[i], vTo[i]);
                }
                i++;
            }
            Clear();
            GL.UseProgram(0);
        }

        void Render(float tx1, float ty1, float dx1, float dy1, int w, int h, double scale, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            if (count >= MAX_ELEMENTS)
            {
                return;
            }

            buffer.PutShort((short)x1).PutShort((short)y1);
            buffer.PutShort((short)x2).PutShort((short)y2);
            buffer.PutFloat(tx1).PutFloat(ty1);
            buffer.PutFloat(dx1).PutFloat(dy1);
            buffer.PutShort((short)w).PutShort((short)h);
            buffer.Put(color.Red()).Put(color.Green()).Put(color.Blue()).Put(opacity.Get());
            buffer.PutFloat((float)scale);

            count++;
        }

        public override void Dis()
        {
            shader.Dis();
            base.Dis();
        }
    }
}