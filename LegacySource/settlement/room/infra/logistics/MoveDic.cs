using System;
using util.text;

namespace settlement.room.infra.logistics
{
    public class MoveDic
    {
        public static string ¤¤capacityD = "¤Used Capacity";
        public static string ¤¤allocatedCrates = "¤Allocated Crates/Total crates";
        public static string ¤¤storing = "¤Storing";
        public static string ¤¤storingD = "¤When storing, this warehouse is closed to outsiders, and no one will use it as a source of goods. Does not affect pull/fetch orders from other storage rooms.";
        public static string ¤¤keepD = "¤Other storage rooms will not be able to pull from this warehouse if the stored goods are below this limit.";
        public static string ¤¤fetch = "¤Fetch";
        public static string ¤¤fetchProblem = "¤There is nothing to fetch in the vicinity!";
        public static string ¤¤pullProblem = "¤Nothing can be pulled or fetched!";
        public static string ¤¤fetchD = "¤When enabled, the workers will fetch all odd resources from the ground and from production rooms within their radius.";
        public static string ¤¤fetching = "¤Fetching";
        
        public static string ¤¤crates = "Crates";
        public static string ¤¤capacity = "Capacity";
        public static string ¤¤capacityRes = "Capacity Reserved";
        public static string ¤¤Stored = "Stored";
        public static string ¤¤StoredD = "How much is stored in your warehouses.";
        public static string ¤¤StoredRes = "Stored Reserved";
        
        public static string ¤¤prio = "¤Prioritize";
        public static string ¤¤prioD = "¤Prioritized fetching allows workers to fetch from all other storage rooms that are not prioritized within their radius.";
        
        static MoveDic()
        {
            D.ts(typeof(MoveDic));
        }
    }
}