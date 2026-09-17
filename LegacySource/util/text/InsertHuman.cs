using System;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;
using util.data;

final class InsertHuman : Inserter<Humanoid>
{
    InsertHuman()
    {
        new II("TITLE")
        {
            public override void Set(Humanoid t, Str str)
            {
                str.Add(t.title());
            }
        };

        new II("WEIGHT")
        {
            public override void Set(Humanoid a, Str str)
            {
                str.Add(a.physics.getMass(), 1);
            }
        };

        new II("HEIGHT")
        {
            public override void Set(Humanoid a, Str str)
            {
                str.Add(a.physics.getHeight(), 1);
            }
        };

        new II("LOC")
        {
            public override void Set(Humanoid b, Str str)
            {
                if (b != null)
                {
                    DIR d = DIR.Get(SETT.TWIDTH / 2, SETT.THEIGHT / 2, b.tc().x(), b.tc().y());
                    if (COORDINATE.tileDistance(SETT.TWIDTH / 2, SETT.THEIGHT / 2, b.tc().x(), b.tc().y()) < 150)
                        d = DIR.C;
                    str.Add(Dic.get(d));
                }
            }
        };

        Join(new InsertIndu(), new GETTER_TRANS<Humanoid, Induvidual>()
        {
            public override Induvidual Get(Humanoid f)
            {
                if (f == null)
                    return null;
                return f.indu();
            }
        });
    }
}