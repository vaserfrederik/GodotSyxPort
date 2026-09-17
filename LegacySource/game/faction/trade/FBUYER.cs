using game.faction;
using init.trade;

namespace game.faction.trade
{
    public interface FBUYER
    {
        void AddReserve(int amount, TRADE_TYPE type, int price, Faction seller);
        void AddDeliver(int amount, TRADE_TYPE type);
        void AddReserveAndDeliver(int amount, TRADE_TYPE type)
        {
            AddReserve(amount, type, 0, null);
            AddDeliver(amount, type);
        }
        double BuyPriority(int amount, double bestPrice);
        int AddPrice(int amount);
    }
}