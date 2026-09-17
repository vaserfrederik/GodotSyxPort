using System;
using System.IO;
using System.Linq;

namespace Init.Paths
{
    public abstract class PATH
    {
        protected readonly string filetype;

        protected PATH(string filetype)
        {
            this.filetype = filetype;
        }

        public Path Get(CharSequence resource)
        {
            if (filetype.Equals(PATHS.s))
                return GetRaw(resource);
            if ((resource.ToString()).EndsWith(filetype))
                return GetRaw(resource);
            return GetRaw(resource + filetype);
        }

        public Path[] Gets(CharSequence resource)
        {
            if (filetype.Equals(PATHS.s))
                return GetRaws(resource);
            if ((resource.ToString()).EndsWith(filetype))
                return GetRaws(resource);
            return GetRaws(resource + filetype);
        }

        public Path GetLikeHell(CharSequence resource)
        {
            return GetRaw(resource);
        }

        public Path[] GetLikeHells(CharSequence resource)
        {
            return GetRaws(resource);
        }

        protected abstract Path GetRaw(CharSequence resource);

        protected abstract Path[] GetRaws(CharSequence resource);

        public final PATH GetFolder(CharSequence folder)
        {
            return GetFolder(folder, filetype);
        }

        public PATH GetFolder(CharSequence folder, string filetype)
        {
            return GetFolder(folder, filetype, false);
        }

        protected abstract PATH GetFolder(CharSequence folder, string filetype, bool create);

        protected PATH GetFolder(CharSequence folder, bool create)
        {
            return GetFolder(folder, filetype, create);
        }

        public abstract Path Get();

        public Path Create(CharSequence file)
        {
            Path p = Get().Resolve(file + filetype);
            try
            {
                if (File.Exists(p))
                    File.Delete(p);
                File.Create(p);
            }
            catch (IOException e)
            {
                e.PrintTrace();
                throw new Errors.DataError("Unable to process file", "" + p);
            }

            return p;
        }

        public void Delete(CharSequence file)
        {
            Path p = Get().Resolve(file + filetype);
            try
            {
                if (File.Exists(p))
                    File.Delete(p);
            }
            catch (IOException e)
            {
                e.PrintTrace();
                throw new Errors.DataError("Unable to delete file", "" + p);
            }
        }

        public abstract bool Exists(CharSequence file);

        public abstract bool Exists(CharSequence file, CharSequence fileType);

        public abstract bool ExistsFolder(CharSequence folder);

        public abstract string[] GetFiles();

        public abstract string[] GetFilesOrdered();

        public string[] GetFiles(int min)
        {
            string[] ss = GetFiles();
            if (ss.Length < min)
                throw new Errors.DataError("insufficient files declared. Needs at least " + min, "" + Get());
            return ss;
        }

        public string[] GetFiles(int min, int max)
        {
            string[] ss = GetFiles();
            if (ss.Length < min)
                throw new Errors.DataError("insufficient files declared. Needs at least " + min, "" + Get());
            else if (ss.Length > max)
            {
                throw new Errors.DataError("too many files declared. Max is: " + max, "" + Get());
            }
            return ss;
        }

        public abstract string[] Folders();

        public string FileEnding()
        {
            return filetype;
        }

        protected abstract void Validate();
    }
}