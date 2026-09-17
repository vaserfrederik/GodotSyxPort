using System;

namespace Snake2D.Util.Misc
{
    public static class Sort
    {
        // Natural ordering entry point
        public static void Sort<T>(T[] ts) where T : IComparable<T>
        {
            if (ts == null || ts.Length <= 1)
                return;
            Sort(ts, 0, ts.Length, null);
        }

        public static void Sort<T>(T[] ts, int fromIndex, int toIndex) where T : IComparable<T>
        {
            Sort(ts, fromIndex, toIndex, null);
        }

        // With IComparer entry point
        public static void Sort<T>(T[] ts, IComparer<T> comparer)
        {
            if (ts == null || ts.Length <= 1)
                return;
            Sort(ts, 0, ts.Length, comparer);
        }

        /**
         * Ranged Heapsort Entry Point. Sorts the specified range of the array
         * [fromIndex, toIndex). * @param ts The array containing the elements to be
         * sorted.
         * 
         * @param fromIndex  The index of the first element (inclusive) to be sorted.
         * @param toIndex    The index of the last element (exclusive) to be sorted.
         * @param comparer The comparer to determine the order, or null for natural
         *                   ordering.
         */
        public static void Sort<T>(T[] ts, int fromIndex, int toIndex, IComparer<T> comparer)
        {
            if (ts == null || fromIndex >= toIndex || toIndex - fromIndex <= 1)
                return;
            if (fromIndex < 0 || toIndex > ts.Length)
                throw new IndexOutOfRangeException(
                        $"Invalid range: [{fromIndex}, {toIndex}) for array length {ts.Length}");

            int n = toIndex - fromIndex;

            // Phase 1: Build the heap inline within the ranged window boundary.
            // We map the binary tree mathematically relative to fromIndex.
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Sink(ts, i, n, fromIndex, comparer);
            }

            // Phase 2: Progressively extract elements from the heap and place them at the
            // end of the range.
            for (int i = n - 1; i > 0; i--)
            {
                // Swap the maximum element of the active window to the end of the shrunken
                // range
                Swap(ts, fromIndex, fromIndex + i);
                // Restore the binary max-heap property for the remaining items in the window
                Sink(ts, 0, i, fromIndex, comparer);
            }
        }

        // Purely iterative sink method - completely free of recursive stack trace
        // overhead
        private static void Sink<T>(T[] a, int k, int n, int offset, IComparer<T> cmp)
        {
            while (2 * k + 1 < n)
            {
                int j = 2 * k + 1; // Left child index relative to window start

                // If right child exists and is strictly greater than left child, jump pointers
                // to it
                if (j + 1 < n && Compare(a[offset + j], a[offset + j + 1], cmp) < 0)
                {
                    j++;
                }

                // If the parent node is already greater than or equal to its largest child,
                // heap is stable
                if (Compare(a[offset + k], a[offset + j], cmp) >= 0)
                {
                    break;
                }

                // Otherwise, swap parent and child, then follow the pointer down the tree
                // inline
                Swap(a, offset + k, offset + j);
                k = j;
            }
        }

        private static void Swap<T>(T[] a, int i, int j)
        {
            T tmp = a[i];
            a[i] = a[j];
            a[j] = tmp;
        }

        private static int Compare<T>(T a, T b, IComparer<T> cmp)
        {
            return cmp != null ? cmp.Compare(a, b) : Comparer<T>.Default.Compare(a, b);
        }
    }
}