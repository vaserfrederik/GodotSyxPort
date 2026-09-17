using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using NAudio.Wave;
using OpenAL;

namespace Snake2D
{
    class Data
    {
        readonly byte[] data;
        readonly int sizeInBytes;
        readonly int alFormat;
        readonly int channels;
        readonly int samplerate;
        readonly float length;

        public Data(string path)
        {
            if (!path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                throw new Errors.DataError("Only WAV and AIFF formats are supported", path);

            using (var audioFile = new AudioFileReader(path))
            {
                if (audioFile.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                    throw new Errors.DataError("Only PCM audio is supported", path);

                length = (float)(audioFile.TotalTime.TotalSeconds);
                if (length > 10)
                    throw new Errors.DataError("Sound effects more than 10s should not be used as sound effects!", path);

                channels = audioFile.WaveFormat.Channels;
                samplerate = audioFile.WaveFormat.SampleRate;

                if (channels == 1)
                {
                    if (audioFile.WaveFormat.BitsPerSample == 8)
                        alFormat = ALFormat.Mono8;
                    else if (audioFile.WaveFormat.BitsPerSample == 16)
                        alFormat = ALFormat.Mono16;
                    else
                        throw new Errors.DataError("Illegal sample size", path);
                }
                else if (channels == 2)
                {
                    throw new Errors.DataError("Stereo sounds can't be sound effects", path);
                }
                else
                {
                    throw new Errors.DataError("Only mono or stereo is supported", path);
                }

                data = new byte[audioFile.Length];
                audioFile.Read(data, 0, data.Length);
            }

            sizeInBytes = data.Length;
        }

        public void Dispose()
        {
            Array.Clear(data, 0, data.Length);
        }

        private static byte[] ConvertAudioBytes(WaveFormat format, byte[] audioBytes, bool twoBytesData)
        {
            byte[] dest = new byte[audioBytes.Length];
            Buffer.BlockCopy(audioBytes, 0, dest, 0, audioBytes.Length);

            if (twoBytesData)
            {
                if (format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < dest.Length; i += 2)
                    {
                        byte b1 = dest[i];
                        byte b2 = dest[i + 1];
                        if (BitConverter.IsLittleEndian)
                        {
                            dest[i] = b2;
                            dest[i + 1] = b1;
                        }
                    }
                }
            }
            else
            {
                if (format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < dest.Length; i++)
                    {
                        byte b = dest[i];
                        dest[i] = (byte)(b + 127);
                    }
                }
            }

            return dest;
        }
    }
}