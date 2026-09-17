using System;
using System.IO;
using NAudio.Wave;
using OpenAL.Bindings;
using snake2d.util.file;

namespace snake2d
{
    class DataStream : DATA_STREAM
    {
        private const int targetedBufferSize = 4096 * 20;
        private readonly int bufferSize;
        private readonly int lastBufferSize;
        private readonly int nrOfBuffers;
        private int currentBuffer = 0;
        private readonly byte[] bufferBytes;
        private readonly byte[] lastBuffer;

        private readonly int totalSize;
        private readonly int alFormat;
        private readonly int samplerate;
        private readonly float length;
        private AudioFileReader stream;

        private readonly string path;

        DataStream(System.IO.Path path)
        {
            this.path = path.ToString();

            if (!(path.ToString().EndsWith(".wav") || path.ToString().EndsWith(".aiff")))
                throw new RuntimeException("only wav and aiff formats are supported");

            try
            {
                stream = new AudioFileReader(path.ToString());
            }
            catch (Exception e)
            {
                e.printStackTrace();
                throw new RuntimeException("Problem creating sound: " + path);
            }

            var format = stream.WaveFormat;

            length = (float)((stream.Length + 0.0) / format.SampleRate);
            totalSize = (int)(stream.Length * (format.BitsPerSample / 8));

            bufferSize = (targetedBufferSize / (format.BitsPerSample / 8)) * (format.BitsPerSample / 8);
            nrOfBuffers = totalSize / bufferSize + (totalSize % bufferSize == 0 ? 0 : 1);
            lastBufferSize = totalSize % bufferSize == 0 ? bufferSize : totalSize % bufferSize;

            samplerate = format.SampleRate;

            if (format.Channels == 1)
            {
                if (format.BitsPerSample == 8)
                {
                    alFormat = ALFormat.AL_FORMAT_MONO8;
                }
                else if (format.BitsPerSample == 16)
                {
                    alFormat = ALFormat.AL_FORMAT_MONO16;
                }
                else
                {
                    throw new RuntimeException("Illegal sample size");
                }
            }
            else if (format.Channels == 2)
            {
                if (format.BitsPerSample == 8)
                {
                    alFormat = ALFormat.AL_FORMAT_STEREO8;
                }
                else if (format.BitsPerSample == 16)
                {
                    alFormat = ALFormat.AL_FORMAT_STEREO16;
                }
                else
                {
                    throw new RuntimeException("Illegal sample size: " + format.BitsPerSample);
                }
            }
            else
            {
                throw new RuntimeException("Only mono or stereo is supported");
            }

            bufferBytes = Alloc.bb(bufferSize);
            lastBuffer = Alloc.bb(lastBufferSize);
        }

        public override bool hasMoreBuffers()
        {
            if (currentBuffer < nrOfBuffers)
                return true;
            return false;
        }

        public override void setNext(int alBuff)
        {
            byte[] buf;

            if (currentBuffer == nrOfBuffers - 1)
            {
                buf = lastBuffer;
            }
            else
            {
                buf = bufferBytes;
            }

            int read = 0, total = 0;
            try
            {
                while ((read = stream.Read(buf, total, buf.Length - total)) != -1
                    && total < buf.Length)
                {
                    total += read;
                }
            }
            catch (IOException ioe)
            {
                ioe.printStackTrace();
            }

            currentBuffer++;

            AL.alBufferData(alBuff, alFormat, convertAudioBytes(stream.WaveFormat, buf, stream.WaveFormat.BitsPerSample == 16), samplerate);
        }

        public override double getProgress()
        {
            if (currentBuffer > 0)
                return (double)((currentBuffer - 1) * bufferSize) / totalSize;
            return 0;
        }

        public override float getLengthInSeconds()
        {
            return length;
        }

        public override void dispose()
        {
            stream.Dispose();
        }

        /**
         * Convert the audio bytes into the stream
         * 
         * @param audio_bytes The audio bytes
         * @param two_bytes_data True if we using double byte data
         * @return The byte buffer of data
         */
        private ByteBuffer convertAudioBytes(WaveFormat format, byte[] audio_bytes, bool two_bytes_data)
        {
            ByteBuffer dest = ByteBuffer.AllocateDirect(audio_bytes.Length);
            dest.Order(ByteOrder.NativeOrder());
            ByteBuffer src = ByteBuffer.Wrap(audio_bytes);

            if (format.Encoding == WaveFormatEncoding.Pcm)
            {
                src.Order(format.BitsPerSample == 16 ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN);
            }

            if (two_bytes_data)
            {
                ShortBuffer dest_short = dest.AsShortBuffer();
                ShortBuffer src_short = src.AsShortBuffer();
                while (src_short.HasRemaining())
                    dest_short.Put(src_short.Get());
            }
            else
            {
                while (src.HasRemaining())
                {
                    byte b = src.Get();
                    if (format.Encoding == WaveFormatEncoding.PcmSigned)
                    {
                        b = (byte)(b + 127);
                    }
                    dest.Put(b);
                }
            }
            dest.Rewind();
            return dest;
        }

        public override void rewind()
        {
            stream.Dispose();
            try
            {
                stream = new AudioFileReader(path);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unable to rewind audioStream");
                e.printStackTrace();
                return;
            }
            currentBuffer = 0;
        }
    }
}