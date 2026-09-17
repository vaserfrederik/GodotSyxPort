using init.race.bio;
using snake2d.util.file;
using snake2d.util.sprite;
using util.info;

public class StatInfo : INFO
{
    private bool isInt = false;
    private bool matters = true;
    private bool hasIndu = true;
    public Opinion defOpinion = Opinion.DEF;
    public SPRITE icon = null;

    public StatInfo(StatInfo other) : base(other.name, other.names, other.desc, null)
    {
        this.isInt = other.isInt;
        this.matters = other.matters;
        this.hasIndu = other.hasIndu;
        defOpinion = other.defOpinion;
        icon = other.icon;
    }

    public StatInfo(Json json) : base(json)
    {
        defOpinion = new Opinion();
        defOpinion.read(json);
    }

    public StatInfo(CharSequence name, CharSequence desc) : base(name, desc)
    {
    }

    public StatInfo(CharSequence name, CharSequence names, CharSequence desc) : base(name, names, desc, null)
    {
    }

    public bool isInt()
    {
        return isInt;
    }

    public void setInt()
    {
        isInt = true;
    }

    public void setOpinion(Opinion op)
    {
        defOpinion = op;
    }

    public void setOpinion(CharSequence more, CharSequence less)
    {
        defOpinion = new Opinion().setMore(more).setLess(less);
    }

    public void setMatters(bool matters, bool hasIndu)
    {
        this.matters = matters;
        this.hasIndu = hasIndu;
    }

    public bool indu()
    {
        return hasIndu;
    }

    public bool matters()
    {
        return matters;
    }
}