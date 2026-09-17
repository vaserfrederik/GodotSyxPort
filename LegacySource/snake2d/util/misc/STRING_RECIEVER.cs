using System;

namespace Snake2D.Util.Misc
{
    public interface IStringReceiver
    {
        void AcceptString(ICharSequence str);
    }

    public interface ICharSequence
    {
        char this[int index] { get; }
        int Length { get; }
    }
}