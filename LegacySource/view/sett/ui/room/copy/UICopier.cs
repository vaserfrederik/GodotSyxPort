using view.main;

namespace view.sett.ui.room.copy
{
    public class UICopier
    {
        private readonly Source source = new Source();
        private readonly Dest dest = new Dest(source);
        private readonly Second second = new Second(dest);
        private readonly First first = new First(source);
        private readonly FirstConfig config = new FirstConfig(source, second, first);

        public UICopier()
        {
        }

        public void Activate()
        {
            source.Init();
            second.RotSet(0);
            VIEW.S().Tools.Place(first, config);
        }
    }
}