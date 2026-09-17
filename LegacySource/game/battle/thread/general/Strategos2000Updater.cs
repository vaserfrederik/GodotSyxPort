using System;
using snake2d;
using snake2d.util.file;
using util.rendering;

namespace game.battle.thread.general
{
    public abstract class Strategos2000Updater : SAVABLE
    {
        public abstract bool Update();
        public abstract void Render(Renderer r, RenderIterator it);
        public abstract void Render(Renderer r, ShadowBatch shadowBatch, RenderData data);
    }
}