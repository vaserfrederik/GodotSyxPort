using System;
using game.battle.util;
using snake2d.util.datatypes;

namespace game.battle.formation
{
    public interface DivFormation : DivPosition, BODY_HOLDER
    {
        DIR dir();

        DIV_FORMATION formation();

        COORDINATE start();

        double dx();

        double dy();

        int width();

        int height(DIV_SPEC spec);

        int dirMaskOrtho(int i);

        DIR dir(int i);

        bool isEdge(int i);

        bool isSameAs(DivFormation o);

        COORDINATE centreTile();

        COORDINATE centrePixel();

        bool isCoherent();

        bool hasExtraRoom();
    }
}