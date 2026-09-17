using System;
using System.Collections.Generic;
using game;
using game.audio;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.color;
using util.colors;
using util.gui.misc;
using util.rendering;
using view.battle;
using view.main;
using view.subview;

public sealed class BattlePlacerPlace : Mode
{
    private readonly GameWindow w;
    private readonly DivSelection s;
    private readonly Action a;
    private bool sounded = false;
    private readonly SoundRace sound = AUDIO.race("UP_PLACE_DIV");

    public BattlePlacerPlace(GameWindow w, DivSelection s, Action a)
    {
        this.w = w;
        this.s = s;
        this.a = a;
    }

    override public void update(bool hovered)
    {
        if (!hovered)
            return;

        if (a.clicked)
        {
        }
        else
        {
            sounded = false;
        }

        if (a.clickReleased)
        {
            GAME.ARMIES().placer.deploy(s.selection(), a.start.x(), w.pixel().x(), a.start.y(), w.pixel().y());
            if (VIEW.b().state() != null && VIEW.b().state().deploying())
            {
                GAME.ARMIES().initAndTeleport(s.selection());
            }
        }
    }

    override public void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
    {
        if (!a.clicked)
        {
            int px = w.pixel().x();
            int py = w.pixel().y();
            if (s.selection().size() == 0 || GAME.ARMIES().placer.isBlocked(px, py, C.TILE_SIZE, s.selection().get(0).army()))
                GCOLOR.MAP().BAD.bind();
            else
            {
                GCOLOR.MAP().BATTLE_OK.bind();
            }

            SPRITES.cons().BIG.dots.renderCentered(r, 0, px - data.offX1(), py - data.offY1());
            VIEW.mouse().setReplacement(SPRITES.icons().m.place_line);

            COLOR.unbind();
            return;
        }
        else
        {
            if (GAME.ARMIES().placer.render(r, s.selection(), a.start.x(), w.pixel().x(), a.start.y(), w.pixel().y(), data))
            {
                if (!sounded)
                {
                    sounded = true;
                    sound.play(true);
                }
            }
        }
    }

    override public void hoverTimer(GBox text)
    {
    }
}