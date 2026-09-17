using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace snake2d
{
    public static class IOUtil
    {
        private IOUtil()
        {
        }

        private static byte[] ResizeBuffer(byte[] buffer, int newCapacity)
        {
            byte[] newBuffer = new byte[newCapacity];
            Array.Copy(buffer, newBuffer, buffer.Length);
            return newBuffer;
        }

        /// <summary>
        /// Reads the specified resource and returns the raw data as a byte array.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <param name="bufferSize">The initial buffer size.</param>
        /// <returns>The resource data.</returns>
        /// <exception cref="IOException">If an IO error occurs.</exception>
        public static byte[] IoResourceToByteBuffer(string resource, int bufferSize)
        {
            byte[] buffer;

            if (File.Exists(resource))
            {
                buffer = new byte[File.ReadAllBytes(resource).Length + 1];
                File.ReadAllBytes(resource).CopyTo(buffer, 0);
            }
            else
            {
                using (FileStream fs = new FileStream(resource, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[bufferSize];

                    while (true)
                    {
                        int bytesRead = fs.Read(buffer, buffer.Length - bufferSize, bufferSize);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        if (buffer.Length - bufferSize == 0)
                        {
                            buffer = ResizeBuffer(buffer, buffer.Length * 2);
                        }
                    }
                }
            }

            return buffer;
        }
    }
}