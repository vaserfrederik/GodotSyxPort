using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace snake2d
{
    final class Shader
    {
        protected int programID;
        private int vertexShaderID;
        private int fragmentShaderID;
        private int geometryShaderID = -1;

        public Shader(double width, double height, string vertex, string geometry, string fragment)
        {
            programID = GL.CreateProgram();
            vertex = GetFile(vertex, "v");
            vertex = vertex.Replace("SCREEN_X", "" + (2f / width));
            vertex = vertex.Replace("SCREEN_Y", "" + (-2f / height));

            vertexShaderID = AttachShader(vertex, programID, ShaderType.VertexShader);

            fragment = GetFile(fragment, "f");

            fragmentShaderID = AttachShader(fragment, programID, ShaderType.FragmentShader);

            if (geometry != null)
            {
                geometry = GetFile(geometry, "g");
                geometryShaderID = AttachShader(geometry, programID, ShaderType.GeometryShader);
            }

            Link();
            Bind();
            Printer.ln(" " + this.GetType() + ": " + programID + ", ");
        }

        public static string GetFile(string name, string append)
        {
            name += "_" + append + ".txt";
            try
            {
                using (Stream stream = Shader.GetType().Assembly.GetManifestResourceStream(name))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                throw new Exception(name);
            }
        }

        private static int AttachShader(string source, int programID, ShaderType type)
        {
            int id = GL.CreateShader(type);
            if (id == 0)
            {
                throw new Exception("shader didn't compile");
            }
            GL.ShaderSource(id, source);
            GL.CompileShader(id);

            string infoLog = GL.GetShaderInfoLog(id);
            if (GL.GetShader(id, ShaderParameter.CompileStatus) == 0)
                throw new Exception("Error creating shader\n" + infoLog);

            GL.AttachShader(programID, id);
            return id;
        }

        private void Link()
        {
            GL.LinkProgram(programID);

            string infoLog = GL.GetProgramInfoLog(programID);
            if (GL.GetProgram(programID, GetProgramParameterName.LinkStatus) == 0)
                throw new Exception("Unable to link shader program:\n" + infoLog);
        }

        public void Bind()
        {
            GL.UseProgram(programID);
        }

        protected void BindAttribute(int position, string name)
        {
            GL.BindAttribLocation(programID, position, name);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void Dis()
        {
            Unbind();

            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            if (geometryShaderID != -1)
            {
                GL.DetachShader(programID, geometryShaderID);
            }

            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(fragmentShaderID);
            if (geometryShaderID != -1)
            {
                GL.DeleteShader(geometryShaderID);
            }

            GL.DeleteProgram(programID);
        }

        public int GetID()
        {
            return programID;
        }

        public int GetUniformLocation(string name)
        {
            int id = GL.GetUniformLocation(programID, name);
            if (id == -1 || GL.GetProgram(programID, GetProgramParameterName.ActiveUniforms) == 0)
                throw new Exception("not able to find shader uniform: " + name);
            return id;
        }

        public string GetScreenVec(float width, float height)
        {
            float w = 2f / width;
            float h = -2f / height;
            return "const vec2 screen = vec2(" + w + "," + h + ");" + "\n";
        }

        protected void SetUniform2f(int loc, float a, float b)
        {
            GL.Uniform2(loc, a, b);
        }

        protected void SetUniform(int loc, float a)
        {
            GL.Uniform1(loc, a);
        }

        protected void SetUniform(int loc, float a, float b, float c)
        {
            GL.Uniform3(loc, a, b, c);
        }

        public void SetUniform1i(string name, int a)
        {
            GL.Uniform1(GetUniformLocation(name), a);
        }
    }
}