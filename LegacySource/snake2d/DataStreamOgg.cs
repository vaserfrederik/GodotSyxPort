using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenAL;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace snake2d
{
    public class DataStreamOgg : DATA_STREAM
    {
        private const int BUFFER_SIZE = 4096 * 4;

        private readonly byte[] vorbis;

        private readonly IntPtr handle;
        private readonly int channels;
        private readonly int sampleRate;
        private int format;

        private readonly int lengthSamples;
        private readonly float lengthSeconds;

        private readonly short[] pcm;

        private int samplesLeft;

        private readonly string file;

        public DataStreamOgg(string filePath)
        {
            this.file = filePath;
            try
            {
                vorbis = File.ReadAllBytes(filePath);
            }
            catch (IOException e)
            {
                throw Error(filePath, e.Message);
            }

            int error = 0;
            handle = STBVorbis.stb_vorbis_open_memory(vorbis, vorbis.Length, ref error, IntPtr.Zero);
            if (handle == IntPtr.Zero)
            {
                throw Error(filePath, "Error: " + error);
            }

            using (STBVorbisInfo info = new STBVorbisInfo())
            {
                GetInfo(handle, info);
                this.channels = info.Channels;
                this.sampleRate = info.SampleRate;
            }

            this.format = GetFormat(channels);

            this.lengthSamples = STBVorbis.stb_vorbis_stream_length_in_samples(handle) * channels;
            this.lengthSeconds = STBVorbis.stb_vorbis_stream_length_in_seconds(handle);

            this.pcm = new short[BUFFER_SIZE];

            samplesLeft = lengthSamples;
        }

        private Errors.DataError Error(string path, string error)
        {
            return new Errors.DataError("Could not process .ogg file. Make sure the audio file is truly encoded in ogg/vobis format" + Environment.NewLine + error, path);
        }

        private static void GetInfo(IntPtr decoder, STBVorbisInfo info)
        {
            STBVorbis.stb_vorbis_get_info(decoder, info);

            // Uncomment the following lines for debugging purposes
            // Printer.ln("stream length, samples: " + STBVorbis.stb_vorbis_stream_length_in_samples(decoder));
            // Printer.ln("stream length, seconds: " + STBVorbis.stb_vorbis_stream_length_in_seconds(decoder));
            //
            // Printer.ln();
            //
            // Printer.ln("channels = " + info.Channels);
            // Printer.ln("sampleRate = " + info.SampleRate);
            // Printer.ln("maxFrameSize = " + info.MaxFrameSize);
            // Printer.ln("setupMemoryRequired = " + info.SetupMemoryRequired);
            // Printer.ln("setupTempMemoryRequired() = " + info.SetupTempMemoryRequired);
            // Printer.ln("tempMemoryRequired = " + info.TempMemoryRequired);
        }

        private static int GetFormat(int channels)
        {
            switch (channels)
            {
                case 1:
                    return ALFormat.Mono16;
                case 2:
                    return ALFormat.Stereo16;
                default:
                    throw new NotSupportedException("Unsupported number of channels: " + channels);
            }
        }

        public override bool HasMoreBuffers()
        {
            return samplesLeft > 0;
        }

        public override void SetNext(int alBuff)
        {
            int samples = 0;

            while (samples < BUFFER_SIZE)
            {
                Buffer.BlockCopy(pcm, samples * sizeof(short), pcm, 0, BUFFER_SIZE * sizeof(short));
                int samplesPerChannel = STBVorbis.stb_vorbis_get_samples_short_interleaved(handle, channels, pcm);
                if (samplesPerChannel == 0)
                {
                    break;
                }

                samples += samplesPerChannel * channels;
            }

            if (samples == 0)
            {
                throw new Exception("getting nonexistant buffer " + file);
            }

            AL.BufferData(alBuff, format, pcm, samples * sizeof(short), sampleRate);
            samplesLeft -= samples;
        }

        public override double GetProgress()
        {
            return 1.0 - samplesLeft / (double)lengthSamples;
        }

        public override float GetLengthInSeconds()
        {
            return lengthSeconds;
        }

        public override void Rewind()
        {
            STBVorbis.stb_vorbis_seek_start(handle);
            samplesLeft = lengthSamples;
        }

        public override void Dispose()
        {
            STBVorbis.stb_vorbis_close(handle);
        }

        public void Skip(int direction)
        {
            Seek(Math.Min(Math.Max(0, STBVorbis.stb_vorbis_get_sample_offset(handle) + direction * sampleRate), lengthSamples));
        }

        public void SkipTo(float offset0to1)
        {
            Seek((int)Math.Round(lengthSamples * offset0to1));
        }

        private void Seek(int sample_number)
        {
            STBVorbis.stb_vorbis_seek(handle, sample_number);
            samplesLeft = lengthSamples - sample_number;
        }
    }
}