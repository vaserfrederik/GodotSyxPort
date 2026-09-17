using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Paths
{
    final class ModOnly : PATH
    {
        private readonly VirtualFolder f;

        ModOnly(string path, string fileType, bool create)
            : base(fileType)
        {
            Path p = PATHS.i.paths[0];
            if (PATHS.currentMods().Count == 0)
                p = PATHS.i.paths[PATHS.i.paths.Count - 1];
            if (create)
                Util.makeDirs(Path.Combine(p, path));
            f = new VirtualFolder(new List<Path>(new[] { p }), path);
        }

        ModOnly(string patha, string path, string fileType, bool create)
            : base(fileType)
        {
            Path p = PATHS.i.paths[0];
            if (PATHS.currentMods().Count == 0)
                p = PATHS.i.paths[PATHS.i.paths.Count - 1];
            if (create)
                Util.makeDirs(Path.Combine(p, path));
            f = new VirtualFolder(new List<Path>(new[] { p }), path);
        }

        private ModOnly(VirtualFolder f, string fileType)
            : base(fileType)
        {
            this.f = f;
        }

        public override string[] getFiles()
        {
            return f.listFiles(fileType);
        }

        public override string[] getFilesOrdered()
        {
            return f.listFilesOrdered(fileType);
        }

        public override string[] folders()
        {
            return f.listFolders();
        }

        protected override Path getRaw(CharSequence resource)
        {
            return f.getExistingFile(resource);
        }

        protected override Path[] getRaws(CharSequence resource)
        {
            return f.getExistingFiles(resource);
        }

        protected override void validate()
        {
        }

        protected override PATH getFolder(CharSequence folder, string fileType, bool create)
        {
            if (create)
            {
                Path p = get();
                Util.makeDirs(Path.Combine(p, folder.ToString()));
            }
            return new ModOnly(f.folder(folder), fileType);
        }

        public override bool exists(CharSequence file)
        {
            return f.exists(file, fileType);
        }

        public override Path get()
        {
            return f.getExistingFile(null);
        }

        public override bool existsFolder(CharSequence folder)
        {
            return f.exists(folder, "");
        }

        public override bool exists(CharSequence file, CharSequence fileType)
        {
            return f.exists(file, fileType);
        }
    }
}