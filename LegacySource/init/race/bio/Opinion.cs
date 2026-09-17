using System;
using System.Collections.Generic;
using System.Text;
using settlement.entity.humanoid;
using settlement.stats.stat;
using snake2d.util.file;
using snake2d.util.sprite.text;
using util.text;

public sealed class Opinion
{
    private static readonly string[] dm = new string[] { Dic.¤¤More + ": {0}" };
    private static readonly string[] dl = new string[] { Dic.¤¤Less + ": {0}" };

    string[] more = dm;
    string[] less = dl;

    public static readonly Opinion DEF = new Opinion();

    public Opinion() { }

    public Opinion(Json morg, Json mspe, Json lorg, Json lspe, string key)
    {
        more = get(morg, mspe, key, dm);
        less = get(lorg, lspe, key, dl);
    }

    private string[] get(Json org, Json spe, string key, string[] backup)
    {
        Json j = org;
        if (spe != null && spe.has(key))
            j = spe;

        if (j != null && j.has(key))
        {
            string[] ss = j.texts(key);
            if (ss.Length > 0)
                return ss;
        }
        return backup;
    }

    public Opinion setMore(params string[] more)
    {
        if (more == null || more.Length == 0)
            this.more = dm;
        else
            this.more = more;
        return this;
    }

    public Opinion setLess(params string[] less)
    {
        if (less == null || less.Length == 0)
            this.less = dl;
        else
            this.less = less;
        return this;
    }

    void insert(Str prep, STAT stat, Humanoid a)
    {
        prep.insert(0, stat.info().name);
        BioLine.insert.set(prep, a);
    }

    public void read(Json json)
    {
        if (json.has("MORE"))
            setMore(BioLine.insert.check(json.texts("MORE")));
        if (json.has("LESS"))
            setLess(BioLine.insert.check(json.texts("LESS")));
    }
}