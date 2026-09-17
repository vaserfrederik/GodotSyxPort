using game.battle.thread.status;
using game;
using game.battle.div;
using init.sprite;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.misc;
using snake2d.util.sets;
using util.colors;
using util.rendering;
using view.interrupter;

class Tests
{
    public Tests(BattleStatus status)
    {
        ON_TOP_RENDERABLE top = new ON_TOP_RENDERABLE()
        {
            private ArrayList<Div> res = new ArrayList<Div>(16);

            public void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
            {
                RenderData.RenderIterator it = data.onScreenTiles();

                DivsTileMap m = BattleStatus.map();

                while (it.has())
                {
                    res.clear();
                    int ai = 0;
                    foreach (Div d in m.get(res, it.tx(), it.ty()))
                    {
                        if (d.army() == GAME.ARMIES().player())
                        {
                            GCOLOR.MAP().BEST.bind();
                        }
                        else
                        {
                            GCOLOR.MAP().BAD.bind();
                        }
                        SPRITES.icons().s.dot.renderC(r, it.x() + (ai % 4) * 16, it.y() + (ai / 4) * 16);
                        ai++;
                    }

                    it.next();
                }

                COLOR.unbind();
            }
        };

        IDebugPanel.add("battle status map", new ACTION()
        {
            public void exe()
            {
                top.add();
            }
        });
    }
}