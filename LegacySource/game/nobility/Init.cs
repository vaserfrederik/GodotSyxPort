using init.paths;
using snake2d.util.sets;
using System.Collections.Generic;

class Init
{
    readonly PATH pData = PATHS.INIT().getFolder("race").getFolder("nobility");
    readonly PATH pText = PATHS.TEXT().getFolder("race").getFolder("nobility");
    readonly List<Noble> all = new List<Noble>(100);
}