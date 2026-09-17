using System;
using System.Collections.Generic;
using snake2d.util.file;

namespace game.events.killer
{
    class KillerType
    {
        public readonly string name;
        public readonly string method;
        public readonly string[] messages;

        public KillerType(Json json)
        {
            name = json.text("NAME");
            method = json.text("METHOD");
            messages = json.texts("MESSAGES", 2, 100);
        }
    }
}