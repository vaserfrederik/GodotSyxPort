using System;

namespace Settlement.Misc.Util
{
    public interface FSERVICE : FINDABLE
    {
        void Consume();

        void StartUsing();

        bool HasQueue();
    }
}