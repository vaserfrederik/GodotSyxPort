using System;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;

namespace snake2d.util.file
{
    public sealed class FileGetter
    {
        public readonly string path;
        private readonly ObjectInputStream object;

        private readonly ByteBuffer buffer;

        public FileGetter(string path) : this(path, false)
        {
        }

        public FileGetter(string path, bool zipped) : this(path, zipped, null)
        {
        }

        public FileGetter(string path, bool zipped, ByteBuffer buffer)
        {
            this.path = path;
            FileInfo f = new FileInfo(path);
            if (!f.Exists || f.Length == 0)
            {
                throw new IOException(
                    $"file is corrupt. Try replacing it from an uncorrupted game folder, or delete it and see what happens {f.Exists} {f.Length} {path}");
            }

            if (f.Length > int.MaxValue)
                throw new RuntimeException("file too large to read");

            this.buffer = buffer ?? ByteBuffer.Allocate((int)f.Length);

            try
            {
                using (FileStream inStream = new FileStream(f.FullName, FileMode.Open, FileAccess.Read))
                using (FileChannel channel = inStream.Channel)
                {
                    channel.Read(this.buffer);
                    channel.Close();
                }
                this.buffer.Flip();
                using (var inn = new ByteBufferBackedStream(this.buffer))
                {
                    object = new ObjectInputStream(inn);
                }
            }
            catch (Exception e)
            {
                e.printStackTrace();
                throw new IOException(e);
            }
        }

        public void Close()
        {
            try
            {
                object.Close();
            }
            catch (IOException e)
            {
                throw new RuntimeException(e);
            }
        }

        public object Object(bool acceptNull) throws IOException
        {
            int pp = GetPosition() + I();
            try
            {
                return object.ReadObject();
            }
            catch (Exception e)
            {
                if (acceptNull)
                {
                    SetPosition(pp);
                    return null;
                }
                throw new IOException("A code artefact is missing in the current configuration of the game. The usual suspect is a mod version change. Contact the modder in question.", e);
            }
        }

        public object Object()
        {
            return Object(false);
        }

        public void Check(string s) throws IOException
        {
            int i = I();
            int h = s.GetHashCode();
            if (i != h)
            {
                throw new IOException($"corrupt data, expecting : {s} ({h}, {i}), {path}");
            }
        }

        public void Check(object o) throws IOException
        {
            Check(o.GetType().Name);
        }

        public void ReadArray(short[][] shorts) throws IOException
        {
            foreach (short[] s in shorts)
                SS(s);
        }

        public void SS(short[] shorts) throws IOException
        {
            for (int i = 0; i < shorts.Length; i++)
                shorts[i] = S();
        }

        public void SSE(short[] data) throws IOException
        {
            int l = I();
            CheckLength(l, data.Length);
            if (l != data.Length)
            {
                short[] b = new short[l];
                SS(b);
                for (int i = 0; i < l && i < data.Length; i++)
                    data[i] = b[i];
            }
            else
                SS(data);
        }

        public void IS(int[] data) throws IOException
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = I();
        }

        public bool ISE(int[] data) throws IOException
        {
            int l = I();
            if (l != data.Length)
            {
                int[] b = Alloc.II(l);
                IS(b);
                for (int i = 0; i < l && i < data.Length; i++)
                    data[i] = b[i];
                return false;
            }
            IS(data);
            return true;
        }

        public void DS(double[] data) throws IOException
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = D();
        }

        public void DSE(double[] data) throws IOException
        {
            int l = I();
            if (l != data.Length)
            {
                double[] b = new double[l];
                DS(b);
                for (int i = 0; i < l && i < data.Length; i++)
                    data[i] = b[i];
            }
            else
                DS(data);
        }

        public void FS(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = F();
        }

        public void FSE(float[] data) throws IOException
        {
            int l = I();
            if (l != data.Length)
            {
                float[] b = new float[l];
                FS(b);
                for (int i = 0; i < l && i < data.Length; i++)
                    data[i] = b[i];
            }
            else
                FS(data);
        }

        public float F()
        {
            return buffer.GetFloat();
        }

        public void IS(int[][] data) throws IOException
        {
            foreach (int[] i in data)
                IS(i);
        }

        public void ISE(int[][] data) throws IOException
        {
            int l = I();
            CheckLength(l, data.Length);
            for (int i = 0; i < l; i++)
            {
                if (i < data.Length)
                {
                    ISE(data[i]);
                }
                else
                {
                    ISE(Alloc.II(0));
                }
            }
        }

        public void ReadArray(int[][] shorts) throws IOException
        {
            foreach (int[] s in shorts)
                IS(s);
        }

        public void BS(byte[][] bytes) throws IOException
        {
            foreach (byte[] s in bytes)
                BS(s);
        }

        public void BS(byte[] bytes) throws IOException
        {
            try
            {
                buffer.Get(bytes);
            }
            catch (BufferUnderflowException e)
            {
                throw new RuntimeException(e);
            }
        }

        public byte B() throws IOException
        {
            if (buffer.Remaining < 1)
                throw new IOException();
            return buffer.Get();
        }

        public void BSE(byte[] bytes) throws IOException
        {
            int l = I();
            CheckLength(l, bytes.Length);
            if (l != bytes.Length)
            {
                byte[] b = Alloc.BB(l);
                BS(b);
                for (int i = 0; i < l && i < bytes.Length; i++)
                    bytes[i] = b[i];
            }
            else
                BS(bytes);
        }

        public int LastInt() throws IOException
        {
            if (buffer.Remaining <= 4)
                throw new IOException();
            int res = buffer.GetInt(buffer.Position - 4);
            return res;
        }

        public bool Test(int i) throws IOException
        {
            int q = I();
            if (q == i)
                return true;
            buffer.Position -= 4;
            return false;
        }

        public short S() throws IOException
        {
            if (buffer.Remaining < 2)
                throw new IOException();
            return buffer.GetShort();
        }

        public bool Bool() throws IOException
        {
            return B() == 1;
        }

        public long L()
        {
            return buffer.GetLong();
        }

        public void LS(long[] ls)
        {
            for (int i = 0; i < ls.Length; i++)
                ls[i] = buffer.GetLong();
        }

        public bool LSE(long[] ls) throws IOException
        {
            int l = I();
            CheckLength(l, ls.Length);
            if (l != ls.Length)
            {
                LS(new long[l]);
                Array.Fill(ls, 0L);
                return true;
            }
            else
            {
                LS(ls);
                return false;
            }
        }

        private void CheckLength(int l, int old) throws IOException
        {
            if (l == 0)
                return;
            if (l < 0)
                throw new IOException();
            if (l != old)
            {
                if (l / old > 10 || old / l > 10)
                    throw new IOException();
            }
        }

        public void LS(long[][] ls)
        {
            for (int i = 0; i < ls.Length; i++)
                LS(ls[i]);
        }

        public void Load(SAVABLE saver) throws IOException
        {
            int le = I();
            if (saver == null)
            {
                SetPosition(GetPosition() + le);
            }
            else
                saver.Load(this);
        }

        public int GetPosition()
        {
            return buffer.Position;
        }

        public void SetPosition(int pos)
        {
            buffer.Position = pos;
        }

        private class ByteBufferBackedStream : Stream
        {
            private ByteBuffer buf;

            public ByteBufferBackedStream(ByteBuffer buf)
            {
                this.buf = buf;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotImplementedException();
            public override long Position
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                count = Math.Min(count, buf.Remaining);
                buf.Get(buffer, offset, count);
                return count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}