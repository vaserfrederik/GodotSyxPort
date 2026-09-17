using System;
using snake2d;
using util.gui.misc;
using view.subview;

public abstract class Tool
{
    private static readonly ToolConfig normal = new ToolConfig();

    protected virtual void Update(float ds, GameWindow window) { }

    protected abstract void UpdateHovered(float ds, GameWindow window);

    protected virtual void Render(SPRITE_RENDERER r, float ds, GameWindow window) { }

    protected abstract void RenderHovered(SPRITE_RENDERER r, float ds, GameWindow window, GBox box);

    protected abstract void Click(GameWindow window);

    protected ToolConfig DefaultConfig()
    {
        return normal;
    }

    private readonly ToolManager manager;

    protected Tool(ToolManager manager)
    {
        this.manager = manager;
    }

    protected virtual bool RightClick()
    {
        return true;
    }

    protected ToolManager Manager()
    {
        return manager;
    }

    public final void Deactivate()
    {
        manager().Set(null, null, true);
    }

    public bool IsActivated()
    {
        return manager().Current() == this;
    }
}