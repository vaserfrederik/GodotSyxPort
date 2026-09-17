package settlement.room.main.util;

import java.nio.file.Path;

import init.paths.PATH;
import init.paths.PATHS;
import settlement.room.main.ROOMS;
import snake2d.util.file.Json;

public class RoomInitData{
	
	private PATH gData = PATHS.INIT().getFolder("room");
	private PATH gText = PATHS.TEXT().getFolder("room");
	public final PATH gSprite = PATHS.SPRITE().getFolder("room");
	private Json data;
	private Json text;
	private String key;
	private String type;
	
	public final ROOMS m;

	public RoomInitData(ROOMS m){
		this.m = m;
	}
	
	
	public String key() {
		return key;
	}
	
	public Json data() {
		return data;
	}
	
	public Json text() {
		return text;
	}
	
	public Path sp() {
		return gSprite.get(key);
	}
	
	public Path sp(String type, String key) {
		return gSprite.get(key);
	}
	
	public String type() {
		return type;
	}

	public RoomInitData init(String key) {
		data = new Json(gData.gets(key));
		text = new Json(gText.gets(key));
		this.key = key;
		return this;
		
	}
	
	public PATH getter() {
		return gData;
	}
	
	public RoomInitData setType(String type) {
		this.type = type;
		return this;
	}
	
	
}
