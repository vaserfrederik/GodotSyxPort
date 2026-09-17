using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace snake2d
{
    internal sealed class VboSorter
    {
        private readonly Chunk[] chunks = new Chunk[256];
        private Chunk[] firstChunk = new Chunk[128];
        private Chunk[] currentChunk = new Chunk[128];
        private readonly Counts counts = new Counts();
        
        private readonly int[] data;
        private int chunkI = 0;

        public VboSorter(int size)
        {
            int MAX = size / chunks.Length;
            
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new Chunk(i * MAX, i * MAX + MAX);
            }
            data = Alloc.Ii(size);
        }
        
        public void Add(int layer, int v)
        {
            if (chunkI >= chunks.Length)
            {
                return;
            }
            
            Chunk c = currentChunk[layer];
            if (c == null)
            {
                c = chunks[chunkI];
                c.nextChunk = null;
                c.count = c.start;
                chunkI++;
                firstChunk[layer] = c;
                currentChunk[layer] = c;
            }
            else if (c.count >= c.max)
            {
                Chunk prev = c;
                c = chunks[chunkI];
                c.nextChunk = null;
                c.count = c.start;
                chunkI++;
                prev.nextChunk = c;
                currentChunk[layer] = c;
            }

            data[c.count] = v;
            c.count++;
        }
        
        public void Clear()
        {
            currentChunk.Fill(null);
            firstChunk.Fill(null);
            chunkI = 0;
        }
        
        public Counts Fill(Buffer<int> buff)
        {
            buff.Position = 0;
            
            for (int i = 0; i < firstChunk.Length; i++)
            {
                Chunk c = firstChunk[i];
                if (c != null)
                {
                    counts.from[i] = buff.Position;
                    while (c != null)
                    {
                        buff.Put(data, c.start, c.count - c.start);
                        c = c.nextChunk;
                    }
                    counts.to[i] = buff.Position;
                }
                else
                {
                    counts.from[i] = 0;
                    counts.to[i] = 0;
                }
            }
            
            Clear();
            return counts;
        }
        
        private sealed class Chunk
        {
            public readonly int start;
            public readonly int max;
            public int count;
            public Chunk nextChunk;
            
            public Chunk(int start, int max)
            {
                this.start = start;
                this.max = max;
            }
        }
        
        public class Counts
        {
            public readonly int[] from = Alloc.Ii(128);
            public readonly int[] to = Alloc.Ii(128);
        }
    }
}