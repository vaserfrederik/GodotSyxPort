using System;
using System.IO;
using game.debug;
using game.faction;
using init.race;
using init.type;
using settlement.main;
using snake2d;
using snake2d.util.file;
using util.rendering;
using view.main;

namespace settlement.entry
{
    public sealed class SENTRY : SettResource
    {
        public SENTRY() : base("SENTRY", false)
        {
            // TODO Auto-generated constructor stub
        }

        private readonly PeopleSpawner spawn = new PeopleSpawner();
        private readonly Immigration im = new Immigration();
        public readonly EntryPoints points = new EntryPoints();
        private readonly EntryUpdater updater = new EntryUpdater();

        public void Add(Race race, HTYPE type, int amount)
        {
            if (amount <= 0)
                return;
            spawn.Add(race, type, amount);
        }

        public int OnTheirWay(Race race, HTYPE type)
        {
            return spawn.OnTheirWay(race, type);
        }

        protected override void Update(double ds, Profiler profiler)
        {
            if (VIEW.b().IsActive())
                return;

            if (FACTIONS.player().capitolRegion() == null)
                return;

            points.Update();

            updater.Update(ds, points);

            if (IsClosed())
            {
                spawn.Update(0);
                im.Update(0);
            }
            else
            {
                spawn.Update(ds);
                im.Update(ds);
            }
        }

        public void Render(Renderer r, RenderData renData)
        {
            points.Render(r, renData);
        }

        protected override void Save(FilePutter file)
        {
            updater.Save(file);
            points.saver.Save(file);
            spawn.Save(file);
            im.saver.Save(file);
        }

        protected override void Load(FileGetter file) => throw new NotImplementedException();

        protected override void Clear()
        {
            updater.Clear();
            points.saver.Clear();
            spawn.Clear();
            im.saver.Clear();
        }

        protected override void Init(bool loaded) { }

        public Immigration Immi() => im;

        public bool IsClosed() => updater.IsClosed() || SETT.INVADOR().invading();

        public bool Beseiged() => updater.Beseiged();

        public double BesigeTime() => updater.BesigeTime();
    }
}