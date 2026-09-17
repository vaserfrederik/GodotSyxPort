package settlement.room.infra.logistics;

import util.text.D;

public class MoveDic {

	public static CharSequence ¤¤capacityD = "¤Used Capacity";
	public static CharSequence ¤¤allocatedCrates = "¤Allocated Crates/Total crates";
	public static CharSequence ¤¤storing = "¤Storing";
	public static CharSequence ¤¤storingD = "¤When storing, this warehouse is closed to outsiders, and no one will use it as a source of goods. Does not affect pull/fetch orders from other storage rooms.";
	public static CharSequence ¤¤keepD = "¤Other storage rooms will not be able to pull from this warehouse if the stored goods are below this limit.";
	public static CharSequence ¤¤fetch = "¤Fetch";
	public static CharSequence ¤¤fetchProblem = "¤There is nothing to fetch in the vicinity!";
	public static CharSequence ¤¤pullProblem = "¤Nothing can be pulled or fetched!";
	public static CharSequence ¤¤fetchD = "¤When enabled, the workers will fetch all odd resources from the ground and from production rooms within their radius.";
	public static CharSequence ¤¤fetching = "¤Fetching";
	
	public static CharSequence ¤¤crates = "Crates";
	public static CharSequence ¤¤capacity = "Capacity";
	public static CharSequence ¤¤capacityRes = "Capacity Reserved";
	public static CharSequence ¤¤Stored = "Stored";
	public static CharSequence ¤¤StoredD = "How much is stored in your warehouses.";
	public static CharSequence ¤¤StoredRes = "Stored Reserved";
	
	public static CharSequence ¤¤prio = "¤Prioritize";
	public static CharSequence ¤¤prioD = "¤Prioritized fetching allows workers to fetch from all other storage rooms that are not prioritized within their radius.";
	
	static {
		D.ts(MoveDic.class);
	}
	
}
