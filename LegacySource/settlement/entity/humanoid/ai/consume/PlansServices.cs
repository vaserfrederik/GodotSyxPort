using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.service.module;
using snake2d.util.sets;

namespace settlement.entity.humanoid.ai.consume
{
    public class PlansServices
    {
        private readonly ArrayList<Node> plans;
        private Tree<Node> sort;

        public PlansServices(params SPlanAbs[] plans)
        {
            int am = 0;
            foreach (var p in plans)
            {
                am += p.services.Count;
            }

            this.plans = new ArrayList<Node>(am);

            foreach (var p in plans)
            {
                foreach (var s in p.services)
                {
                    this.plans.Add(new Node(p, s.service()));
                }
            }

            sort = new Tree<Node>(this.plans.size())
            {
                protected override bool IsGreaterThan(Node current, Node cmp)
                {
                    return current.v > cmp.v;
                }
            };
        }

        public bool WorthTrying(Humanoid a, AIManager d)
        {
            foreach (var n in plans)
            {
                foreach (var s in n.plan.services)
                {
                    RoomServiceAccess b = s.service();
                    if (b.accessRequest(a) && b.finder.Has(a.tc()))
                        return true;
                }
            }
            return false;
        }

        public AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            sort.Clear();
            foreach (var n in plans)
            {
                if (n.b.accessRequest(a) && n.b.finder.Has(a.tc()))
                {
                    n.v = RND.rFloat() * n.b.usage;
                    if (!n.b.stats().access(a))
                    {
                        sort.Add(n);
                    }
                }
            }

            while (sort.hasMore())
            {
                Node n = sort.pollGreatest();
                d.planByte3 = (byte)n.b.room().typeIndex();
                AiPlanActivation p = n.plan.activate(a, d);
                if (p != null)
                    return p;
            }

            foreach (var n in plans)
            {
                if (n.b.accessRequest(a) && n.b.finder.Has(a.tc()))
                {
                    if (n.b.stats().access(a))
                    {
                        sort.Add(n);
                    }
                }
            }

            while (sort.hasMore())
            {
                Node n = sort.pollGreatest();
                d.planByte3 = (byte)n.b.room().typeIndex();
                AiPlanActivation p = n.plan.activate(a, d);
                if (p != null)
                    return p;
            }

            foreach (var n in plans)
            {
                n.b.clearAccess(a);
            }

            return null;
        }

        private class Node
        {
            private readonly SPlanAbs plan;
            private readonly RoomServiceAccess b;
            private double v;

            public Node(SPlanAbs plan, RoomServiceAccess b)
            {
                this.plan = plan;
                this.b = b;
            }
        }
    }
}