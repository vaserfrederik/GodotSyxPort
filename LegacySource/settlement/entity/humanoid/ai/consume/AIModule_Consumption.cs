using System.Collections.Generic;

namespace Settlement.Entity.Humanoid.AI.Consume
{
    public class AIModule_Consumption : List<AIModule>
    {
        public readonly AIModule_Drink drink = new AIModule_Drink();
        public readonly AIModule_Food food = new AIModule_Food();
        public readonly AIModule_Shop shop = new AIModule_Shop();

        public AIModule_Consumption()
        {
            Add(drink);
            Add(food);
            Add(shop);
        }
    }
}