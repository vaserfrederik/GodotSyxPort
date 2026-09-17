using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Init.Paths
{
    class Util
    {
        private Util()
        {
        }

        static string GetLocal()
        {
            string OS = Environment.OSVersion.Platform.ToString().ToUpper();

            if (OS.Contains("MAC") || OS.Contains("DARWIN"))
            {
                string s = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                s += Path.DirectorySeparatorChar + "Library" + Path.DirectorySeparatorChar + "Application Support" + Path.DirectorySeparatorChar + "songsofsyx";
                return s;
            }
            else if (OS.Contains("WIN"))
            {
                return Environment.GetEnvironmentVariable("AppData") + Path.DirectorySeparatorChar + "songsofsyx";
            }
            else if (OS.Contains("NUX"))
            {
                string s = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                s += Path.DirectorySeparatorChar + ".local" + Path.DirectorySeparatorChar + "share" + Path.DirectorySeparatorChar + "songsofsyx";
                return s;
            }
            else
            {
                throw new Exception("could not figure out OS " + Environment.OSVersion.Platform.ToString().ToUpper());
            }
        }

        static void MakeDirs(DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                try
                {
                    dir.Create();
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }

                if (!dir.Exists)
                    throw new DataError("The game could not create a directory for game files. Please check permissions", dir.FullName);
            }

            if (!dir.Exists)
            {
                throw new DataError("The game could not read from its file directory. Please check permissions", dir.FullName);
            }
        }

        static string Abort(string missingfile) throws DataError
        {
            string ending = "";

            string root = missingfile;

            throw new DataError("The file or directory does not exist. Try to reinstall the game.", root + ending);
        }

        static bool Check(Path path)
        {
            if (path == null)
                return false;
            if (!File.Exists(path.FullName))
                return false;

            DirectoryInfo pa = path.Directory;

            string cmp = path.Name;
            if (cmp.EndsWith(Path.DirectorySeparatorChar))
                cmp = cmp.Substring(0, cmp.Length - Path.DirectorySeparatorChar.ToString().Length);
            foreach (var p in ListFiles(pa))
            {
                if (p.Name.StartsWith(cmp))
                {
                    return true;
                }
            }
            return false;
        }

        static Path CheckHard(Path path, string file)
        {
            path = path.Combine(file);
            if (!File.Exists(path.FullName))
            {
                throw new DataError("The file or directory does not exist. Try to reinstall the game.", path.FullName);
            }
            return path;
        }

        static List<Path> ListFiles(Path path)
        {
            List<Path> res = new List<Path>();

            if (path == null || !Directory.Exists(path.FullName))
                return res;

            try
            {
                var it = Directory.EnumerateFileSystemEntries(path.FullName).Select(p => new Path(p));
                foreach (var p in it)
                {
                    res.Add(p);
                }
            }
            catch (IOException e)
            {
                e.printStackTrace();
            }

            return res;
        }
    }

    public class Path
    {
        public string FullName { get; private set; }

        public Path(string fullPath)
        {
            FullName = fullPath;
        }

        public string Name => Path.GetFileName(FullName);
        public DirectoryInfo Directory => Directory.GetParent(FullName);
    }

    public class DataError : Exception
    {
        public DataError(string message, string path) : base(message)
        {
            // Additional handling or logging can be done here
        }
    }
}