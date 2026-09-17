package settlement.room.main.furnisher;

import static settlement.main.SETT.ROOMS;

import settlement.room.main.Room;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.file.Json;

public class FurnisherMinimapColor {

	private final float[][] bs;
	private final COLOR color;
	private final ColorImp imp = new ColorImp();
	
	public FurnisherMinimapColor(Json json) {
		if (!json.has("MINI_COLOR"))
			color = COLOR.WHITE50;
		else
			color = new ColorImp(json, "MINI_COLOR");
		
		if (!json.has("MINI_COLOR_PATTERN")) {
			bs = new float[][] {
				{1.0f},
			};
		}else {
			
			String[] ss = json.texts("MINI_COLOR_PATTERN", 1, 32);
			final int l = ss[0].length();
			bs = new float[ss.length][l];
			
			for (int si = 0; si < ss.length; si++) {
				String s = ss[si];
				if (l != s.length())
					json.error("the pattern must have the same length of all its strings!", "MINI_COLOR_PATTERN");
				for (int i = 0; i < l; i++) {
					float v = 1.0f;
					int c = s.charAt(i)- '0';
					if (c >= 0 && c <= 9) {
						v = (float) (0.5 + 0.5*c/9.0);
					}
					bs[si][i] = v;
				}
			}
			
			
		}
		
	}
	
	public COLOR get(int tx, int ty) {
		Room r = ROOMS().map.get(tx, ty);
		
		int x1 = r.x1(tx, ty);
		int y1 = r.y1(tx, ty);

		imp.set(color);
		imp.shadeSelf(bs[(ty-y1)%bs.length][(tx-x1)%bs[0].length]);
		return imp;
	}
}
