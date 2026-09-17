using System;
using System.IO;

namespace util.spritecomposer
{
    public abstract class Initer
    {
        public Initer()
        {
        }

        public abstract void CreateAssets();

        public TextureHolder Get(string prefix, int WIDTH, int extraHeight)
        {
            try
            {
                TextureHolder t = Read(prefix, WIDTH);
                if (t != null)
                    return t;
            }
            catch (IOException e)
            {
            }
            Resources.Delete(prefix);

            try
            {
                TryCreate(prefix, WIDTH, extraHeight);
            }
            catch (IOException e)
            {
                throw new RuntimeException(e);
            }

            try
            {
                TextureHolder t = Read(prefix, WIDTH);
                if (t == null)
                    throw new RuntimeException("saved texture cache could not be loaded. Ensure the local file folder has access");
                return t;
            }
            catch (IOException e)
            {
                throw new RuntimeException(e);
            }
        }

        private void TryCreate(string prefix, int WIDTH, int extraHeight) throws IOException
        {
            GAME.Notify("creating new texture atlases " + prefix);
            Resources.Init(prefix, WIDTH);
            CreateAssets();
            Resources.Save(prefix, extraHeight);
        }

        private TextureHolder Read(string prefix, int WIDTH) throws IOException
        {
            Resources.Dispose();
            Result res = Resources.Read(prefix, WIDTH);
            if (res == null)
                return null;
            try
            {
                CreateAssets();
            }
            catch (IOException e)
            {
                CORE.DisposeClient();
                res.diffuse.Dispose();
                res.normal.Dispose();
                throw e;
            }
            return new TextureHolder(res.diffuse, res.normal, 0, Optimizer.Get(16).startY, 16, 16);
        }
    }
}