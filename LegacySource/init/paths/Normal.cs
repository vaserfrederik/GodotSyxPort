using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.sets;

namespace init.paths
{
    internal sealed class Normal : PATH
    {
        private readonly VirtualFolder f;

        public Normal(string path, string filetype)
            : base(filetype)
        {
            f = new VirtualFolder(PATHS.i.paths, path);
        }

        public Normal(LIST<Path> roots, string path, string filetype)
            : base(filetype)
        {
            f = new VirtualFolder(roots, path);
        }

        public Normal(VirtualFolder f, string filetype)
            : base(filetype)
        {
            this.f = f;
        }

        public override string[] getFiles()
        {
            return f.listFiles(filetype);
        }

        public override string[] getFilesOrdered()
        {
            return f.listFilesOrdered(filetype);
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

        protected override PATH getFolder(CharSequence folder, string filetype, bool create)
        {
            return new Normal(f.folder(folder), filetype);
        }

        public override bool exists(CharSequence file)
        {
            return f.exists(file, filetype);
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
            return f.exists(file, filetype);
        }
    }
}