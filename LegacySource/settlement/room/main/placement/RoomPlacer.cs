using System;
using System.Collections.Generic;
using static settlement.main.SETT;
using static settlement.room.main.construction.ConstructionData;
using init.sprite;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.data;
using util.rendering;
using view.tool;

public sealed class RoomPlacer
{
    public readonly PlacerArea PlacerArea;
    public readonly PlacerItemArea PlacerAreaItem;
    public readonly PlacerItemSingle PlacerItemSingle;
    public readonly Instance Instance;
    public readonly UtilStats Resources;
    public readonly UtilHistory History;
    public readonly UtilExtraCost Cost;
    public readonly UtilPlacability Placability;
    public readonly PlacerDoor Door;
    public readonly PlacableMulti PlacerDoor;
    public readonly UtilStructure Structure;

    public readonly BOOLEANImp BuildOnWalls = new BOOLEANImp(true);
    public readonly BOOLEANImp ShowOverlay = new BOOLEANImp(true)
    {
        public override BOOLEAN_MUTABLE Set(bool b)
        {
            if (b)
                ShowFoundation.Set(false);
            return base.Set(b);
        }
    };
    public readonly BOOLEANImp ShowFoundation = new BOOLEANImp(false)
    {
        public override BOOLEAN_MUTABLE Set(bool b)
        {
            if (b)
                ShowOverlay.Set(false);
            return base.Set(b);
        }
    };
    public readonly AutoWalls AutoWalls = new AutoWalls();

    private readonly PLACEMENT P;

    private RoomState State;
    private int OldDegrade = 0;
    private bool RenderExpense;

    public RoomPlacer(PLACEMENT p, Instance ins)
    {
        this.P = p;
        Instance = ins;
        PlacerArea = new PlacerArea(this);
        PlacerAreaItem = new PlacerItemArea(this);
        PlacerItemSingle = new PlacerItemSingle(this);
        Door = new PlacerDoor(this);
        PlacerDoor = Door.Placer;

        new ON_TOP_RENDERABLE()
        {
            public void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
            {
                if (Blueprint() == null || Blueprint().Constructor() == null)
                    return;
                RenderData.RenderIterator it = data.OnScreenTiles();
                while (it.Has())
                {
                    Room room = ROOMS().Map.Get(it.Tx(), it.Ty());
                    if (room == Instance)
                        RenderPlaceholder(r, shadowBatch, it);
                    it.Next();
                }
            }
        }.Add();
    }

    private Coo RCoo = new Coo();

