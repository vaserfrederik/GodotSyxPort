using System;

namespace snake2d.util.misc
{
    public interface ERROR_HANDLER
    {
        void Handle(string output, string dump);
        void Handle(Exception e, string dump);
        void Handle(DataError e, string dump);
        void Handle(GameError e, string dump);
    }
}