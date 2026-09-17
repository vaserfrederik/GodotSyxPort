using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.paths
{
    public static class PathParser
    {
        public const string split = "->";

        public static Path Get(PATH basePath, string relPath, Json json, string jsonKey, int off)
        {
            string[] ss = relPath.Split(split);
            if (ss.Length < 1 - off || ss.Length - 1 - off < 0)
            {
                string e = relPath + " does not specify a path with the root of: " + basePath.Get().FullName + " paths are specified by folder->folder->file, where the folder part is optional. Both the folders and the file must exist.";
                Error(e, json, jsonKey);
                return null;
            }

            PATH p = basePath;
            for (int i = 0; i < ss.Length - 1 - off; i++)
            {
                if (!p.ExistsFolder(ss[i]))
                {
                    string e = "The folder specified: " + ss[i] + ", does not exist in: " + p.Get().FullName;
                    Error(e, json, jsonKey);
                    return null;
                }
                p = p.GetFolder(ss[i]);
            }
            string file = ss[ss.Length - 1 - off];

            if (!p.Exists(file))
            {
                string e = "The file: " + file + p.FileEnding() + ", does not exist in: " + p.Get().FullName;
                Error(e, json, jsonKey);
                return null;
            }

            return p.Get(file);
        }

        public static LIST<Path> GetMany(PATH basePath, string relPath, Json json, string jsonKey)
        {
            LinkedList<Path> res = new LinkedList<Path>();

            string[] ss = relPath.Split(split);
            if (ss.Length < 1)
            {
                string e = relPath + " does not specify a path with the root of: " + basePath.Get().FullName + " paths are specified by folder->folder->file, where the folder part is optional. Both the folders and the file must exist.";
                Error(e, json, jsonKey);
                return null;
            }

            PATH p = basePath;
            for (int i = 0; i < ss.Length - 1; i++)
            {
                if (!p.ExistsFolder(ss[i]))
                {
                    string e = "The folder specified: " + ss[i] + ", does not exist in: " + p.Get().FullName;
                    Error(e, json, jsonKey);
                    return null;
                }
                p = p.GetFolder(ss[i]);
            }
            string file = ss[ss.Length - 1];

            if (file.EndsWith('*'))
            {
                string begin = file.Substring(0, file.Length - 1);
                foreach (string f in p.GetFiles())
                {
                    if (f.StartsWith(begin))
                    {
                        res.Add(p.Get(f));
                    }
                }
                if (res.Count == 0)
                {
                    string e = "There are no files: " + relPath + ", that match this pattern: " + p.Get().FullName;
                    Error(e, json, jsonKey);
                }

                return res;
            }

            if (!p.Exists(file))
            {
                string e = "The file: " + file + p.FileEnding() + ", does not exist in: " + p.Get().FullName;
                Error(e, json, jsonKey);
                return null;
            }

            res.Add(p.Get(file));

            return res;
        }

        public static void Error(string error, Json json, string jsonKey)
        {
            if (json != null)
            {
                GAME.Warn(json.ErrorGet(error, jsonKey));
            }
            else
            {
                throw new Errors.DataError(error);
            }
        }
    }
}