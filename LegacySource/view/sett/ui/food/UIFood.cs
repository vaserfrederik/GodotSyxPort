using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using init.resources;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.service.food.canteen;
using settlement.room.service.food.eatery;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.gui.window;
using snake2d.util.sets;

namespace yournamespace
{
    public class UIFood : GUIPanel
    {
        private static UIFood _instance;
        public static UIFood Instance => _instance ?? (_instance = new UIFood());

        private UIFood()
        {
            SetSize(600, 400);
            Add(new Label(Label.LARGE, "Food Overview"));
            // Add other components here
        }
    }

    public class UIFoodResource : GUIPanel
    {
        private readonly ResG _resource;

        public UIFoodResource(ResG resource)
        {
            _resource = resource;
            SetSize(200, 100);
            Add(new Label(Label.MEDIUM, resource.resource.names));
            // Add other components here
        }
    }
}