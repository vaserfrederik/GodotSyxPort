using System;
using System.Collections.Generic;
using System.Linq;
using settlement.main;
using init.resources;
using snake2d.util.datatypes;
using util.data;
using view.sett;
using view.tool;

namespace settlement.job
{
    class Debug
    {
        static bool showRoom = false;

        public Debug()
        {
            PLACABLE p;

            p = new PlacableMulti("reservePerform")
            {
                place = (tx, ty, a, t) =>
                {
                    Job j = JOBS().getter.get(tx, ty);
                    if (j == null)
                        return;

                    RBIT bb = j.jobResourceBitToFetch();
                    RESOURCE res = null;
                    if (bb != null)
                    {
                        foreach (RESOURCE r in RESOURCES.ALL())
                        {
                            if (bb.has(r))
                            {
                                res = r;
                                break;
                            }
                        }
                    }

                    if (j.jobReserveCanBe())
                    {
                        j.jobReserve(res);
                    }
                    else if (j.jobReservedIs(res))
                    {
                        j.jobPerform(null, res, 1);
                    }
                },

                isPlacable = (tx, ty, a, t) =>
                {
                    Job j = JOBS().getter.get(tx, ty);
                    if (j != null)
                    {
                        RBIT bb = j.jobResourceBitToFetch();
                        RESOURCE res = null;
                        if (bb != null)
                        {
                            foreach (RESOURCE r in RESOURCES.ALL())
                            {
                                if (bb.has(r))
                                {
                                    res = r;
                                    break;
                                }
                            }
                        }

                        if (j.jobReserveCanBe() || j.jobReservedIs(res))
                        {
                            return null;
                        }
                    }
                    return "";
                }
            };

            IDebugPanelSett.add("job", p);

            BOOLEAN_MUTABLE roomJobs = new BOOLEAN_MUTABLE
            {
                is = () => showRoom,
                set = (bool b) =>
                {
                    showRoom = b;
                    return this;
                }
            };

            IDebugPanelSett.add("Show roomjobs", roomJobs);
        }
    }
}