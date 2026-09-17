using System;
using System.IO;
using Newtonsoft.Json;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.race.appearence
{
    public sealed class RAddon
    {
        public readonly ColorCollection Col;
        public readonly TILE_SHEET SheetStand;
        public readonly TILE_SHEET SheetLay;
        public readonly Lockable<Induvidual> Cons;

        public RAddon(Json json, RColors colors, RAddon[] done)
        {
            Cons = GVALUES.INDU.LOCK.Push();
            Cons.Push("CONDITIONS", json);
            Col = json.Has("COLOR") ? colors.Collection.Read("COLOR", json) : ColorCollection.DUMMY;
            int ii = json.i("ADDON_INDEX", 0, 8);

            if (done[ii] != null)
            {
                SheetStand = done[ii].SheetStand;
                SheetLay = done[ii].SheetLay;
                return;
            }

            int y1 = 194 + 44 * ii;
            int x1 = 66;

            SheetStand = new ITileSheet()
            {
                Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    s.Singles.Init(x1, y1, 1, 1, 2, 1, d.S24);
                    s.Singles.SetSkip(0, 2).Paste(3, true);
                    return d.S24.SaveGame();
                }
            }.Get();

            SheetLay = new ITileSheet()
            {
                Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    s.Singles.Init(s.Singles.Body().x2(), y1, 1, 1, 2, 1, d.S32);
                    s.Singles.SetSkip(0, 2).Paste(3, true);
                    return d.S32.SaveGame();
                }
            }.Get();
        }

        public void RenderStanding(Renderer r, int dir, int x, int y, Induvidual in2, bool dead)
        {
            Render(SheetStand, r, dir, x, y, in2, dead);
        }

        public void RenderLaying(Renderer r, int dir, int x, int y, Induvidual in2, bool dead)
        {
            Render(SheetLay, r, dir, x, y, in2, dead);
        }

        public void RenderLayingTextured(TILE_SHEET stencil, int si, Renderer r, int dir, int x, int y, Induvidual in2, bool dead)
        {
            if (!Cons.Passes(in2))
                return;

            Col.Get(in2, dead).Bind();

            stencil.RenderTextured(SheetLay.GetTexture(dir), si, x, y);
            COLOR.Unbind();
        }

        private void Render(TILE_SHEET s, Renderer r, int dir, int x, int y, Induvidual in2, bool dead)
        {
            if (!Cons.Passes(in2))
                return;

            Col.Get(in2, dead).Bind();
            s.Render(r, dir, x, y);
            COLOR.Unbind();
        }

        public void RenderLaying(Renderer r, int dir, int x, int y, Induvidual in2, bool dead, COLOR cDecay, double decay)
        {
            if (!Cons.Passes(in2))
                return;

            ColorImp.TMP.Interpolate(Col.Get(in2, dead), cDecay, decay);
            ColorImp.TMP.Bind();
            SheetLay.Render(r, dir, x, y);
            COLOR.Unbind();

            Render(SheetLay, r, dir, x, y, in2, dead);
        }
    }
}