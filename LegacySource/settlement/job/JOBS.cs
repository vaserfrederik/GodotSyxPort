using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.data;
using util.gui.misc;
using util.keymap;
using util.rendering;
using view.tool;

public sealed class JOBS : SettResource
{
    private readonly Bitsmap2D map = new Bitsmap2D(0, 12, SETT.TWIDTH, SETT.THEIGHT);
    private readonly Bitsmap1D statei = new Bitsmap1D(-1, 2, SETT.TAREA);

    public Bitsmap2D paintmap = new Bitsmap2D(0, 3, SETT.TWIDTH, SETT.THEIGHT);

    public readonly BOOLEAN_MUTABLE planMode = new BOOLEAN_MUTABLE
    {
        private bool i = false;

        public override bool Is() => i;

        public override BOOLEAN_MUTABLE Set(bool b)
        {
            i = b;
            return this;
        }
    };
    private int hoverI = -1;

    readonly Bitsmap1D progress = new Bitsmap1D(0, 3, SETT.TAREA);
    readonly Bitmap1D wantsRes = new Bitmap1D(SETT.TAREA, false);
    readonly StateManager state = new StateManager(statei);

    public readonly MAP_OBJECT<Job> getter;
    // public readonly MAP_INT in2dexMap = new MAP_INT()
    // {
    //     public override int Get(int tx, int ty)
    //     {
    //         if (IN_BOUNDS(tx, ty))
    //             return Get(tx + ty * TWIDTH);
    //         return Job.NOTHING;
    //     }

    //     public override int Get(int tile)
    //     {
    //         return map[tile];
    //     }
    // };
    public readonly PlacableMulti tool_clear = new PlacerDelete();
    public readonly PlacableMulti tool_activate = new PlacerActivate();
    public readonly PlacableMulti tool_dormant = new PlacerDormant();
    public readonly PlacableMulti tool_remove_all = new PlacerRemoveAll();
    public readonly PlacableMulti tool_remove_smartl = new PlacerRemoveSmart();
    public readonly MAP_OBJECT<Job> jobGetter = new JobGetter();
    readonly JobRoom room = new JobRoom(null);
    readonly JobRoom[] rooms = new JobRoom[RESOURCES.ALL().Size()];
    {
        for (int i = 0; i < rooms.Length; i++)
            rooms[i] = new JobRoom(RESOURCES.ALL().Get(i));
    }

    public readonly PlacableMulti tool_repair = new PlacerRepair();

    public readonly JobBuildRoads roads = new JobBuildRoads();
    public readonly LIST<JobBuildStructure> build_structure = JobBuildStructure.Make();
    public readonly JobBuildForts build_fort = new JobBuildForts();
    public readonly LIST<Job> fences = JobBuildFence.Make();
    public readonly JobClears clearss = new JobClears();
    public readonly LIST<PLACABLE> clears = new ArrayList<PLACABLE>(clearss.placers);
    public readonly BlockedJobs blocked = new BlockedJobs();

