using System;
using settlement.entity.humanoid;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;
using util.gui.misc;

namespace settlement.entity.humanoid.ai.main
{
    public interface HAI
    {
        public RESOURCE resourceCarried();
        public int resourceA();
        public void getOccupation(Humanoid a, Str string);
        public COORDINATE getDestination();
        public void hoverInfoSet(Humanoid a, GBox text);
    }
}