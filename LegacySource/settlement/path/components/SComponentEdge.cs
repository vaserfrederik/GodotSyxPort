using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    public sealed class SComponentEdge
    {
        private SComponentEdge next;
        private SComponent to;
        private float cost;
        private float distance;
        private static List<SComponentEdge> chache = new List<SComponentEdge>(2048);
        private static int count = 0;

        private SComponentEdge()
        {
        }

        public SComponent To()
        {
            return to;
        }

        public double Cost2()
        {
            return cost;
        }

        public double Distance()
        {
            return distance;
        }

        void Retire()
        {
            next = null;
            to = null;
            count--;
            if (chache.Count < chache.Capacity)
                chache.Add(this);
        }

        static SComponentEdge Create(SComponent to, double cost, double distance, SComponentEdge next)
        {
            count++;
            SComponentEdge e = null;
            if (chache.Count > 0)
                e = chache[chache.Count - 1];
            else
                e = new SComponentEdge();
            chache.RemoveAt(chache.Count - 1);

            e.to = to;
            e.cost = (float)cost;
            e.distance = (float)distance;
            e.next = next;
            return e;
        }

        static int Count()
        {
            return count;
        }

        void SetNext(SComponentEdge e)
        {
            this.next = e;
        }

        public SComponentEdge Next()
        {
            return next;
        }
    }
}