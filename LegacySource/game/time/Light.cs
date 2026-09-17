using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.light;
using snake2d.util.misc;
using util.data;
using util.gui.slider;
using view.interrupter;
using view.main;

public class Light
{
    private bool isNight = false;
    private double partOf;
    private double partOfCircular;

    private readonly Ambient abient = new Ambient();
    public readonly LightShadows shadow = new LightShadows();
    private readonly AmbientLight room = new AmbientLight(0.7, 0.5, 0.3, 0, 20);
    private readonly Gui gui = new Gui();

    DOUBLE time = new DOUBLE()
    {
        public override double getD()
        {
            return TIME.days().bitPartOf();
        }
    };

    public Light()
    {
        ACTION a = new ACTION()
        {
            public override void exe()
            {
                INT.IntImp inImp = new INT.IntImp(0, 300);
                time = inImp;
                GuiSection s = new GuiSection();
                s.add(new GSliderInt(inImp, 300, true));
                VIEW.inters().popup.show(s, s, true);
            }
        };
        IDebugPanel.add("Light Test", a);
    }

    public void bindRoom()
    {
        room.setTilt(20);
        room.setDir(180);
        double roomI = 1.0;
        if (dayIs())
        {
            roomI = 1.0 - partOfCircular();
            roomI *= roomI;
        }
        room.r(0.6 * roomI);
        room.g(0.3 * roomI);
        room.b(0.1 * roomI);
        CORE.renderer().lightDepthSet((byte)127);
        CORE.renderer().setTileLight(room);
    }

    public void apply(RECTANGLE rec, RGB mask)
    {
        apply(rec.x1(), rec.x2(), rec.y1(), rec.y2(), mask);
    }

    public void apply(int x1, int x2, int y1, int y2, RGB mask)
    {
        abient.apply(x1, x2, y1, y2, mask);
    }

    public void applyGuiLight(float ds, RECTANGLE rec)
    {
        gui.register(ds, rec);
    }

    public void applyGuiLight(float ds, int x1, int x2, int y1, int y2)
    {
        gui.register(ds, x1, x2, y1, y2);
    }

    public bool dayIs()
    {
        return !isNight;
    }

    public bool nightIs()
    {
        return isNight;
    }

    public double partOf()
    {
        return partOf;
    }

    public double partOfCircular()
    {
        return partOfCircular;
    }

    void update(double ds)
    {
        double dayL = TIME.seasons().currentDay.dayLength();
        double nightL = 1.0 - dayL;
        double now = time.getD();

        double dawn = nightL / 2.0;
        double dusk = dawn + dayL;

        if (now <= dawn)
        {
            partOf = 0.5 + 0.5 * now / dawn;
            isNight = true;
        }
        else if (now <= dusk)
        {
            partOf = (now - dawn) / dayL;
            isNight = false;
        }
        else
        {
            partOf = 0.5 * (now - dusk) / (nightL / 2);
            isNight = true;
        }

        if (partOf <= 0.5)
        {
            partOfCircular = partOf * 2.0;
        }
        else
        {
            partOfCircular = 1.0 - (partOf - 0.5) * 2.0;
        }

        shadow.update(this);
        gui.update(this, ds);
    }
}