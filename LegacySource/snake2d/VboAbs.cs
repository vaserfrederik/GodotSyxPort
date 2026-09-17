using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

abstract class VboAbs
{
    private readonly int vertexArrayID;
    private readonly int attributeElementID;

    protected readonly ByteBuffer buffer;

    protected readonly int MAX_ELEMENTS;
    protected readonly int ELEMENT_SIZE;
    private readonly int BUFFER_SIZE;

    private readonly int NR_OF_ATTRIBUTES;

    private readonly int type;
    private readonly int indexMul;

    VboAbs(int type, int maxElements, params VboAttribute[] attributes)
    {
        GlHelper.CheckErrors();
        MAX_ELEMENTS = maxElements;
        NR_OF_ATTRIBUTES = attributes.Length;

        this.type = type;
        int vertecies = 0;
        if (type == GL_TRIANGLES)
        {
            indexMul = 6;
            vertecies = 4;
        }
        else if (type == GL_POINTS)
        {
            indexMul = 1;
            vertecies = 1;
        }
        else
        {
            throw new RuntimeException("unsupported type");
        }

        int byteStride = 0;
        foreach (VboAttribute v in attributes)
        {
            byteStride += v.sizeInBytes;
        }

        if (byteStride % 4 != 0)
            throw new RuntimeException(byteStride + " Needs padding with " + (4 - (byteStride % 4)));

        ELEMENT_SIZE = byteStride * vertecies;
        BUFFER_SIZE = ELEMENT_SIZE * MAX_ELEMENTS;
        buffer = ByteBuffer.AllocateDirect(BUFFER_SIZE);

        vertexArrayID = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayID);

        attributeElementID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, attributeElementID);

        int index = 0;
        int pointerOffset = 0;
        foreach (VboAttribute v in attributes)
        {
            if (v.isInt)
                GL.VertexAttribIPointer(index, v.amount, v.glType, byteStride, pointerOffset);
            else
                GL.VertexAttribPointer(index, v.amount, v.glType, v.normalized, byteStride, pointerOffset);
            index++;
            pointerOffset += v.sizeInBytes;
        }

        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(buffer.Capacity), buffer, BufferUsageHint.StreamDraw);
        if (type == GL_TRIANGLES)
        {
            ElementArrays.QuadBind();
        }
        else if (type == GL_POINTS)
        {
            ElementArrays.PointBind();
        }

        GL.BindVertexArray(0);

        GlHelper.CheckErrors();
    }

    protected void Flush(int from, int to)
    {
        if (from == to)
            return;
        GL.DrawElements(type, (to - from) * indexMul, DrawElementsType.UnsignedInt, from * 4 * indexMul);
    }

    public void Clear()
    {
        buffer.Clear();
    }

    public void Dis()
    {
        GlHelper.CheckErrors();
        // Disable the VBO index from the VAO attributes list
        GL.BindVertexArray(vertexArrayID);

        GL.BindBuffer(BufferTarget.ArrayBuffer, attributeElementID);

        for (int i = 0; i < NR_OF_ATTRIBUTES; i++)
        {
            GL.DisableVertexAttribArray(i);
        }

        // Dispose the buffer object
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffers(attributeElementID);

        // Dispose the vertex array
        GL.BindVertexArray(0);
        GL.DeleteVertexArrays(vertexArrayID);

        GlHelper.CheckErrors();

        buffer.Dispose();
    }

    protected void BindAndUpload()
    {
        if (buffer.Position == 0)
            return;

        buffer.Flip();

        Bind();

        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(buffer.Capacity), buffer);
    }

    protected void Upload()
    {
        buffer.Flip();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(buffer.Capacity), buffer);
    }

    protected void Bind()
    {
        GL.BindVertexArray(vertexArrayID);

        GL.BindBuffer(BufferTarget.ArrayBuffer, attributeElementID);

        for (int i = 0; i < NR_OF_ATTRIBUTES; i++)
        {
            GL.EnableVertexAttribArray(i);
        }
    }

    public class VboAttribute
    {
        private readonly bool isInt;
        private readonly int amount;
        private readonly int glType;
        private readonly int sizeInBytes;
        private readonly bool normalized;

        public VboAttribute(int amount, int glType, bool normalized, int sizeInBytes)
        {
            isInt = false;
            this.amount = amount;
            this.glType = glType;
            this.sizeInBytes = sizeInBytes * amount;
            this.normalized = normalized;
        }

        public VboAttribute(int amount, int glType, int sizeInBytes)
        {
            isInt = true;
            this.amount = amount;
            this.glType = glType;
            this.sizeInBytes = sizeInBytes * amount;
            this.normalized = false;
        }
    }
}