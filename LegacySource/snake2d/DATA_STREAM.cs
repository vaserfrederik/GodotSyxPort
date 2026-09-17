using System;
using System.IO;

namespace snake2d
{
    interface DATA_STREAM
    {
        static DATA_STREAM GetStream(string path)
        {
            if (path.EndsWith(".wav") || path.EndsWith(".aiff"))
            {
                return new DataStream(path);
            }
            else if (path.EndsWith(".ogg"))
            {
                return new DataStreamOgg(path);
            }
            else
            {
                throw new Exception("only .wav, .aiff and .ogg formats are supported for streaming audio");
            }
        }

        bool HasMoreBuffers();

        void SetNext(int alBuffHandle);

        double GetProgress();

        float GetLengthInSeconds();

        void Dispose();

        void Rewind();
    }
}