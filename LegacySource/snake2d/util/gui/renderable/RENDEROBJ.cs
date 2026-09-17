using System;

namespace snake2d.util.gui.renderable
{
    public interface RENDEROBJ : BODY_HOLDERE
    {
        void render(SPRITE_RENDERER r, float ds);
        bool visableIs();
        RENDEROBJ visableSet(bool yes);

        default SPRITE asSprite()
        {
            return new SPRITE.Imp(body().width(), body().height())
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    body().moveX1Y1(X1, Y1);
                    this.render(r, 0);
                }
            };
        }
    }

    public abstract class RenderImp : RENDEROBJ
    {
        protected readonly Rec body = new Rec();
        private bool isVisable = true;

        public RenderImp(int width, int height)
        {
            body.setDim(width, height);
        }

        public RenderImp()
        {
        }

        public RenderImp(int size) : this(size, size)
        {
        }

        public RecFacade body()
        {
            return body;
        }

        public bool visableIs()
        {
            return isVisable;
        }

        public RenderImp visableSet(bool yes)
        {
            isVisable = yes;
            return this;
        }
    }

    public sealed class RenderDummy : RENDEROBJ
    {
        protected readonly Rec body = new Rec();

        public RenderDummy(int width, int height)
        {
            body.setDim(width, height);
        }

        public RenderDummy(int size) : this(size, size)
        {
        }

        public RecFacade body()
        {
            return body;
        }

        public bool visableIs()
        {
            return true;
        }

        public RenderDummy visableSet(bool yes)
        {
            return this;
        }

        public void render(SPRITE_RENDERER r, float ds)
        {
            // TODO Auto-generated method stub
        }
    }

    public class Sprite : RenderImp, DIMENSION
    {
        private SPRITE sprite;
        private COLOR color = COLOR.WHITE100;
        private DIR align = DIR.NW;

        public Sprite()
        {
        }

        public Sprite(int dim)
        {
            body.setDim(dim);
        }

        public Sprite(int width, int height)
        {
            body.setDim(width, height);
        }

        public Sprite(SPRITE sprite)
        {
            setSprite(sprite);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            if (sprite != null)
            {
                adjust();
                color.bind();
                sprite.render(r, body.x1(), body.y1());
                COLOR.unbind();
            }
        }

        public Sprite setAlign(DIR d)
        {
            this.align = d;
            return this;
        }

        public RENDEROBJ setSprite(SPRITE sprite)
        {
            this.sprite = sprite;
            if (sprite != null)
                adjust();
            return this;
        }

        private void adjust()
        {
            if (body.width() != sprite.width() || body.height() != sprite.height())
            {
                align.reposition(body, sprite.width(), sprite.height());
            }
        }

        public Sprite setColor(COLOR color)
        {
            this.color = color;
            return this;
        }

        public int width()
        {
            return sprite.width();
        }

        public int height()
        {
            return sprite.height();
        }

        public Rec body()
        {
            return body;
        }
    }
}