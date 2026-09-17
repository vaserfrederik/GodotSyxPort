using System;
using System.Collections.Generic;
using System.IO;
using init.paths;
using init.race.appearence;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;
using util.text;

public class RACES
{
    private static RACES i;

    private readonly ArrayList<Race> all;
    private readonly ArrayList<Race> playable;
    private readonly RMAPS<Race> map;
    private RaceServiceSorter service;

    private static readonly CharSequence ¤¤name = "¤Species";

    private RaceSprites sprites;
    private readonly RaceBoosts boosts;
    private RaceResources resources;

    static RACES()
    {
        D.ts(typeof(RACES));
    }

    public RACES()
    {
        i = this;
        PATH p = PATHS.INIT().getFolder("race");
        PATH pt = PATHS.TEXT().getFolder("race");
        string[] files = p.getFiles();
        all = new ArrayList<Race>(files.Length);

        if (files.Length == 0)
        {
            throw new Errors.DataError("no races defined!", p.get());
        }

        foreach (string s in files)
        {
            new Race(s, new Json(p.gets(s)), new Json(pt.gets(s)), all);
        }

        int pl = 0;
        foreach (Race r in all)
        {
            if (r.playable)
                pl++;
        }
        this.map = new RMAPS<Race>("RACES", all);

        playable = new ArrayList<Race>(pl);
        foreach (Race r in all)
        {
            if (r.playable)
                playable.add(r);
        }

        boosts = new RaceBoosts();
    }

    public static void expand() throws IOException
    {
        ExpandInit init = new ExpandInit();

        foreach (Race r in i.all)
        {
            r.expand(init);
        }
        RacePreferrence.init();

        i.sprites = new RaceSprites();
        i.resources = new RaceResources(i.all);
    }

    public static RaceResources res()
    {
        return i.resources;
    }

    public static LIST<Race> all()
    {
        return i.all;
    }

    public static LIST<Race> playable()
    {
        return i.playable;
    }

    public static RMAPS<Race> map()
    {
        return i.map;
    }

    public static RaceServiceSorter SERVICE()
    {
        return i.service;
    }

    public static CharSequence name()
    {
        return ¤¤name;
    }

    public static RaceSprites sprites()
    {
        return i.sprites;
    }

    public static RaceBoosts boosts()
    {
        return i.boosts;
    }
}