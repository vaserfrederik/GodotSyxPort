using snake2d.util.color;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.info;
using util.keymap;

public sealed class HTYPE : INFO, MAPPED
{
    // HTYPE(LISTE<HTYPE> all, string key, HCLASS c, string name, string names, string desc, bool player, bool works, bool hostile, COLOR color, SPRITE icon)
    // {
    //     this(all, key, c, name, names, desc, player, works, hostile, player, color, icon);
    // }

    public HTYPE(LISTE<HTYPE> all, string key, HCLASS c, string name, string names, string desc, COLOR color, SPRITE icon)
        : base(name, names, desc, null)
    {
        this.color = color;
        this.CLASS = c;
        this.key = key;
        this.icon = icon;
        index = all.add(this);
        parent = this;
    }

    private readonly int index;
    public readonly string key;
    public bool hostile = false;
    public bool works = false;
    public bool visible = true;
    public readonly COLOR color;
    private HTYPE parent;
    public readonly HCLASS CLASS;
    public readonly SPRITE icon;

    private HTYPE child = null;

    public override string ToString()
    {
        return "" + name;
    }

    public int index()
    {
        return index;
    }

    public string key()
    {
        return key;
    }

    public bool visible()
    {
        return visible;
    }

    public HTYPE child()
    {
        return child;
    }

    public bool isWorks()
    {
        return works;
    }

    public bool isHostile()
    {
        return hostile;
    }

    public HCLASS parentClass()
    {
        return parent().CLASS;
    }

    public HTYPE parent()
    {
        return parent;
    }
}