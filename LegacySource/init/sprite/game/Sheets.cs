using System;
using System.Collections.Generic;
using System.IO;
using init.sprite;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.sprite.game
{
    public class Sheets
    {
        public readonly LIST<SheetPair> sheets;

        public Sheets(SheetType type, Json json) 
        {
            sheets = SPRITES.GAME().sheets(type, json);
        }

        public Sheets(Sheet s, SheetData d) 
        {
            sheets = new ArrayList<SheetPair>(new SheetPair(s, d));
        }

        public SheetPair get(int random) 
        {
            if (sheets.size() == 0)
                return null;
            return sheets.getC(random);
        }
    }
}