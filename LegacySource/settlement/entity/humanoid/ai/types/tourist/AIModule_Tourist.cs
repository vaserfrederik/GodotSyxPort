using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.room.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.infra.inn;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.types.tourist;
using settlement.tourism;
using init.type;
using init.sprite.UI;
using settlement.entity.humanoid.ai.main;
using snake2d.util.datatypes;
using snake2d.util.bit;
using util.text;

namespace settlement.entity.humanoid.ai.types.tourist
{
    public class AIModule_Tourist : AIModule
    {
        private static readonly int time = (int)(1 * TIME.hoursPerDay());
        static readonly int shasCheckedIn = 0b0000_0001;
        static readonly int shasCheckedOut = 0b0000_0010;
        static readonly int shasReview = 0b0000_0100;
        static readonly int shasSight = 0b0000_1000;
        static readonly int shasService = 0b0001_0000;

        private static readonly CharSequence ¤¤name = "¤Sight Seeing";
        private static readonly CharSequence ¤¤checkin = "¤checking in";
        private static readonly CharSequence ¤¤checkout = "¤checking out";
        private static readonly CharSequence ¤¤sight = "¤sightseeing";
        private static readonly CharSequence ¤¤leaving = "¤leaving";

        static AIModule_Tourist()
        {
            D.ts(AIModule_Tourist.class);
        }

        public AIModule_Tourist() : base(UI.icons().s.crossheir, ¤¤name, null) { }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (SETT.ENTRY().beseiged())
            {
                return leaving.activate(a, d);
            }

            if ((AIModules.data().byte1.get(d) & shasCheckedIn) == 0)
            {
                if (SETT.ROOMS().INN.service().finder.reserve(a.tc(), d.path, int.MaxValue))
                {
                    AIModules.data().coo(d).set(d.path.destX(), d.path.destY());
                    return checkIn.activate(a, d);
                }
            }

            if ((AIModules.data().byte1.get(d) & shasSight) == 0)
            {
                AIModules.data().byte1.set(d, AIModules.data().byte1.get(d) | shasSight);
                return see.activate(a, d);
            }

            if ((AIModules.data().byte1.get(d) & shasService) == 0)
            {
                AIModules.data().byte1.set(d, AIModules.data().byte1.get(d) | shasService);
                return TOURISM.servicePlan(a, d);
            }

            if (Bits.getDistance(AIModules.data().byte2.get(d), TIME.hours().bitsSinceStart(), 0x0FF) > time)
            {
                if ((AIModules.data().byte1.get(d) & shasCheckedOut) == 0)
                {
                    if (service(d) == null)
                    {
                        if (SETT.ROOMS().INN.service().finder.reserve(a.tc(), d.path, int.MaxValue))
                        {
                            AIModules.data().coo(d).set(d.path.destX(), d.path.destY());
                            return checkIn.activate(a, d);
                        }
                    }
                }
            }
            return null;
        }

        public override void update(Humanoid a, AIManager d, int dt)
        {
            // Implement update logic here
        }

        public override void render(Humanoid a, AIManager d, Renderer r)
        {
            // Implement rendering logic here
        }

        public override void cancel(Humanoid a, AIManager d)
        {
            if (service(d) != null)
            {
                service(d).consume();
            }
        }

        static void consume(AIManager d)
        {
            if (service(d) != null)
            {
                service(d).consume();
            }
            AIModules.data().coo(d).set(-1, -1);
        }

        static FSERVICE service(AIManager d)
        {
            FSERVICE s = SETT.ROOMS().INN.service().service(AIModules.data().x.get(d), AIModules.data().y.get(d));
            if (s != null && !s.findableReservedIs())
            {
                GAME.Notify(AIModules.data().coo(d));
            }
            if (s != null && s.findableReservedIs())
            {
                return s;
            }
            return null;
        }

        AIPlanActivation activate(Humanoid a, AIManager d)
        {
            if ((AIModules.data().byte1.get(d) & shasReview) == 0)
            {
                TOURISM.touristFinish(a.indu(), AIModules.data().coo(d));
            }
            AIModules.data().byte1.set(d, AIModules.data().byte1.get(d) | shasReview);
            consume(d);

            if (SETT.PATH().finders.entryPoints.find(a.tc().x(), a.tc().y(), d.path, int.MaxValue))
            {
                return base.activate(a, d);
            }
            HumanoidResource.dead = CAUSE_LEAVES.OTHER();
            return AI.plans().NOP.activate(a, d);
        }

        static bool isServiceReserved(FSERVICE s)
        {
            return s != null && s.findableReservedIs();
        }
    }
}