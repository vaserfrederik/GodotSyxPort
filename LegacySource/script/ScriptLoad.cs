using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Zip;
using init.paths;
using snake2d;
using snake2d.util.sets;

public static class ScriptLoad
{
    private static KeyMap<LinkedList<ScriptLoad>> cache = new KeyMap<LinkedList<ScriptLoad>>();

    public readonly SCRIPT script;
    public readonly string className;
    public readonly string file;
    public readonly string key;

    private ScriptLoad(SCRIPT script, string cn, string file)
    {
        this.script = script;
        this.className = cn;
        this.file = file;
        key = file + "->" + cn;
    }

    public static LIST<ScriptLoad> Get(string script)
    {
        return new Init().CompileScripts(script);
    }

    public static LIST<ScriptLoad> GetAll()
    {
        return new ArrayList<ScriptLoad>(new Init().CompileScripts(PATHS.SCRIPT().jar.GetFiles()));
    }

    private class Init
    {
        private readonly PATH pathRoot = PATHS.SCRIPT().jar;
        private readonly LinkedList<ScriptLoad> all = new LinkedList<ScriptLoad>();
        private readonly LinkedList<Uri> urlList = new LinkedList<Uri>();
        private readonly KeyMap<string> classToJar = new KeyMap<string>();

        public Init()
        {
            ForceInit(typeof(SCRIPT));
        }

        private LIST<ScriptLoad> CompileScripts(params string[] files)
        {
            files = GrabCachedScripts(files);

            if (files.Length == 0)
                return all;

            foreach (string file in files)
            {
                JarInputStream jarFile = CopyAndMakeJarUrl(file);
                if (jarFile == null)
                    continue;

                try
                {
                    JarEntry je = jarFile.GetNextJarEntry();

                    while (je != null)
                    {
                        JarEntry jarEntry = je;
                        je = jarFile.GetNextJarEntry();
                        if (jarEntry.Name.Contains("META-INF"))
                            continue;
                        ProcessJarEntry(jarEntry, file);
                    }

                    jarFile.Close();

                }
                catch (IOException e)
                {
                    LOG.Err("script: " + pathRoot.Get() + Path.DirectorySeparatorChar + file + " unable to cache!");
                    e.printStackTrace();
                }
            }

            LoadScripts();

            cache.Clear();

            foreach (ScriptLoad l in all)
            {
                if (!cache.ContainsKey(l.file))
                    cache.Put(l.file, new LinkedList<ScriptLoad>());
                cache.Get(l.file).Add(l);
            }

            return all;
        }

        private string[] GrabCachedScripts(params string[] files)
        {
            int uninited = 0;

            foreach (string f in files)
            {
                if (cache.ContainsKey(f))
                {
                    all.Add(cache.Get(f));
                }
                else
                    uninited++;
            }

            string[] nFiles = new string[uninited];
            uninited = 0;
            foreach (string f in files)
            {
                if (!cache.ContainsKey(f))
                {
                    nFiles[uninited++] = f;
                }
            }

            return nFiles;
        }

        private void LoadScripts()
        {
            LOG.Ln("SCRIPTS");

            AssemblyLoader loader = new AssemblyLoader();

            foreach (string className in classToJar.Keys())
            {
                Assembly assembly = loader.LoadFromUri(new Uri("file:///" + classToJar.Get(className)));
                Type s = assembly.GetType(className);

                if (typeof(SCRIPT).IsAssignableFrom(s) && !s.IsAbstract)
                {
                    try
                    {
                        SCRIPT sc = (SCRIPT)Activator.CreateInstance(s);
                        all.Add(new ScriptLoad(sc, className, classToJar.Get(className)));
                        LOG.Ln(" -script available: : " + sc.name());
                    }
                    catch (TargetInvocationException e)
                    {
                        throw new Errors.DataError(className + " could not be created. Probably cause would be a non-public constructor, or constructor parameters", classToJar.Get(className));
                    }
                    catch (Exception e)
                    {
                        e.printStackTrace();
                        throw new RuntimeException("some weirdness with loading scripts. See std err");
                    }
                }
            }
        }

        private JarInputStream CopyAndMakeJarUrl(string jarFile)
        {
            if (!pathRoot.Exists(jarFile))
            {
                LOG.Err("script: " + pathRoot.Get() + Path.DirectorySeparatorChar + jarFile + " does not exist and will be ignored.");
                return null;
            }
            LOG.Ln("loading script jar " + jarFile);

            try
            {
                FileInfo p = pathRoot.Get(jarFile);
                urlList.Add(p.FullName);
                return new JarInputStream(File.OpenRead(p.FullName));
            }
            catch (IOException e)
            {
                LOG.Err("script: " + pathRoot.Get() + Path.DirectorySeparatorChar + jarFile + " unable to cache! Ignoring.");
                e.printStackTrace();
            }
            return null;
        }

        private void ProcessJarEntry(JarEntry jarEntry, string jarFile)
        {
            if (jarEntry.Name.EndsWith(".class"))
            {
                string className = jarEntry.Name;
                className = jarEntry.Name.Substring(0, jarEntry.Name.Length - 6);
                className = className.Replace("/", ".");

                if (classToJar.ContainsKey(className))
                {
                    return;
                }

                try
                {
                    if (Type.GetType(className) != null)
                    {
                        throw new Errors.DataError(className + " already exist in the game and will clash with the game. This class needs to be renamed", pathRoot.Get(jarFile));
                    }
                }
                catch (Exception e)
                {
                }
                catch (TypeLoadException e)
                {
                    throw new Errors.DataError(className + " could not be loaded.", pathRoot.Get(jarFile));
                }

                classToJar.Put(className, jarFile);
            }
        }

        private static T ForceInit<T>(Type klass) where T : class
        {
            try
            {
                Type.GetType(klass.FullName, true, klass.Assembly);
            }
            catch (TypeLoadException e)
            {
                throw new AssertionError(e); // Can't happen
            }
            return (T)klass;
        }
    }
}