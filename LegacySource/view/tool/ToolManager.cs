using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using view.interrupter;
using view.main;
using view.subview;

public sealed class ToolManager : Interrupter
{
    private ToolConfig config;
    private Tool current;
    private Tool def;
    private bool hovered;
    public readonly ToolPlacer placer;
    private readonly GameWindow window;
    private readonly ToolConfig configDummy = new ToolConfig();
    private ArrayList<RENDEROBJ> rens = new ArrayList<RENDEROBJ>(16);

    public ToolManager(InterManager manager, GameWindow window)
    {
        this.window = window;

        placer = new ToolPlacer(this, window);
        lastSet();
        pin();
        persistantSet();

        show(manager);
    }

    public GameWindow window()
    {
        return window;
    }

    public ToolManager setDefault(Tool def)
    {
        this.def = def;
        if (current == null)
            set(def, def.defaultConfig(), true);
        return this;
    }

    public void setHovered(bool hovered)
    {
        this.hovered = hovered;
    }

    public bool isHovered()
    {
        return hovered;
    }

    public override bool update(float ds)
    {
        window.update(ds);
        if (current == null)
            return true;

        config.update(!hovered);
        if (current != null)
        {
            if (hovered)
            {
                current.updateHovered(ds, window);
            }
            else
            {
                current.update(ds, window);
            }
        }

        return true;
    }

    public void place(PLACABLE placer, ToolConfig config)
    {
        if (placer == null)
        {
            set(null, null, true);
            return;
        }
        this.placer.activate(placer);
        set(this.placer, config, true);
    }

    public void place(PLACABLE placer, ToolConfig config, bool disturb)
    {
        if (placer == null)
        {
            set(null, null, disturb);
            return;
        }
        this.placer.activate(placer);
        set(this.placer, config, disturb);
    }

    public void place(PLACABLE placer)
    {
        if (placer == null)
        {
            set(null, null, true);
            return;
        }
        this.placer.activate(placer);
        set(this.placer, null, true);
    }

    public void set(Tool t)
    {
        if (placer == null)
        {
            set(null, null, true);
            return;
        }
        set(t, t.defaultConfig(), true);
    }

    public void set(Tool t, ToolConfig config, bool disturb)
    {
        rens.clear();
        if (config != null)
            config.addUI(rens);

        if (t == current && this.config == config)
            return;

        ToolConfig old = this.config;

        if (t == null)
        {
            t = def;
        }

        this.config = config;
        if (this.config == null && t != null)
            this.config = t.defaultConfig();

        if (this.config == null)
            this.config = configDummy;

        this.config.activateAction();

        current = t;
        if (disturb)
            manager().disturb();
        if (added.isActivated())
            manager().remove(added);
        manager().add(added);

        if (old != null)
            old.deactivateAction();
    }

    public bool is(PLACABLE t)
    {
        return isActivated() && current == this.placer && this.placer.getCurrent() == t;
    }

    protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
    {
        hovered = true;

        foreach (RENDEROBJ r in rens)
        {
            if (r is HOVERABLE)
            {
                if (((HOVERABLE)r).hover(mCoo))
                {
                    hovered = false;
                }
            }
        }
        if (hovered)
        {
            window.hover();
        }
        return true;
    }

    protected override void mouseClick(MButt button)
    {
        if (button == MButt.LEFT)
        {
            foreach (RENDEROBJ r in rens)
            {
                if (r is CLICKABLE)
                {
                    if (((CLICKABLE)r).hoveredIs())
                    {
                        ((CLICKABLE)r).click();
                        return;
                    }
                }
            }
            if (hovered && current != null)
                current.click(window);
        }
        else
            otherClick(button);
    }

    protected override bool otherClick(MButt button)
    {
        if (button == MButt.RIGHT)
        {
            if (current != null && current.rightClick())
            {
                if (config.back())
                    set(null, null, false);
            }
            return true;
        }
        return false;
    }

    protected override void hoverTimer(GBox text)
    {
        foreach (RENDEROBJ r in rens)
        {
            if (r is HOVERABLE)
            {
                if (((HOVERABLE)r).hoveredIs())
                    ((HOVERABLE)r).hoverInfoGet(text);
            }
        }
    }

    protected override bool render(Renderer r, float ds)
    {
        if (rens.size() > 0)
        {
            foreach (RENDEROBJ re in rens)
            {
                re.render(r, ds);
            }
        }

        r.newLayer(true, window.zoomout());

        if (current == null)
            return true;
        if (hovered)
            current.renderHovered(r, ds, window, VIEW.hoverBox());
        else
            current.render(r, ds, window);

        return true;
    }

    protected override void afterTick()
    {
        rens.clear();
        if (current != null)
            config.addUI(rens);
        hovered = false;
    }

    public Tool current()
    {
        return current;
    }

    private readonly Interrupter added = new Interrupter()
    {
        protected override bool update(float ds)
        {
            return true;
        }

        protected override bool render(Renderer r, float ds)
        {
            return true;
        }

        protected override void mouseClick(MButt button)
        {
        }

        protected override bool otherClick(MButt button)
        {
            if (current != null && current != def)
                return ToolManager.this.otherClick(button);
            hide();
            return false;
        }

        protected override void hoverTimer(GBox text)
        {
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return false;
        }
    };

    public ToolConfig configCurrent()
    {
        return config;
    }
}