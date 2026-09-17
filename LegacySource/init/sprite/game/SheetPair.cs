using System;

namespace Init.Sprite.Game
{
    public class SheetPair
    {
        public Sheet S;
        public SheetData D;

        public SheetPair(Sheet sheet, SheetData data)
        {
            this.S = sheet;
            this.D = data;
        }

        public SheetPair()
        {
        }
    }
}