using System;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using init.paths;
using snake2d.util.color;
using snake2d.util.file;

public static class COLOR_MAP
{
    private static Json d = new Json(PATHS.SPRITE_UI().getLikeHells("Colors.txt")).json("MAP");

    public static readonly COLOR DORMANT = new ColorImp(d, "DORMANT");
    public static readonly COLOR BAD = new ColorImp(d, "BAD");
    public static readonly COLOR SOSO = new ColorImp(d, "SOSO");
    public static readonly COLOR OK = new ColorImp(d, "OK");
    public static readonly COLOR BETTER = new ColorImp(d, "BETTER");

    public static readonly COLOR OK_2_BETTER = new ColorShifting(OK, BETTER);

    public static readonly COLOR BEST = new ColorImp(d, "BEST");
    public static readonly COLOR BEST_DARK = BEST.shade(0.75);

    public static readonly COLOR JOB_DORMANT = DORMANT;
    public static readonly COLOR JOB_ACTIVE = OK;
    public static readonly COLOR JOB_RESERVED = BETTER;
    public static readonly COLOR JOB_BLOCKED = JOB_ACTIVE.shade(0.75);

    public static readonly COLOR BATTLE_DORMANT = DORMANT;
    public static readonly COLOR BATTLE_OK = OK;

    public static readonly COLOR OVERLAY_GOOD = new ColorImp(d, "OVERLAY_GOOD");
    public static readonly COLOR OVERLAY_BAD = new ColorImp(d, "OVERLAY_BAD");

    public static readonly COLOR F_PLAYER = new ColorImp(d, "F_PLAYER");
    public static readonly COLOR F_ALLY = new ColorImp(d, "F_ALLY");
    public static readonly COLOR F_NEAUTRAL = new ColorImp(d, "F_NEAUTRAL");
    public static readonly COLOR F_ENEMY = new ColorImp(d, "F_ENEMY");
    public static readonly COLOR F_REBEL = new ColorImp(d, "F_REBEL");

    public static COLOR get(Faction f)
    {
        if (f == null)
            return F_REBEL;
        if (f == FACTIONS.player())
            return F_PLAYER;
        if (DIP.get((FactionNPC)f).ally)
            return F_ALLY;
        if (!DIP.WAR().is(FACTIONS.player(), f))
            return F_NEAUTRAL;
        else
            return F_ENEMY;
    }
}