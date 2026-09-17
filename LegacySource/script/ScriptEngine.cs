using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace script
{
    public class ScriptEngine : GameResource
    {
        private List<Script> loads = new List<Script>();
        private static LIST<ScriptLoad> all;

        public ScriptEngine(string[] scripts) : base("SCRIPTS", true)
        {
            LOG.ln("adding scripts " + scripts.Length);

            Dictionary<string, bool> map = new Dictionary<string, bool>();
            foreach (string s in scripts)
            {
                if (!map.ContainsKey(s))
                    map[s] = false;
            }
            LIST<ScriptLoad> loads = ScriptLoad.getAll();
            foreach (ScriptLoad l in loads)
            {
                if (map.ContainsKey(l.key) || l.script.forceInit())
                {
                    map[l.key] = true;
                    LOG.ln("adding script: " + l.file + " " + l.className + " " + l.script.forceInit());
                    Script sc = new Script(l);
                    this.loads.Add(sc);
                }
            }

            foreach (string s in scripts)
            {
                if (!map[s])
                {
                    GAME.Warn("Could not find script: " + s);
                }
            }

            init.initBeforeGameCreated();
        }

        public void init(GAME game)
        {
            foreach (Script s in loads)
            {
                try
                {
                    s.ins = s.load.script.createInstance();
                }
                catch (Exception e)
                {
                    error(s.load, e);
                }
            }
        }

        public string[] currentScripts()
        {
            string[] scripts = new string[loads.Count];
            int i = 0;
            foreach (Script l in loads)
                scripts[i++] = l.load.key;
            return scripts;
        }

        private void error(ScriptLoad l, Exception e)
        {
            StringWriter writer = new StringWriter();
            writer.Write("error in script " + l.className);
            writer.Write(Environment.NewLine);
            e.StackTrace.ToString();

            throw new Errors.DataError(writer.ToString(), l.file);
        }

        public static LIST<ScriptLoad> getAll()
        {
            if (all == null)
                all = ScriptLoad.getAll();

            return all;
        }

        public static LIST<ScriptLoad> getInJar(string jarFile)
        {
            List<ScriptLoad> res = new List<ScriptLoad>();
            foreach (ScriptLoad l in getAll())
            {
                if (l.file.Equals(jarFile))
                {
                    res.Add(l);
                }
            }
            return res;
        }

        protected override void save(FilePutter file)
        {
            file.mark(this);
            file.i(loads.Count);
            foreach (Script s in loads)
            {
                file.chars(s.load.key);
                int pos = file.getPosition();
                file.i(0);
                s.ins.save(file);
                int size = (file.getPosition() - pos) - 4;
                file.setAtPosition(pos, size);
            }
        }

        public override void load(FileGetter file) throws IOException
        {
            file.check(this);
            int am = file.i();

            Dictionary<string, Script> map = new Dictionary<string, Script>();
            foreach (Script s in loads)
                map[s.load.key] = s;

            while (am-- > 0)
            {
                string key = file.chars();
                int size = file.i();
                int position = file.getPosition();
                if (map.ContainsKey(key))
                {
                    Script l = map[key];
                    try
                    {
                        l.ins.load(file);
                    }
                    catch (Exception e)
                    {
                        error(l.load, e);
                    }
                    if (size != (file.getPosition() - position))
                    {
                        LOG.ln("Unable to load script. Was saved with + " + size + " bytes, but read "
                                + (file.getPosition() - position) + " " + l.ins.GetHashCode());
                        file.setPosition(position + size);
                        if (l.ins.handleBrokenSavedState())
                        {
                            LOG.ln("Script wants to carry on anyway, so be it.");
                        }
                        else
                            continue;
                    }

                }
                else
                {
                    LOG.ln("Script does not exist. Skipping. " + size + " " + key);
                    for (int i = 0; i < size; i++)
                        file.b();
                }

            }
        }

        protected override void update(double ds, Profiler prof)
        {
            prof.logStart(typeof(ScriptEngine));
            foreach (Script s in loads)
                try
                {
                    s.ins.update(ds);
                }
                catch (Exception e)
                {
                    error(s.load, e);
                }
            prof.logEnd(typeof(ScriptEngine));
        }

        private static class Script
        {
            private readonly ScriptLoad load;
            private SCRIPT_INSTANCE ins;

            public Script(ScriptLoad load)
            {
                this.load = load;
            }

        }

        public final SCRIPT init = new SCRIPT()
        {

            public override string name()
            {
                throw new RuntimeException();
            }

            public override bool isSelectable()
            {
                throw new RuntimeException();
            }

            public override void initBeforeGameCreated()
            {
                foreach (Script s in loads)
                    try
                    {
                        s.load.script.initBeforeGameCreated();
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

            public override string desc()
            {
                throw new RuntimeException();
            }

            public override SCRIPT_INSTANCE createInstance()
            {
                throw new RuntimeException();
            }

            public override void initBeforeGameInited()
            {
                foreach (Script s in loads)
                    try
                    {
                        s.load.script.initBeforeGameInited();
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

        };

        public final SCRIPT_INSTANCE callback = new SCRIPT_INSTANCE()
        {

            public override void update(double ds)
            {
                throw new RuntimeException();
            }

            public override void hoverTimer(double mouseTimer, GBox text)
            {
                foreach (Script s in loads)
                    try
                    {
                        s.ins.hoverTimer(mouseTimer, text);
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

            public override void render(Renderer r, float ds)
            {
                foreach (Script s in loads)
                    try
                    {
                        s.ins.render(r, ds);
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

            public override void mouseClick(MButt button)
            {
                foreach (Script s in loads)
                    try
                    {
                        s.ins.mouseClick(button);
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

            public override void hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                foreach (Script s in loads)
                    try
                    {
                        s.ins.hover(mCoo, mouseHasMoved);
                    }
                    catch (Exception e)
                    {
                        error(s.load, e);
                    }
            }

            public override void save(FilePutter file)
            {
                throw new RuntimeException();
            }

            public override void load(FileGetter file)
            {
                throw new RuntimeException();
            }
        };

    }

}