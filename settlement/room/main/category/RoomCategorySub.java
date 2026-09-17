package settlement.room.main.category;

import init.sprite.UI.Icon;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.category.RoomCategories.RoomCategoryMain;
import snake2d.util.color.COLOR;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;

public final class RoomCategorySub {
	
	private final ArrayListGrower<RoomBlueprintImp> all = new ArrayListGrower<RoomBlueprintImp>();
	public final COLOR color;
	private final CharSequence name;
	private final Icon icon;
	RoomCategoryMain main;
	
	RoomCategorySub(ArrayListGrower<RoomCategorySub> all, CharSequence name, Icon icon, COLOR color) {
		this.name = name;
		this.icon = icon;
		
		this.color = color;
		all.add(this);
	}
	
	public int add(RoomBlueprintImp imp) {
		return all.add(imp);
	}
	
	public CharSequence name() {
		return name;
	}
	
	public Icon icon(){
		return icon;
	}
	
	public LIST<RoomBlueprintImp> rooms(){
		return all;
	}
	
	public RoomCategoryMain main() {
		return main;
	}
	
}
