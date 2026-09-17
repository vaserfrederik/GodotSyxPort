using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using util.text;

namespace view.tool
{
    public class PlacableMessages
    {
        private PlacableMessages()
        {
        }

        public static string ¤¤SOLID_BLOCK = "¤Must be placed on non-solid tiles.";
        public static string ¤¤ROOM_BLOCK = "¤Must not be placed on room.";
        public static string ¤¤ROOM_MUST = "¤Must be placed on room.";
        public static string ¤¤MISC = "¤Blocked by something.";
        public static string ¤¤STRUCTURE_BLOCK = "¤Blocked by structure. Dismantle it first.";
        public static string ¤¤ROCK_MUST = "¤Must be placed on rock.";
        public static string ¤¤TREE_MUST = "¤Must be placed on tree.";
        public static string ¤¤WATER_MUST = "¤Must be placed on water.";
        public static string ¤¤WATER_RETURN = "¤Water can only be dug where there is ground water.";
        public static string ¤¤MOUNTAIN_MUST = "¤Must be placed on solid mountain.";
        public static string ¤¤MOUNTAIN_NOT = "¤Can't be placed on mountains or caves. Use mountain specific tools for this.";
        public static string ¤¤STRUCTURE_CLEAR = "¤Must be placed on structures. Walls, ceilings, fortifications or roads.";
        public static string ¤¤ROAD_ALREADY = "¤Road already exists there.";
        public static string ¤¤JOB_BLOCK = "¤Blocked by other job.";
        public static string ¤¤JOB_MUST = "¤Must be placed on jobs.";
        public static string ¤¤BROKEN_MUST = "¤Must be placed on broken walls or rooms.";
        public static string ¤¤CAVE_MUST = "¤Must be placed on a cave.";

        public static string ¤¤MAX_SIZE_REACHED = "¤Max size reached!";
        public static string ¤¤MAX_DIMENSION_REACHED = "¤Max dimension reached!";
        public static string ¤¤TERRAIN_BLOCK = "¤Blocked by terrain!";
        public static string ¤¤NOT_EDIBLE = "¤Must be placed on edible vegetation!";
        public static string ¤¤NOT_RIPE = "¤Edible vegetation must be ripe (Late Summer)!";
        public static string ¤¤IN_MAP = "¤Must be placed within map!";
        public static string ¤¤ONE_CLEAR_TILE = "¤Needs at least 1 unblocked tile.";
        public static string ¤¤SAME_REGION = "¤Needs to be in the same region.";
        public static string ¤¤REGION = "¤Must be placed in a region!";
        public static string ¤¤BLOCKED = "¤Blocked!";
        public static string ¤¤BLOCK_WILL = "¤Will block other tile!";
        public static string ¤¤BLOCKED_WILL = "¤Will be blocked by other tile!";
        public static string ¤¤ROOM_OR_STRUCTURE_MUST = "¤Must be placed on rooms or structures!";
        public static string ¤¤ITEM_MUST = "¤Must be placed on items!";

        static PlacableMessages()
        {
            D.ts(typeof(PlacableMessages));
        }
    }
}