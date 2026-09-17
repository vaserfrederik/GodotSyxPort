using System;
using view.sett.ui.minimap;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util.gui.misc;
using view.interrupter;
using view.keyboard;
using view.subview;

public class UIMinimapSett : Interrupter
{
    final UIMiniMapSettView view;
    final UIMinimapPanel map;
    private readonly UIMinimapPanelButts buttons;
    public readonly UIMinimapSettConfig config;

    public UIMinimapSett(InterManager i, int y1, GameWindow w, UIMinimapSettConfig config)
    {
        if (config == null)
            config = UIMinimapSettConfig.NORMAL;
        this.config = config;
        desturberSet().persistantSet().pin();
        view = new UIMiniMapSettView(this, i, w, config);

        map = new UIMinimapPanel(w, config);
        map.body().moveX2(C.WIDTH());
        map.body().moveY1(y1);

        buttons = new UIMinimapPanelButts(view, map, w);
        buttons.section.body().moveX2(C.WIDTH());
        buttons.section.body().moveY1(map.body().y2());

        update(0);
        show(i);
    }

    public int y2()
    {
        return buttons.section.body().y2();
    }

    public void add()
    {
    }

    public void open()
    {
        view.showMin();
    }

    public bool openIs()
    {
        return view.isActivated();
    }

    protected override void hoverTimer(GBox text)
    {
        buttons.section.hoverInfoGet(text);
    }

    protected override bool render(Renderer r, float ds)
    {
        map.render(r, ds);
        buttons.section.render(r, ds);

        return true;
    }

    protected override void mouseClick(MButt button)
    {
        if (button == MButt.LEFT)
        {
            map.click();
            buttons.section.click();
        }
    }

    protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
    {
        if (map.hover(mCoo) || buttons.section.hover(mCoo))
            return true;
        return false;
    }

    protected override bool update(float ds)
    {
        if (KEYS.MAIN().MINIMAP.consumeClick())
        {
            view.show();
        }
        return true;
    }

    public UIMinimapPanelButts panel()
    {
        return buttons;
    }

    public class Butt : GButt.ButtPanel
    {
        public Butt(SPRITE icon) : base(icon)
        {
            setDim(30, 26);
        }
    }
}