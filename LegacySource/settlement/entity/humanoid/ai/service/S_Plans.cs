using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.stocks;
using settlement.room.service.module;
using snake2d;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace settlement.entity.humanoid.ai.service
{
    class S_Plans
    {
        private readonly double[] usageI = new double[NEEDS.ALL().Size()];

        public readonly ArrayListGrower<S_Plan> all = new ArrayListGrower<S_Plan>();
        public readonly ArrayList<ArrayListGrower<S_Plan>> needMap = new ArrayList<ArrayListGrower<S_Plan>>(NEEDS.ALL().Size());

        public S_Plans()
        {
            while (needMap.HasRoom())
                needMap.Add(new ArrayListGrower<S_Plan>());

            add(new M_PlanBarber());
            add(new M_PlanBath());
            add(new M_PlanBrothel());
            add(new M_PlanHearth());
            add(new M_PlanLavatory());
            add(new M_PlanPhysician());
            add(new M_PlanWell());
            add(new M_PlanCourt());

            add(new M_PlanSpectator("speak", SETT.ROOMS().SPEAKERS));
            add(new M_PlanSpectator("stock", new ArrayList<ROOM_STOCKS>(SETT.ROOMS().STOCKS)));
            add(new M_PlanSpectator("stage", SETT.ROOMS().STAGES));
            add(new M_PlanSpectator("arena", SETT.ROOMS().FIGHTPITS));
            add(new M_PlanSpectator("garena", SETT.ROOMS().GARENAS));

            {
                add(new S_PlanShrine());
                add(new S_PlanTemple());
            }

            {
                add(new PlanSkinny());
            }

            for (int i = 0; i < usageI.Length; i++)
            {
                if (usageI[i] > 0)
                    usageI[i] = 1.0 / usageI[i];
            }

            foreach (NEED n in NEEDS.ALL())
            {
                if (n is NEED_E)
                    continue;
                if (needMap.Get(n.index()).Size() == 0)
                    LOG.err(n.key);
            }
        }

        private void add(MPlan<ROOM_SERVICE_ACCESS_HASER> plan)
        {
            foreach (ROOM_SERVICE_ACCESS_HASER ss in plan.services)
            {
                RoomServiceAccess n = ss.service();
                S_Plan p = new S_Plan(n.stats(), n.usage)
                {
                    public override bool hasAccess(Humanoid a, AIManager d)
                    {
                        return n.stats().access().indu().get(a.indu()) > 0;
                    }

                    public override AiPlanActivation getPlan(Humanoid a, AIManager d)
                    {
                        return getPlan(a, d, n.radius());
                    }

                    public override bool allowed(Humanoid a, AIManager d)
                    {
                        return n.stats().accessRequest(a) && n.finder.has(a.tc());
                    }

                    public override bool goodTime(Humanoid a, AIManager d)
                    {
                        return n.isGoodTime();
                    }

                    public override AiPlanActivation getPlan(Humanoid a, AIManager d, int dist)
                    {
                        if (n.stats().accessRequest(a) && n.finder.has(a.tc()))
                        {
                            d.planByte3 = (byte)n.room().typeIndex();
                            MPlan.dist = dist;
                            AiPlanActivation p = plan.activate(a, d);
                            if (p != null)
                                return p;
                        }
                        n.clearAccess(a);
                        return null;
                    }
                };
                add(p);
            }
        }

        private void add(M_PlanSpectator plan)
        {
            foreach (ROOM_SPECTATOR_HASER s in plan.services)
            {
                add(new S_PlanEntertain(s.service().need, s, plan));
            }
        }

        private S_Plan add(S_Plan p)
        {
            usageI[p.need.index()] += p.usage;
            needMap.Get(p.need.index()).Add(p);
            all.Add(p);
            return p;
        }

        public AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            double lacksAccessTot = 0;
            double allTot = 0;

            foreach (S_Plan p in all)
            {
                if (!p.allowed(a, d))
                {
                    p.service.clearAccess(a.indu());
                    continue;
                }

                double v = p.need.rate.get(a.indu()) * usageI[p.need.index()];
                if (p.goodTime(a, d) && !p.hasAccess(a, d))
                {
                    lacksAccessTot += v;
                }
                allTot += v;
            }

            if (lacksAccessTot > 0)
            {
                lacksAccessTot = RND.rFloat() * lacksAccessTot;
                foreach (S_Plan p in all)
                {
                    if (p.allowed(a, d) && p.goodTime(a, d) && !p.hasAccess(a, d))
                    {
                        double v = p.need.rate.get(a.indu()) * usageI[p.need.index()];
                        lacksAccessTot -= v;
                        if (lacksAccessTot <= 0)
                        {
                            AiPlanActivation pp = p.getPlan(a, d);
                            if (pp != null)
                                return pp;
                            p.service.clearAccess(a.indu());
                            break;
                        }
                    }
                }
            }

            allTot = RND.rFloat() * allTot;
            foreach (S_Plan p in all)
            {
                if (p.allowed(a, d) && p.goodTime(a, d))
                {
                    double v = p.need.rate.get(a.indu()) * usageI[p.need.index()];
                    allTot -= v;
                    if (allTot <= 0)
                    {
                        AiPlanActivation pp = p.getPlan(a, d);
                        if (pp == null)
                            p.service.clearAccess(a.indu());
                        return pp;
                    }
                }
            }

            return null;
        }
    }
}