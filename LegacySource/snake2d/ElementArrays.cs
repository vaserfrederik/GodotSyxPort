using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Snake2D
{
    abstract class ElementArrays
    {
        abstract public void Bind();
        abstract public void Dis();

        private static ElementArrays quad;
        private static ElementArrays point;

        static public void QuadBind()
        {
            if (quad != null)
            {
                quad.Bind();
                return;
            }

            int[] indices = new int[(1 << 16) * 6];
            int tmp = 0;
            for (int i = 0; i < indices.Length; i += 6)
            {
                indices[i] = tmp++;    //0
                indices[i + 1] = tmp++;  //1
                indices[i + 2] = tmp--;  //1
                indices[i + 3] = tmp++;  //2
                indices[i + 4] = tmp++;  //3
                indices[i + 5] = tmp++;  //4
            }

            GCHandle indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            IntPtr ptr = indicesHandle.AddrOfPinnedArrayElement(0);
            int vertexFixID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexFixID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), ptr, BufferUsageHint.StaticRead);
            indicesHandle.Free();

            quad = new ElementArrays()
            {
                Dis = () =>
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    GL.DeleteBuffer(vertexFixID);
                },
                Bind = () =>
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexFixID);
                }
            };
            quad.Bind();
            GLHelper.CheckErrors();
        }

        static public void PointBind()
        {
            if (point != null)
            {
                point.Bind();
                return;
            }

            int[] indices = new int[(1 << 16)];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            GCHandle indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            IntPtr ptr = indicesHandle.AddrOfPinnedArrayElement(0);
            int vertexFixID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexFixID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), ptr, BufferUsageHint.StaticRead);
            indicesHandle.Free();

            point = new ElementArrays()
            {
                Dis = () =>
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    GL.DeleteBuffer(vertexFixID);
                },
                Bind = () =>
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexFixID);
                }
            };
            point.Bind();
            GLHelper.CheckErrors();
        }

        static public void Dispose()
        {
            GLHelper.CheckErrors();
            if (quad != null)
            {
                quad.Dis();
                GLHelper.CheckErrors();
            }
            if (point != null)
            {
                point.Dis();
                GLHelper.CheckErrors();
            }
            quad = null;
            point = null;
        }
    }
}