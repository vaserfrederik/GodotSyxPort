using System;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;
using util.data.DOUBLE;
using util.data.INT;

namespace game.battle.thread.general.offence
{
    class Context : SAVABLE
    {
        public readonly Bitmap2D blob = new Bitmap2D(SETT.TILE_BOUNDS, false);
        public readonly Bitmap2D block = new Bitmap2D(SETT.TILE_BOUNDS, false);
        public readonly ContextLines lines = new ContextLines();
        public IntImp checkI = new IntImp();
        public readonly DoubleImp value = new DoubleImp();
        public Bitmap1D deployedToLine = new Bitmap1D(Config.battle().DIVISIONS_PER_ARMY, false);
        public int[] distsToLine = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
        public int[] distsFromLineToBlob = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
        public int[] trickedDivs = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
        public double flanking = RND.rFloat() * 50;
        public readonly UtilDivMap map = new UtilDivMap();

        public Context()
        {
        }

        public override void save(FilePutter file)
        {
            blob.save(file);
            block.save(file);
            lines.save(file);
            checkI.save(file);
            value.save(file);
            deployedToLine.save(file);
            file.isE(distsToLine);
            file.isE(distsFromLineToBlob);
            file.isE(trickedDivs);
        }

        public override void load(FileGetter file)
        {
            blob.load(file);
            block.load(file);
            lines.load(file);
            checkI.load(file);
            value.load(file);
            deployedToLine.load(file);
            file.isE(distsToLine);
            file.isE(distsFromLineToBlob);
            file.isE(trickedDivs);
        }

        public override void clear()
        {
            flanking = RND.rFloat() * 50;
            lines.clear();
        }
    }
}