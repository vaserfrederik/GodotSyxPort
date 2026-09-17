using System;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;

namespace view.main
{
    public sealed class Mouse : COORDINATE
    {
        private readonly Coo _coo = new Coo();
        private readonly SPRITE _sprites;
        private bool _hidden = false;
        private SPRITE _overlay = null;

        public Mouse()
        {
            _sprites = UI.decor().mouse;
        }

        public void Render(Renderer r, float ds)
        {
            if (_hidden)
            {
                _hidden = false;
                return;
            }

            if (_overlay == null && CLICKABLE.ClickableAbs.clickableHovered)
            {
                UI.decor().mouseHov.render(r, _coo.x(), _coo.y());
            }
            else
            {
                _sprites.render(r, _coo.x(), _coo.y());
            }

            if (_overlay != null)
            {
                _overlay.render(r, _coo.x() + 10, _coo.y());
            }

            _overlay = null;
            CLICKABLE.ClickableAbs.clickableHovered = false;
        }

        public void SetReplacement(SPRITE o)
        {
            _overlay = o;
        }

        public void Hide(bool hide)
        {
            _hidden = hide;
        }

        public int x()
        {
            return _coo.x();
        }

        public int y()
        {
            return _coo.y();
        }

        public bool IsWithinRec(RECTANGLE shape)
        {
            return _coo.isWithinRec(shape);
        }

        internal Coo GetCoo()
        {
            return _coo;
        }
    }
}