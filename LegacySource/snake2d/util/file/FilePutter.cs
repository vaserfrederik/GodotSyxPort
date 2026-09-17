using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Snake2D.Util.File
{
    public sealed class FilePutter
    {
        private readonly Stream outStream;
        private readonly BinaryWriter binaryWriter;
        private readonly ByteBuffer buffer;
        public readonly Path path;

        public FilePutter(Path path, int size)
        {
            buffer = new ByteBuffer(size);
            this.path = path;
            outStream = new ByteBufferBackedStream(buffer);
            binaryWriter = new BinaryWriter(outStream);
            binaryWriter.Flush();
        }

        public void Save()
        {
            try
            {
                binaryWriter.Flush();
                File f = new File(path.ToString());
                f.CreateNewFile();
                using (FileStream outfile = new FileStream(f.FullName, FileMode.Create))
                {
                    buffer.Flip();
                    outfile.Write(buffer.Array, buffer.Position, buffer.Remaining);
                    outfile.Flush();
                }
            }
            catch (IOException e1)
            {
                throw new Exception(e1);
            }
        }

        private Exception ex;
        private bool working = false;

        public bool Zip(ACTION checkin)
        {
            Thread current = Thread.CurrentThread;

            ex = null;
            working = true;

            Thread t = new Thread(() =>
            {
                try
                {
                    binaryWriter.Flush();
                    File f = new File(path.ToString());
                    f.CreateNewFile();
                    f.SetWritable(true);

                    using (FileStream outStream = new FileStream(f.FullName, FileMode.Create))
                    using (DeflateStream defl = new DeflateStream(outStream, CompressionLevel.Optimal))
                    {
                        buffer.Flip();
                        byte[] sizeBytes = BitConverter.GetBytes(buffer.Limit);
                        defl.Write(sizeBytes, 0, sizeBytes.Length);
                        defl.Flush();

                        defl.Write(buffer.Array, buffer.Position, buffer.Limit);
                        defl.Flush();
                    }

                    working = false;
                }
                catch (IOException e1)
                {
                    ex = e1;
                    working = false;
                    current.Interrupt();
                    e1.PrintStackTrace();
                    throw new Exception(e1);
                }
            });
            t.Name = "zipper";
            t.Start();

            long m = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long check = 0;

            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m < 10000 && working)
            {
                Thread.Yield();
                if (check != (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m) / 1000)
                {
                    check = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m) / 100;
                    checkin.exe();
                }
            }

            if (working)
            {
                Console.Error.WriteLine("saver thread took too long to save...");
            }

            if (ex != null)
            {
                ex.PrintStackTrace();
                throw new Exception(ex);
            }

            return true;
        }

        public FilePutter Bool(bool boolValue)
        {
            B((byte)(boolValue ? 1 : 0));
            return this;
        }

        public void Mark(string s)
        {
            I(s.GetHashCode());
        }

        public void Mark(Type c)
        {
            Mark(c.Name);
        }

        public void Mark(object o)
        {
            Mark(o.GetType().Name);
        }

        public void Object(object o)
        {
            try
            {
                int pos = GetPosition();
                I(0);
                binaryWriter.Write(o);
                binaryWriter.Flush();
                int npos = GetPosition();
                int l = GetPosition() - pos;
                buffer.Position = pos;
                I(l);
                buffer.Position = npos;
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(o);
                Console.Error.WriteLine(o.GetType());
                Console.Error.WriteLine(o.GetType().Name);
                throw new Exception(o + " " + e);
            }
        }

        public FilePutter I(int i)
        {
            WriteInt(i);
            return this;
        }

        public FilePutter B(byte b)
        {
            buffer.Put(b);
            return this;
        }

        public FilePutter L(long l)
        {
            buffer.PutLong(l);
            return this;
        }

        public FilePutter Ls(long[] ls)
        {
            foreach (long l in ls)
                buffer.PutLong(l);
            return this;
        }

        public void LsE(long[] tiles)
        {
            I(tiles.Length);
            Ls(tiles);
        }

        public FilePutter Ls(long[][] ls)
        {
            foreach (long[] l in ls)
                Ls(l);
            return this;
        }

        public void WriteArray(short[][] tiles)
        {
            foreach (short[] sa in tiles)
            {
                Ss(sa);
            }
        }

        public FilePutter S(short s)
        {
            buffer.PutShort(s);
            return this;
        }

        public void Ss(short[] tiles)
        {
            foreach (short s in tiles)
                buffer.PutShort(s);
        }

        public void SsE(short[] tiles)
        {
            I(tiles.Length);
            Ss(tiles);
        }

        public void Is(int[] tiles)
        {
            foreach (int i in tiles)
                buffer.PutInt(i);
        }

        public void IsE(int[] tiles)
        {
            I(tiles.Length);
            Is(tiles);
        }

        public void Ds(double[] tiles)
        {
            foreach (double i in tiles)
                buffer.PutDouble(i);
        }

        public void DsE(double[] tiles)
        {
            I(tiles.Length);
            Ds(tiles);
        }

        public void Save(SAVABLE ss)
        {
            int pos = GetPosition();
            I(0);
            ss.Save(this);
            int le = GetPosition() - pos - 4;
            SetAtPosition(pos, le);
        }

        public void Fs(float[] data)
        {
            foreach (float i in data)
                buffer.PutFloat(i);
        }

        public void FsE(float[] data)
        {
            I(data.Length);
            Fs(data);
        }

        public void F(float f)
        {
            buffer.PutFloat(f);
        }

        public void Is(int[][] tiles)
        {
            foreach (int[] sa in tiles)
            {
                Is(sa);
            }
        }

        public void IsE(int[][] tiles)
        {
            I(tiles.Length);
            foreach (int[] i in tiles)
                IsE(i);
        }

        public void Bs(byte[][] bytes)
        {
            foreach (byte[] sa in bytes)
            {
                Bs(sa);
            }
        }

        public void Bs(byte[] sa)
        {
            buffer.Put(sa);
        }

        public void BsE(byte[] sa)
        {
            I(sa.Length);
            buffer.Put(sa);
        }

        public void WriteInt(int i)
        {
            buffer.PutInt(i);
        }

        public FilePutter D(double d)
        {
            buffer.PutDouble(d);
            return this;
        }

        public int WrittenInts()
        {
            return buffer.Position / 4;
        }

        public int GetPosition()
        {
            return buffer.Position;
        }

        public void SetAtPosition(int pos, int value)
        {
            int p = buffer.Position;
            buffer.Position = pos;
            I(value);
            buffer.Position = p;
        }

        public void Chars(CharSequence c)
        {
            I(0);
            I(c.Length);
            for (int i = 0; i < c.Length; i++)
            {
                buffer.PutShort((short)c.CharAt(i));
            }
        }

        public void Charss(CharSequence[] cc)
        {
            I(0);
            I(cc.Length);
            for (int i = 0; i < cc.Length; i++)
            {
                Chars(cc[i]);
            }
        }

        private class ByteBufferBackedStream : Stream
        {
            private ByteBuffer buf;

            public ByteBufferBackedStream(ByteBuffer buf)
            {
                this.buf = buf;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.buf.Put(buffer, offset, count);
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotImplementedException();
            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Flush()
            {
                // No-op
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
        }
    }
}