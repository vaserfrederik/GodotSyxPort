using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d;

namespace init.paths
{
    static class Util
    {
        public static bool check(Path path)
        {
            return path.Exists;
        }

        public static IEnumerable<Path> listFiles(Path path)
        {
            if (path.Exists && path.IsDirectory)
            {
                foreach (var file in Directory.EnumerateFiles(path.ToString()))
                {
                    yield return new Path(file);
                }
            }
        }
    }

    final class VirtualFolder
    {
        private readonly LIST<Path> bases;
        private readonly string appendix;

        public VirtualFolder(LIST<Path> bases, string path)
        {
            this.bases = bases;
            this.appendix = path;
            Validate();
        }

        private Path Validate(Path path)
        {
            if (!Util.check(path))
            {
                throw new Errors.DataError("This file/directory does not exist: " + path,
                    path);
            }

            return path;
        }

        private void Validate()
        {
            foreach (Path p in bases)
            {
                Path f = Resolve(p, null);
                if (File.Exists(f.ToString()))
                {
                    if (!Directory.Exists(f.ToString()))
                        throw new Errors.DataError("This file is not a directory: .",
                            Path.GetFullPath(f.ToString()));
                    return;
                }
            }
            Path path = Resolve(bases.Get(bases.Size() - 1), null);
            throw new Errors.DataError("This file/directory does not exist: " + path,
                path);
        }

        public VirtualFolder Folder(CharSequence next)
        {
            if (appendix == null || appendix.Length <= 1)
                return new VirtualFolder(bases, "" + next + PATHS.s);

            return new VirtualFolder(bases, appendix + PATHS.s + next);
        }

        public Path GetExistingFile(CharSequence name)
        {
            Path p = GetPossibleFile(name);
            if (p == null)
                throw new Errors.DataError("This resource could not be found: ", Resolve(bases.Get(bases.Size() - 1), "" + name));
            return p;
        }

        public Path[] GetExistingFiles(CharSequence name)
        {
            if (name == null)
                name = "";
            string r = "" + name;

            int am = 0;

            foreach (Path root in bases)
            {
                Path p = Resolve(root, r);
                if (File.Exists(p.ToString()))
                {
                    am++;
                }
            }

            Path[] pps = new Path[am];
            am = 0;

            foreach (Path root in bases)
            {
                Path p = Resolve(root, r);
                if (File.Exists(p.ToString()))
                {
                    Validate(p);
                    pps[am++] = p;
                }
            }
            if (am == 0)
                throw new Errors.DataError("This resource could not be found: ", Resolve(bases.Get(bases.Size() - 1), "" + name));
            return pps;
        }

        public bool Exists(CharSequence file, CharSequence filetype)
        {
            string f = "" + file + filetype;
            foreach (Path m in bases)
            {
                if (Util.check(Resolve(m, f)))
                    return true;
            }
            return false;
        }

        public Path GetPossibleFile(CharSequence name)
        {
            if (name == null)
                name = "";
            string r = "" + name;

            foreach (Path root in bases)
            {
                Path p = Resolve(root, r);
                if (File.Exists(p.ToString()))
                {
                    Validate(p);
                    return p;
                }

            }

            return null;
        }

        private Path Resolve(Path basePath, string resource)
        {
            if (appendix.Length > 1)
                basePath = basePath.Combine(appendix);
            if (resource == null || resource.Length == 0)
                return basePath;
            try
            {
                return basePath.Combine(resource);
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
                return null;
            }
        }

        public string[] ListFiles(string ending)
        {
            if (ending.Length <= 1)
                throw new RuntimeException();

            HashSet<string> map = new HashSet<string>();
            bool ignore = false;
            for (int i = 0; i < bases.Size(); i++)
            {
                Path m = bases.Get(i);
                if (i == bases.Size() - 1)
                {
                    if (ignore)
                        continue;
                }
                else
                {
                    ignore |= CheckIgnore(Resolve(m, null));
                }
                foreach (string s in List(Resolve(m, null), ending))
                {
                    map.Add(s);
                }
            }

            string[] all = map.ToArray();
            Array.Sort(all);

            return all;
        }

        public string[] ListFilesOrdered(string ending)
        {
            if (ending.Length <= 1)
                throw new RuntimeException();

            HashSet<string> map = new HashSet<string>();
            for (int i = 0; i < bases.Size(); i++)
            {
                Path m = bases.Get(i);
                foreach (string s in List(Resolve(m, null), ending))
                {
                    map.Add(s);
                }
            }

            string[] all = new string[map.Count];
            map.Clear();
            int k = 0;
            for (int i = 0; i < bases.Size(); i++)
            {
                Path m = bases.Get(i);
                foreach (string s in List(Resolve(m, null), ending))
                {
                    if (!map.Contains(s))
                    {
                        map.Add(s);
                        all[k++] = s;
                    }
                }
            }
            return all;
        }

        public string[] ListFolders()
        {
            HashSet<string> map = new HashSet<string>();

            for (int i = 0; i < bases.Size(); i++)
            {
                Path m = Resolve(bases.Get(i), null);
                final string sep = m.GetFileSystem().GetSeparator();
                foreach (Path p in Util.listFiles(m))
                {
                    if (Directory.Exists(p.ToString()))
                    {
                        string s = "" + p.GetFileName();
                        if (s.StartsWith("_"))
                            continue;
                        if (s.EndsWith(sep))
                        {
                            s = s.Substring(0, s.Length - sep.Length);
                        }
                        map.Add(s);
                    }
                }
                if (CheckIgnore(Resolve(m, null)))
                    break;
            }

            string[] all = map.ToArray();
            Array.Sort(all);

            return all;
        }

        private bool CheckIgnore(Path path)
        {
            path = path.Combine("_IgnoreVanilla.txt");
            return File.Exists(path.ToString());
        }

        private static string[] List(Path path, string ending)
        {
            if (!File.Exists(path.ToString()))
                return new string[0];
            if (!Directory.Exists(path.ToString()))
                throw new Errors.DataError("This file should be a directory, but is not...",
                    "" + path);

            LinkedList<string> res = new LinkedList<string>();

            foreach (Path p in Util.listFiles(path))
            {
                string s = "" + p.GetFileName();
                res.Add(s);
            }
            return Clean(0, ending, 0, res);
        }

        private static string[] Clean(int index, string ending, int size, LinkedList<string> origional)
        {
            int am = 0;
            foreach (string s in origional)
                if (GetClean(s, ending) != null)
                    am++;
            string[] res = new string[am];
            am = 0;
            foreach (string s in origional)
            {
                string c = GetClean(s, ending);
                if (c != null)
                    res[am++] = c;
            }
            return res;
        }

        private static string GetClean(string s, string ending)
        {
            if (s[0] == '_')
                return null;
            if (ending != null && !s.EndsWith(ending))
                return null;
            return s.Substring(0, s.Length - ending.Length);
        }
    }
}