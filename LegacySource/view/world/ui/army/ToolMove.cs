using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using view.world.ui.army;
using game.faction.diplomacy;
using init.constant;
using init.sprite;
using snake2d;
using util.text;
using view.main;
using view.subview;
using view.tool;
using world;
using world.army;
using world.entity;
using world.entity.army;
using world.map.regions;

namespace view.world.ui.army
{
    class ToolMove : PlacableSimpleTile
    {
        private bool can = false;
        private Region bReg = null;
        private WArmy incept = null;

        public ToolMove() : base(Dic.¤¤Move, "")
        {
            // TODO Auto-generated constructor stub
        }

        override public CharSequence isPlacable(int tx, int ty)
        {
            can = false;
            bReg = null;
            incept = null;

            if (AD.men(null).get(Army.army) == 0)
            {
                return Dic.¤¤MoveCant;
            }

            if (Army.army.ctx() == tx && Army.army.cty() == ty)
                return E;

            if (!WORLD.FOW().is(tx, ty))
            {
                foreach (WEntity e in WORLD.ENTITIES().fill(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH))
                {
                    if (e is WArmy && e != Army.army)
                    {
                        incept = (WArmy)e;
                        if (WORLD.PATH().path(Army.army.ctx(), Army.army.cty(), tx, ty, Army.army.path().treaty()) == null)
                        {
                            return Dic.¤¤Unreachable;
                        }
                        can = true;
                        WORLD.OVERLAY().hoverEntity(e);

                        if (DIP.WAR().is(incept.faction(), Army.army.faction()))
                        {
                            VIEW.hoverBox().text(Dic.¤¤Attack);
                            VIEW.mouse().setReplacement(SPRITES.icons().m.sword);
                        }
                        else
                        {
                            VIEW.hoverBox().text(Dic.¤¤Intercept);
                            VIEW.mouse().setReplacement(SPRITES.icons().m.crossair);
                        }
                        return null;
                    }
                }
            }

            Region reg = WORLD.REGIONS().centre.get(tx, ty);
            if (reg != null && WArmyState.canBesiege(Army.army, reg))
            {
                WORLD.OVERLAY().hoverBox(reg);
                VIEW.mouse().setReplacement(SPRITES.icons().m.sword);
                if (Army.army.besigeTile(reg) != null)
                {
                    VIEW.hoverBox().text(Dic.¤¤Besiege);
                    bReg = reg;
                    can = true;
                    return null;
                }
                else
                {
                    return Dic.¤¤Unreachable;
                }
            }

            if (WORLD.PATH().path(Army.army.ctx(), Army.army.cty(), tx, ty, Army.army.path().treaty()) == null)
            {
                return Dic.¤¤Unreachable;
            }

            can = true;
            VIEW.mouse().setReplacement(SPRITES.icons().m.crossair);
            VIEW.hoverBox().text(Dic.¤¤Move);

            return null;
        }

        override public void renderOverlay(GameWindow window)
        {
            WORLD.OVERLAY().hoverArmy(Army.army);
        }

        override public void renderPlaceHolder(SPRITE_RENDERER r, int tx, int ty, int cx, int cy, bool isPlacable)
        {
            if (bReg != null || incept != null)
                return;

            base.renderPlaceHolder(r, tx, ty, cx, cy, isPlacable);
        }

        override public void place(int tx, int ty)
        {
            if (!can)
                return;

            if (incept != null)
            {
                Army.army.intercept(incept);
                return;
            }
            else if (bReg != null)
            {
                Army.army.besiege(bReg);
            }
            else
            {
                Army.army.setDestination(tx, ty);
            }
        }

        final ToolConfig config = new ToolConfig()
        {
            override public void deactivateAction()
            {
            }

            override public void update(bool UIHovered)
            {
                if (!VIEW.world().panels.added(VIEW.world().UI.armies.army))
                    VIEW.world().tools.place(null, null, false);
            }

            override public bool back()
            {
                VIEW.world().panels.remove(VIEW.world().UI.armies.army);
                return true;
            }
        };
    }
}