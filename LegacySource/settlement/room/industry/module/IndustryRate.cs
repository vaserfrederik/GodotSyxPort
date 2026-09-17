using System.Collections.Generic;

namespace Settlement.Room.Industry.Module
{
    public interface IndustryRate
    {
        List<RoomBoost> Boosts();
        Boostable Bonus();
    }
}