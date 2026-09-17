using System;
using settlement.job;
using static settlement.main.SETT;
using static settlement.main.SETT.JOBS;
using static settlement.main.SETT.ROOMS;
using static settlement.main.SETT.TERRAIN;
using static settlement.main.SETT.THINGS;
using static settlement.main.SETT.TWIDTH;
using game.GAME;
using game.audio.AUDIO;
using game.audio.SoundRace;
using game.faction.FResources.RTYPE;
using init.resources.RESOURCE;
using init.sprite.SPRITES;
using settlement.entity.humanoid.Humanoid;
using settlement.job.StateManager.State;
using settlement.main.SETT;
using settlement.room.main.Room;
using settlement.thing.ThingsResources.ScatteredResource;
using settlement.tilemap.terrain.Terrain.TerrainTile;
using snake2d.Renderer;
using snake2d.SPRITE_RENDERER;
using snake2d.util.datatypes.DIR;
using util.rendering.RenderData.RenderIterator;
using util.rendering.ShadowBatch;
using util.text.D;
using view.tool.PlacableMulti;

class JobRoom : Job {

    private static CharSequence ¤¤clearTerrain = "Clearing Terrain";
    private static CharSequence ¤¤clearVegetation = "Clearing Terrain";
    private static CharSequence ¤¤getting = "Getting Materials";
    private static CharSequence ¤¤constructing = "Constructing";
    private static CharSequence ¤¤removing = "Removing Obstacle";
    static {
        D.ts(typeof(JobRoom));
    }
    
    private enum PSTATE {
        CLEAR_TERRAIN(¤¤clearTerrain), CLEAR_VEG(¤¤clearVegetation), REMOVING(¤¤removing), FETCHING(
                ¤¤getting), DOING(¤¤constructing);

        public readonly CharSequence Name;

        private PSTATE(CharSequence name) {
            this.Name = name;
        }
    }

    private PSTATE state;
    private ROOM_JOBBER r;
    private readonly RESOURCE res;
    private readonly SoundRace sound = AUDIO.race("BUILD");
    
    JobRoom(RESOURCE res) : base("ROOM_" + (res == null ? "NONE" : res.key), "work", SPRITES.icons().m.questionmark) {
        this.res = res;
    }

    override void init(int tx, int ty) {
        JOBS().progress.set(tx + ty * TWIDTH, 0);
        JOBS().wantsRes.set(tx + ty * TWIDTH, getState(tx, ty) == PSTATE.FETCHING);
    }

    override protected bool get(int tx, int ty) {
        state = getState(tx, ty);
        Room room = ROOMS().map.get(tx, ty);
        if (room == null || !(room is ROOM_JOBBER))
            return false;
        r = (ROOM_JOBBER) room;
        base.get(tx, ty);
        if (JOBS().wantsRes.get(tile))
            state = PSTATE.FETCHING;
        else if (state == PSTATE.FETCHING)
            state = PSTATE.CLEAR_VEG;
            
        return true;
    }
    
    private PSTATE getState(int tx, int ty) {
        if (((ROOM_JOBBER) ROOMS().map.get(tx, ty)).needsTerrainToBeCleared(tx, ty) && terrainNeedsClear(tx, ty))
            return PSTATE.CLEAR_TERRAIN;
        else if (((ROOM_JOBBER) ROOMS().map.get(tx, ty)).needsFertilityToBeCleared(tx, ty) && !GRASS().current.is(tx, ty, 0))
            return PSTATE.CLEAR_VEG;
        else if (res != null)
            return PSTATE.FETCHING;
        else if (((ROOM_JOBBER) ROOMS().map.get(tx, ty)).becomesSolid(tx, ty) && THINGS().resources.get(tx, ty) != null)
            return PSTATE.REMOVING;
        else
            return PSTATE.DOING;
    }

    bool terrainNeedsClear(int tx, int ty) {
        return TERRAIN().get(tx, ty).clearing().needs() && TERRAIN().get(tx, ty).clearing().can();
    }

    //    @Override
    //    public long jobResourceBitToFetch() {
    //        if (state == PSTATE.FETCHING && res != null)
    //            return res.bit;
    //        return 0;
    //    }

    override public RESOURCE resourceCurrentlyNeeded() {
        if (state == PSTATE.FETCHING)
            return res;
        return null;
    }

    override public void jobStartPerforming() {
        // TODO Auto-generated method stub

    }