    public JOBS() : base("JOBS", true)
    {
        new Debug();

        getter = new MAP_OBJECT<Job>()
        {
            public override Job Get(int tx, int ty)
            {
                if (!IN_BOUNDS(tx, ty))
                    return null;

                int i = map.Get(tx, ty);
                if (i != Job.NOTHING)
                {
                    Job j = Job.all.Get(i - 1);
                    if (!j.Get(tx, ty))
                    {
                        PlacerDelete.Place(tx, ty);
                        return null;
                    }
                    return j;
                }

                return null;
            }

            public override Job Get(int tile)
            {
                return Get(tile % TWIDTH, tile / TWIDTH);
            }

            public override bool Is(int tile)
            {
                int i = map.Get(tile);
                return i != Job.NOTHING;
            }

            public override bool Is(int tx, int ty)
            {
                if (!IN_BOUNDS(tx, ty))
                    return false;
                return Is(tx + ty * TWIDTH);
            }
        };

        new ON_TOP_RENDERABLE()
        {
            public override void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
            {
                RenderData.RenderIterator i = data.OnScreenTiles();
                COLOR_MAP c = GCOLOR.MAP();
                while (i.Has())
                {
                    int index = map.Get(i.Tile());

                    if (index != Job.NOTHING)
                    {
                        index -= 1;

                        if (i.Tile() == hoverI)
                        {
                            COLOR.WHITE2WHITE.Bind();
                        }
                        else
                        {
                            switch (state.Get(i.Tile()))
                            {
                                case State.DORMANT:
                                    c.DORMANT.Bind();
                                    break;
                                case State.RESERVABLE:
                                    c.JOB_ACTIVE.Bind();
                                    break;
                                case State.RESERVED:
                                    c.JOB_RESERVED.Bind();
                                    break;
                                case State.BLOCKED:
                                    c.JOB_BLOCKED.Bind();
                                    break;
                            }
                        }
                        Job j = Job.all.Get(index);

                        if (j != null)
                        {
                            j.RenderAbove(r, i.X(), i.Y(), 0, i.Tx(), i.Ty());

                            if (CORE.Renderer().GetZoomout() <= 1)
                            {
                                j.Get(i.Tx(), i.Ty());
                                RESOURCE res = j.ResourceCurrentlyNeeded();
                                if ((j == clearss.food && !SETT.WEATHER().growthRipe.CropsAreRipe()) || (res != null && !j.JobReservedIs(res) && !PATH().Finders.Resource.Normal.Has(i.Tx(), i.Ty(), res)))
                                {
                                    COLOR.WHITE702WHITE100.Bind();
                                    SPRITES.cons().ICO.warning.Render(r, i.X(), i.Y());
                                    COLOR.Unbind();
                                }
                            }
                        }
                    }
                    else if (SETT.TERRAIN().Get(i.Tile()) is TGrowable && SETT.TERRAIN().GROWABLES.Get(0).job.Is(i.Tile()))
                    {
                        if (i.Tile() == hoverI)
                        {
                            COLOR.WHITE2WHITE.Bind();
                        }
                        else
                        {
                            c.DORMANT.Bind();
                        }
                        SPRITES.cons().BIG.dashed_hollow.Render(r, 0, i.X(), i.Y());
                    }
                    i.Next();
                }
                COLOR.Unbind();
            }
        }.Add();
        Clear();

        KeyMap<Job> map = new KeyMap<Job>();
        foreach (Job j in Job.all)
            map.Put(j.key(), j);
    }

    public void Render(Renderer r, ShadowBatch shadowBatch, RenderData data)
    {
        RenderData.RenderIterator i = data.OnScreenTiles();

        while (i.Has())
        {
            int index = map.Get(i.Tile());
            if (index != Job.NOTHING)
            {
                Job.all.Get(index - 1).RenderBelow(r, shadowBatch, i, progress.Get(i.Tile()));
            }
            i.Next();
        }
    }

    void Set(Job job, int tx, int ty)
    {
        map.Set(tx, ty, job != null ? job.Id + 1 : Job.NOTHING);
        if (job != null)
        {
            state.Set(tx, ty, (int)State.RESERVABLE);
            progress.Set(tx, ty, 0);
            wantsRes.Set(tx, ty, false);
        }
        else
        {
            state.Set(tx, ty, 0);
            progress.Set(tx, ty, 0);
            wantsRes.Set(tx, ty, false);
        }
    }

    public override void Clear()
    {
        map.Clear();
        statei.Clear();
        progress.Clear();
        wantsRes.Clear();
        blocked.Clear();
    }

    public override void Init(bool loaded)
    {
        clearss.InitSpeeds();
        if (loaded)
        {
            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                getter.Get(c);
            }
        }
    }

    public override void Update(double ds, Profiler profiler)
    {
        hoverI = -1;
        blocked.Update(ds);
    }

    public void Hover(int tx, int ty, GBox box)
    {
        hoverI = tx + ty * SETT.TWIDTH;
        if (getter.Is(tx, ty))
        {
            Job j = getter.Get(tx, ty);
            hoverI = j.tile;
            j.Hover(box);

            box.NL();
            box.Add(box.text().add(state.GetDepth(tx, ty)));
        }
        else if (SETT.TERRAIN().Get(tx, ty) is TGrowable && SETT.TERRAIN().GROWABLES.Get(0).job.Is(tx, ty))
        {
            clearss.HoverEdible(box, tx, ty);
        }
    }

    public readonly MAP_SETTER clearer = new MAP_SETTER()
    {
        public override MAP_SETTER Set(int tx, int ty)
        {
            PlacerDelete.Place(tx, ty);
            return this;
        }

        public override MAP_SETTER Set(int tile)
        {
            throw new RuntimeException();
        }
    };
}