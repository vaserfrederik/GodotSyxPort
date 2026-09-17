package settlement.room.home.house;

import settlement.room.main.furnisher.FurnisherItem;
import snake2d.util.datatypes.DIR;

class SpriteCarpet {

	private final int[][][][] data = new int[4][4][][];
	
	SpriteCarpet() {
		
		data[0] = make(
			new int[][] {
				{0x00,0x00,0x00,},
				{0x00,0x10,0x10,},
				{0x00,0x10,0x10,},
			}
		);
		
		data[1] = make(
				new int[][] {
					{0x00,0x00,0x00,},
					{0x00,0x10,0x10,},
					{0x00,0x10,0x10,},
					{0x00,0x10,0x10,},
					{0x00,0x00,0x00,},
				}
			);
		
		data[2] = make(
			new int[][] {
				{0x00,0x00,0x00,0x00,0x00},
				{0x00,0x10,0x10,0x10,0x10},
				{0x00,0x10,0x10,0x10,0x10},
				{0x00,0x10,0x10,0x10,0x10},
				{0x00,0x10,0x10,0x10,0x10},
				{0x00,0x00,0x00,0x00,0x00},
			}
		);
		
		
	}
	
	private int[][][] make(int[][] o){
		int[][][] r = new int[4][][];
		
		for (int i = 0; i < 4; i++) {
			r[i] = o;
			o = rotate(o);
		}
		
		
		for (int[][] is : r) {
			int oi = 0;
			int co = 0;
			for (int y = 0; y < is.length; y++) {
				for (int x = 0; x < is[y].length; x++) {
					if (is[y][x] == 0)
						continue;
					else if (is[y][x] != oi) {
						co = 0;
						oi = is[y][x];
						is[y][x] |= co;
						co++;
					}
				}
			}
			
			
		}
		
		return r;
	}
	
	private int[][] rotate(int[][] l) {
		final int M = l.length;
		final int N = l[0].length;
		int[][] ret = new int[N][M];
		for (int r = 0; r < M; r++) {
			for (int c = 0; c < N; c++) {
				ret[c][M - 1 - r] = l[r][c];
			}
		}
		return ret;
	}
	
	public int get(int rx, int ry, FurnisherItem it) {
		int[][] m = data[it.group().index()][it.rotation&1];
		if (ry < 0 || ry >= m.length)
			return 0;
		if (rx < 0 || rx >= m[0].length)
			return 0;
		return (m[ry][rx] >> 4)&0x0F;
	}
	
	public int get(int rx, int ry, DIR d, FurnisherItem it) {
		return get(rx+d.x(), ry+d.y(), it);
	}
}
