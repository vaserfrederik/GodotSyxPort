using System;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using view.ui.wiki;

namespace util.info
{
    public class INFO
    {
        public readonly string name;
        public readonly string names;
        public readonly string desc;
        public readonly ACTION wiki;

        public INFO(Json json, ACTION wiki)
        {
            if (!json.Has("NAME"))
                json = json.Json("INFO");
            name = json.Text("NAME");
            if (json.Has("NAMES"))
                names = json.Text("NAMES");
            else
                names = name;
            desc = json.Text("DESC");
            if (wiki == null)
                wiki = WIKI.Add(json);
            this.wiki = wiki;
        }

        public INFO(Json json) : this(json, null) { }

        public INFO(string name, string desc) : this(name, name + "s", desc, null) { }

        public INFO(string name, string names, string desc, ACTION wiki)
        {
            this.name = name;
            this.names = names;
            this.desc = desc;
            this.wiki = wiki;
        }

        public void Hover(GUI_BOX box)
        {
            box.Title(name);
            box.Text(desc);
            box.NL();
        }
    }
}