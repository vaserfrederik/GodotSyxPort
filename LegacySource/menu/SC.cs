using System;

namespace Menu
{
    using Snake2D;
    using Snake2D.Util.DataTypes;

    public interface SC
    {
        bool Hover(COORDINATE mCoo);
        bool Click();
        void RenderBackground(Background back, float ds, COORDINATE mCoo);
        void Render(SPRITE_RENDERER r, float ds);
        bool Back(Menu menu);

        void Poll(KeyEvent e);
    }
}