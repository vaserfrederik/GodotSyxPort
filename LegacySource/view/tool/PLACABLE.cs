using System;
using System.Collections.Generic;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;

public interface PLACABLE
{
    static readonly string E = "";

    /**
     * 
     * @return get an small image representing this tile
     */
    SPRITE GetIcon();

    /**
     * 
     * @return the name of the tile
     */
    string Name();

    PLACABLE GetUndo();
    default LIST<CLICKABLE> GetAdditionalButt()
    {
        return null;
    }

    default void HoverDesc(GBox box)
    {
        box.Text(Name());
    }
}