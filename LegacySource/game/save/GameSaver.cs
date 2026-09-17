using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game;
using game.save;
using init.paths;
using init.sprite;
using settlement.main;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;
using view.main;

namespace game.save
{
    public class GameSaver
    {
        private readonly KeyMap<Savable> smap = new KeyMap<Savable>();
        private readonly ArrayListGrower<Savable> before = new ArrayListGrower<Savable>();
        private readonly ArrayListGrower<Savable> all = new ArrayListGrower<Savable>();

        private readonly ArrayListGrower<Action<Path>> beforeSave = new ArrayListGrower<Action<Path>>();
        private readonly ArrayListGrower<Action<Path>> afterSave = new ArrayListGrower<Action<Path>>();
        private readonly ArrayListGrower<Action<Path>> beforeLoad = new ArrayListGrower<Action<Path>>();
        private readonly ArrayListGrower<Action<Path>> afterLoad = new ArrayListGrower<Action<Path>>();

        private readonly PROP prop = new PROP(this);
        private readonly AutoSaver auto = new AutoSaver(this);

        private double timeOfLastSave;
        private static readonly string ¤¤save = "Saving";
        private static readonly string ¤¤savingDisk = "Saving to disk, please wait.";

        static GameSaver()
        {
            D.ts(typeof(GameSaver));
        }

        public GameSaver(GAME game)
        {
            add(prop);
            timeOfLastSave = CORE.getUpdateInfo().getSecondsSinceFirstUpdate();
        }

        public Path save(string name)
        {
            return save(name, false);
        }

        public Path save(string name, bool minified)
        {
            return save(PATHS.local().save().get(), name, minified);
        }

        public Path save(Path path, string name, bool minified)
        {
            path = path.Join(name + PATHS.local().save().fileEnding());
            bool succ = false;
            SPRITES.loader().minify(minified, ¤¤save);
            SPRITES.loader().init();
            SPRITES.loader().print("Saving the world...");
            auto.reset();
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                FilePutter fp = new FilePutter(path, 120 * SETT.TAREA);
                save(fp);
                CORE.checkIn();
                Action a = () =>
                {
                    SPRITES.loader().print(¤¤savingDisk);
                };
                a();
                succ = fp.zip(a);
                timeOfLastSave = CORE.getUpdateInfo().getSecondsSinceFirstUpdate();
                GC.Collect();
                CORE.getInput().clearAllInput();
                SPRITES.loader().minify(false, ¤¤save);
                auto.reset();
                GC.Collect();
                return succ ? path : null;
            }
            catch (IOException e)
            {
                e.printStackTrace();
                SPRITES.loader().minify(false, ¤¤save);
                return null;
            }
        }

        private void save(FilePutter f)
        {
            foreach (Action<Path> a in beforeSave)
                a(f.path);

            GameSpec.save(f);
            save(f, before);
            save(f, all);

            foreach (Action<Path> a in afterSave)
                a(f.path);
        }

        private void save(FilePutter f, LIST<Savable> li)
        {
            f.i(li.size());
            foreach (Savable s in li)
            {
                string k = s.key;
                int pos = f.getPosition();
                f.i(0);
                s.save(f);
                int le = f.getPosition() - pos - 4;
                f.setAtPosition(pos, le);
            }
        }

        void load(FileGetter f) throws IOException
        {
            foreach (Action<Path> a in beforeLoad)
                a(f.path);

            load(f, before);
            load(f, all);

            foreach (Action<Path> a in afterLoad)
                a(f.path);
        }

        private void load(FileGetter f, LIST<Savable> li) throws IOException
        {
            KeyMap<Savable> map = new KeyMap<Savable>();
            foreach (Savable e in li)
            {
                map.put(e.key, e);
            }

            int am = f.i();
            for (int i = 0; i < am; i++)
            {
                string k = f.chars();
                int pos = f.getPosition() + f.i() + 4;
                Savable e = map.get(k);
                if (e != null)
                {
                    try
                    {
                        e.load(f);
                    }
                    catch (Exception ee)
                    {
                        ee.printStackTrace(Console.Out);
                        LOG.ln(k + " " + f.getPosition() + " " + pos + " " + e.GetType().Name);
                        f.setPosition(pos);
                        e.loadFail();
                    }

                    CORE.checkIn();
                    if (f.getPosition() != pos)
                    {
                        LOG.ln(k + " " + f.getPosition() + " " + pos + " " + e.GetType().Name);
                        f.setPosition(pos);
                        e.loadFail();
                    }
                }
                else
                {
                    LOG.ln("skipping " + k);
                    f.setPosition(pos);
                }
            }
        }

        public double getTimeSinceLastSave()
        {
            return CORE.getUpdateInfo().getSecondsSinceFirstUpdate() - timeOfLastSave;
        }

        public void quicksave()
        {
            saveNamed("QuickSave", 3, true);
        }

        public void saveNew()
        {
            saveNamed("A New Beginning", 3, false);
        }

        public bool saveNamed(string sname, int max, bool mini)
        {
            if (!VIEW.canSave())
                return false;
            int am = 0;
            foreach (string s in PATHS.local().save().getFiles())
            {
                if (SaveFile.name(s).Equals(sname))
                {
                    am++;
                }
            }
            while (am >= max)
            {
                string least = null;
                foreach (string s in PATHS.local().save().getFiles())
                {
                    if (SaveFile.name(s).Equals(sname))
                    {
                        if (least == null || SaveFile.time(s) < SaveFile.time(least))
                        {
                            least = s;
                        }
                    }
                }
                am--;
                PATHS.local().save().delete(least);
            }
            return save(SaveFile.stamp(sname), mini) != null;
        }

        public void autoSave(double ds)
        {
            auto.autosave(ds);
        }

        public void addSpecialSaver(Savable s)
        {
            before.add(s);
            smap.put(s.key, s);
        }

        public void add(Savable s)
        {
            all.add(s);
            smap.put(s.key, s);
        }

        public void onBeforeSave(Action<Path> a)
        {
            beforeSave.add(a);
        }

        public void onAfterSave(Action<Path> a)
        {
            afterSave.add(a);
        }

        public void onBeforeLoad(Action<Path> a)
        {
            beforeLoad.add(a);
        }

        public void onAfterLoad(Action<Path> a)
        {
            afterLoad.add(a);
        }
    }
}