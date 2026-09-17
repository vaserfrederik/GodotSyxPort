using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Paths
{
    internal sealed class VanillaOnly : PATH
    {
        private readonly VirtualFolder f;

        public VanillaOnly(Path path, string filetype, bool create) : base(filetype)
        {
            if (create)
                Util.MakeDirs(path);
            var pp = new ArrayList<Path>(path);
            f = new VirtualFolder(pp, "");
        }

        public override string[] GetFiles()
        {
            return f.ListFiles(filetype);
        }

        public override string[] GetFilesOrdered()
        {
            return f.ListFilesOrdered(filetype);
        }

        public override string[] Folders()
        {
            return f.ListFolders();
        }

        protected override Path GetRaw(CharSequence resource)
        {
            return f.GetExistingFile(resource);
        }

        protected override Path[] GetRaws(CharSequence resource)
        {
            return f.GetExistingFiles(resource);
        }

        protected override void Validate()
        {
        }

        protected override PATH GetFolder(CharSequence folder, string filetype, bool create)
        {
            if (create)
            {
                Path p = f.GetExistingFile(null).Resolve(folder.ToString());
                if (!Directory.Exists(p.ToString()))
                {
                    try
                    {
                        Directory.CreateDirectory(p.ToString());
                    }
                    catch (IOException e)
                    {
                        e.printStackTrace();
                    }
                }
            }
            return new VanillaOnly(f.GetExistingFile(folder), filetype, create);
        }

        public override bool Exists(CharSequence file)
        {
            return f.Exists(file, filetype);
        }

        public override Path Get()
        {
            return f.GetExistingFile(null);
        }

        public override bool ExistsFolder(CharSequence folder)
        {
            return f.Exists(folder, "");
        }

        public override bool Exists(CharSequence file, CharSequence fileType)
        {
            return f.Exists(file, filetype);
        }
    }
}