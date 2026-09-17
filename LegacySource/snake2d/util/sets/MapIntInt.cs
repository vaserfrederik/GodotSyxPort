using System;
using System.IO;

namespace Snake2d.Util.Sets
{
    public class MapIntInt : SAVABLE
    {
        private int[] keys;
        private int[] values;
        private int size;

        public MapIntInt() : this(16)
        {
        }

        public MapIntInt(int initialCapacity)
        {
            keys = Alloc.Ii(initialCapacity);
            values = Alloc.Ii(initialCapacity);
            size = 0;
        }

        /**
         * Puts key -> value. If key already exists, overwrites the value.
         * Returns the index in the internal array.
         */
        public int Put(int key, int value)
        {
            int index = Search(key);

            if (index >= 0)
            {
                // Key exists -> update value
                values[index] = value;
                return index;
            }

            // Insert in sorted position
            if (size >= keys.Length)
            {
                Grow();
            }

            // Shift elements to make space
            index = -(index + 1); // Convert to insertion point

            for (int i = size; i > index; i--)
            {
                keys[i] = keys[i - 1];
                values[i] = values[i - 1];
            }

            keys[index] = key;
            values[index] = value;
            size++;
            return index;
        }

        /**
         * Returns value for key, or -1 if not found
         */
        public int Get(int key)
        {
            int index = Search(key);
            return index >= 0 ? values[index] : -1;
        }

        /**
         * Returns value for key, or defaultValue if not found
         */
        public int GetOrDefault(int key, int defaultValue)
        {
            int index = Search(key);
            return index >= 0 ? values[index] : defaultValue;
        }

        public bool Contains(int key)
        {
            return Search(key) >= 0;
        }

        public int Size()
        {
            return size;
        }

        public bool IsEmpty()
        {
            return size == 0;
        }

        public void Clear()
        {
            size = 0;
        }

        /**
         * Get key at sorted position i
         */
        public int KeyAt(int i)
        {
            return keys[i];
        }

        /**
         * Get value at sorted position i
         */
        public int ValueAt(int i)
        {
            return values[i];
        }

        // Binary search - returns index if found, or negative insertion point
        private int Search(int key)
        {
            int low = 0;
            int high = size - 1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                if (keys[mid] < key)
                {
                    low = mid + 1;
                }
                else if (keys[mid] > key)
                {
                    high = mid - 1;
                }
                else
                {
                    return mid;
                }
            }
            return -(low + 1); // Not found, return insertion point
        }

        private void Grow()
        {
            int newCapacity = keys.Length * 2;
            int[] newKeys = Alloc.Ii(newCapacity);
            int[] newValues = Alloc.Ii(newCapacity);

            Array.Copy(keys, 0, newKeys, 0, size);
            Array.Copy(values, 0, newValues, 0, size);

            keys = newKeys;
            values = newValues;
        }

        // ==================== SAVABLE ====================

        public void Save(FilePutter file)
        {
            file.I(keys.Length);
            file.Is(keys);
            file.Is(values);
            file.I(size);
        }

        public void Load(FileGetter file)
        {
            int capacity = file.I();
            keys = Alloc.Ii(capacity);
            values = Alloc.Ii(capacity);
            file.Is(keys);
            file.Is(values);
            size = file.I();
        }
    }
}