    private void RenderPlaceholder(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
    {
        FurnisherItemTile it = ROOMS().FData.Tile.Get(i.Tile());

        if (Blueprint().Constructor().UsesArea() && Blueprint().Constructor().MustBeIndoors())
        {
            if (dExpensive.Is(i.Tile(), 1))
                GCOLOR.MAP().SOSO.Bind();
            else if (RenderExpense)
            {
                double am = Cost.Total();
                am *= 1.0 + Cost.Total();
                if (am < 0)
                    am = 0;
                am = Math.Ceiling(am);
            }
            else
                GCOLOR.MAP().SOSO.Bind();
        }
        else
            GCOLOR.MAP().SOSO.Bind();
    }

    private void SetState(RoomState state)
    {
        this.State = state;
    }

    public void RenderExpense()
    {
        RenderExpense = true;
    }

    public PLACABLE Area()
    {
        return PlacerArea;
    }

    public int Size()
    {
        return Instance.Area();
    }

    public PlacableFixed Item(int itemGroup)
    {
        if (!Blueprint().Constructor().UsesArea())
        {
            PlacerItemSingle.Set(Blueprint(), itemGroup, Instance.Upgrade());
            return PlacerItemSingle;
        }
        PlacerAreaItem.Set(Blueprint(), itemGroup, Instance.Upgrade());
        return PlacerAreaItem;
    }

    public PLACABLE ItemPlacerCurrent()
    {
        if (Blueprint() == null)
            return null;
        if (!Blueprint().Constructor().UsesArea())
        {
            return PlacerItemSingle;
        }
        return PlacerAreaItem;
    }

    public COORDINATE Create()
    {
        if (CreateProblem() != null)
        {
            throw new Exception("" + CreateProblem());
        }
        TBuilding structure = null;
        if (Blueprint().Constructor().MustBeIndoors())
        {
            structure = this.Structure.Get();
        }

        if (Blueprint().Constructor().MustBeIndoors())
        {
            if (AutoWalls.Is())
            {
                Door.Build(structure);
            }
            else
            {

            }
        }

        ConstructionInit init = new ConstructionInit(Instance.Upgrade(), Instance.Constructor(), structure, OldDegrade, State);
        Coo.TMP.Set(Instance.MX(), Instance.MY());
        TmpArea tmp = SETT.ROOMS().TmpArea(this);
        tmp.Set(Instance, Instance.MX(), Instance.MY());
        SETT.ROOMS().Construction.CreateWithConstructionData(tmp, init);
        Instance.ClearRegardless();
        return Coo.TMP;
    }

    public CharSequence CreateProblem()
    {
        if (Instance.Blue == null)
            return PLACABLE.E;
        return Placability.CreateProblem(Instance);
    }

    public CharSequence CreateWarning()
    {
        if (Instance.Blue == null)
            return null;
        return Instance.Blue.Constructor().Warning(Instance);
    }

    public FurnisherItemGroup CreateProblemItem()
    {
        return Placability.CreateProblemGroup();
    }

    public bool CreateProblemWalls()
    {
        if (AutoWalls.Is())
            return Door.CreateProblem() != null;
        return false;
    }

    public int ResNeeded(int rI)
    {
        if (Instance.Blue == null)
            return 0;
        double am = Resources.Needed(rI) - Resources.Allocated(rI);
        am *= 1.0 + Cost.Total();
        if (am < 0)
            return 0;
        return (int)Math.Ceiling(am);
    }

    public int ResNeededNoCost(int rI)
    {
        if (Instance.Blue == null)
            return 0;
        double am = Resources.Needed(rI) - Resources.Allocated(rI);
        return (int)Math.Ceiling(am);
    }

    public int ResNeededOnlyCost(int rI)
    {
        return ResNeeded(rI) - ResNeededNoCost(rI);
    }

    public int Unroofed()
    {
        if (Instance.Blue == null)
            return 0;
        return Instance.Unroofed;
    }

    public int Walls()
    {
        if (Instance.Blue == null)
            return 0;
        if (AutoWalls.Is())
            return Resources.Walls;
        return 0;
    }

    public double ItemStats(int si)
    {
        if (Instance.Blue == null)
            return 0;
        return Resources.Stat(si);
    }

    public PlacableFixed CreateItemPlacer(RoomBlueprintImp b, int group)
    {
        PlacerItemSingle it = new PlacerItemSingle(this);
        it.Set(b, 0, group);
        return it;
    }

    public bool HasHistory()
    {
        if (Instance.Blue == null)
            return false;
        if (History.HasHistory())
            return true;
        return false;
    }

    public bool PopHistory()
    {
        if (Instance.Blue == null)
            return false;
        if (History.HasHistory())
        {
            History.PopHistory();
            return true;
        }
        return false;
    }

    public bool RemoveAllItems()
    {
        if (Instance.Blue == null)
            return false;
        if (Resources.Items > 0)
        {
            foreach (COORDINATE c in Instance.Body())
            {
                if (Instance.Is(c))
                    PlacerAreaItem.RemoveItem(c.X, c.Y);
            }
            return true;
        }
        return false;
    }

    public bool RemoveArea()
    {
        if (Instance.Blue == null)
            return false;
        if (Instance.Area() > 0)
        {
            RoomBlueprintImp b = Blueprint();
            Instance.Clear(Blueprint());
            Resources.Clear();
            History.Clear();
            Init(b, Instance.Upgrade());
            return true;
        }
        return false;
    }

    public double Isolation()
    {
        if (Instance.Blue == null)
            return 0;
        if (!Blueprint().Constructor().UsesArea())
        {
            AREA a = PlacerItemSingle.ItemAreaCurrent;
            if (a == null)
                return 0;
            return Door.Isolation(Instance.Blue, a, AutoWalls.Is());
        }
        else
            return Door.Isolation(Instance.Blue, Instance, AutoWalls.Is());
    }

    public UtilExtraCost Cost()
    {
        return Cost;
    }

    void Update(double ds)
    {
        RenderExpense = false;
    }
}