using System;
using System.IO;

using Init;

namespace Init.Type
{
    public sealed class TypeInit : InitResource
    {
        public TypeInit(INIT init) : base(init)
        {
            new HClasses();
            new CauseLeaves();
            new CauseArrives();
            new HTypes(null);
            new Climates();
            new Terrains();
            new Traits();
            new BuildingPrefs();
            WGroup.Init();
            HGroup.Init();
            HClassRace.Init(null, null);
            HTypeRace.Init(null, null);
            new Needs();
            new Diseases();
            new Crimes(null);
            new CrimePunishments(null);
        }
    }
}