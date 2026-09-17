using settlement.path.components.finder;

namespace settlement.path.finders
{
    public interface SFINDER : SCompPatherFinder
    {
        bool IsTile(int tx, int ty, int tileNr);
    }
}