    override public double jobPerformTime(Humanoid skill) {
        switch (state) {
            case PSTATE.CLEAR_TERRAIN:
                if (SETT.TERRAIN().MOUNTAIN.is(coo))
                    return JOBS().clearss.tunnel.jobPerformTime(skill);
                
                TerrainTile t = TERRAIN().get(coo);
                if (t.clearing().isEasilyCleared())
                    return 2;
                return 20; 
            case PSTATE.CLEAR_VEG:
                return 2.0;
            case PSTATE.REMOVING:
                return 0;
            case PSTATE.FETCHING:
                return 0;
            case PSTATE.DOING:
                return 10;
        }
        throw new RuntimeException();
    }
    
    override public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {

        if (!jobReservedIs(r)) {
            throw new RuntimeException(JOBS().state.is(coo, State.RESERVED) + " " + r + " " + resourceCurrentlyNeeded());
        }

        RESOURCE res = null;
        switch (state) {
            case PSTATE.CLEAR_TERRAIN:
                if (SETT.TERRAIN().MOUNTAIN.is(coo)) {
                    res = JOBS().clearss.tunnelPerform(coo);
                } else {
                    TerrainTile t = TERRAIN().get(tile);
                    res = t.clearing().clear1(coo.x(), coo.y());
                }
                
                break;
            case PSTATE.CLEAR_VEG:
                GRASS().current.increment(coo.x(), coo.y(), -4);
                break;
            case PSTATE.REMOVING:
                ScatteredResource ress = THINGS().resources.get(coo.x(), coo.y());
                if (ress == null)
                    break;
                if (ress.findableReservedCanBe()) {
                    ress.findableReserve();
                    ress.resourcePickup();
                } else {
                    ress.resourcePickup();
                }
                res = ress.resource();
                break;
            default:
                int tx = coo.x();
                int ty = coo.y();
                ROOM_JOBBER j = this.r;
                PlacerDelete.place(tx, ty);
                j.jobFinsih(tx, ty, r, ram);
                if (!SETT.JOBS().getter.is(tx, ty)) {
                    for (int di = 0; di < DIR.ORTHO.size(); di++) {
                        DIR d = DIR.ORTHO.get(di);
                        if (SETT.JOBS().getter.is(tx, ty, d))
                            SETT.JOBS().state.set(SETT.JOBS().state.get(tx, ty, d), SETT.JOBS().getter.get(tx, ty, d));
                    }
                }
                return null;
        }
        
        JOBS().wantsRes.set(tile, getState(coo.x(), coo.y()) == PSTATE.FETCHING);

        get(coo.x(), coo.y());
        jobReserveCancel(r);
        if (res != null)
            GAME.player().res().inc(res, RTYPE.PRODUCED, 1);
        return res;

    }

    override bool becomesSolidNext() {
        return ((ROOM_JOBBER) ROOMS().map.get(coo.x(), coo.y())).becomesSolid(coo.x(), coo.y());
    }
    
    override public bool becomesSolid() {
        return ((ROOM_JOBBER) ROOMS().map.get(coo.x(), coo.y())).becomesSolid(coo.x(), coo.y());
    }
    
    override public int jobResourcesNeeded(Humanoid skill) {
        return ((ROOM_JOBBER) ROOMS().map.get(coo.x(), coo.y())).totalResourcesNeeded(coo.x(), coo.y());
    }

    override public CharSequence jobName() {
        return state.Name;
    }

    override public bool jobUseTool() {
        return true;
    }

    
    
    override public SoundRace jobSound() {
        switch (state) {
            case PSTATE.CLEAR_TERRAIN:
                return TERRAIN().get(coo).clearing().sound(coo.x(), coo.y());
            case PSTATE.CLEAR_VEG:
                return GRASS().clearSound;
            case PSTATE.REMOVING:
                return null;
            case PSTATE.FETCHING:
                return null;
            case PSTATE.DOING:
                return sound;
        }
        throw new RuntimeException();
    }

    override protected void renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i, int state) {
        
    }

    override public PlacableMulti placer() {
        return null;
    }

    override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty) {
        if (DebugMode || DebugMode && DebugMode) {
            SPRITES.icons().m.repair.render(r, x, y);
        }
    }
    
    override public int resAmount() {
        return res != null ? 1 : 0;
    }

    override public RESOURCE res() {
        return res;
    }

    override public TerrainTile becomes(int tx, int ty) {
        return TERRAIN().NADA;
    }
}