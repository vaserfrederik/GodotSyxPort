using view.sett.ui.health;
using view.ui.div;
using view.ui.economy;
using view.ui.family;
using view.ui.goods;
using view.ui.log;
using view.ui.manage;
using view.ui.profile;
using view.ui.raider;
using view.ui.tech;
using view.ui.tourism;
using view.ui.wiki;

public class UIView
{
    public readonly UITreasury economy;
    public readonly UITourists tourists;
    public readonly UIGoods goods;
    public readonly UITechTree tech;
    public readonly UIRaiding raider;
    public readonly UIProfile profile;
    public readonly UIFamilyTree family;
    public readonly UILevel level;
    public readonly UIHealth health;
    public readonly UILog log = new UILog(null);
    public readonly WIKI wiki = new WIKI();
    public readonly UIDiv div = new UIDiv();
    public readonly IManager manager;

    public UIView()
    {
        economy = new UITreasury();
        goods = new UIGoods();
        tech = new UITechTree();
        level = new UILevel();
        family = new UIFamilyTree();
        profile = new UIProfile(true);
        health = new UIHealth();
        tourists = new UITourists();
        raider = new UIRaiding();
        manager = new IManager(this);
    }
}