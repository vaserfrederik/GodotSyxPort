using System.IO;
using init.paths;
using snake2d.util.file;

namespace init.sprite.imps
{
    public class CustomSprites
    {
        public CustomSprites() throws IOException
        {
            Json j = new Json(PATHS.CONFIG().init.gets("GameSprites"));
            new SpriteHead(j);
            new SpriteSkulls(j);
        }
    }
}