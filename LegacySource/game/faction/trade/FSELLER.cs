using game.faction;
using init.trade;

namespace game.faction.trade
{
    public interface FSELLER
    {
        public int removePrice(int amount);
        public int removeMax();
        public void remove(int amount, TRADE_TYPE type, int price, Faction buyer);
    }
}