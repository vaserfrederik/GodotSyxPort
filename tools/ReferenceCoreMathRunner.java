import snake2d.util.bit.Bits;
import snake2d.util.misc.CLAMP;

/** Fixed boundary scenarios against the original compiled game classes. */
public final class ReferenceCoreMathRunner {
    public static void main(String[] args) {
        int[][] integers = {{-3, 0, 10}, {0, 0, 10}, {7, 0, 10}, {30, 0, 10}, {2, 5, -5}};
        for (int i = 0; i < integers.length; i++) {
            int[] c = integers[i];
            emit("int-" + i, CLAMP.i(c[0], c[1], c[2]));
        }
        int[][] bytes = {{-1, 0, 10}, {-8, -5, 5}, {5, -5, 5}, {127, -5, 5}, {-128, -200, 20}};
        for (int i = 0; i < bytes.length; i++) {
            int[] c = bytes[i];
            emit("byte-" + i, CLAMP.b((byte)c[0], c[1], c[2]));
        }
        double[][] doubles = {{Double.NaN, 1, 10}, {Double.NEGATIVE_INFINITY, 1, 10},
            {Double.POSITIVE_INFINITY, 1, 10}, {-3.5, 1, 10}, {5.25, 1, 10}, {30, 1, 10}};
        for (int i = 0; i < doubles.length; i++) {
            double[] c = doubles[i];
            emit("double-" + i, Double.doubleToLongBits(CLAMP.d(c[0], c[1], c[2])));
        }
        double[][] cycles = {{2.25, 5}, {5, 5}, {7.25, 5}, {12.25, 5}, {-2, 5}};
        for (int i = 0; i < cycles.length; i++) {
            double[] c = cycles[i];
            emit("cycle-" + i, Double.doubleToLongBits(CLAMP.c(c[0], c[1])));
        }
        int[] masks = {0, 0xF0, 0x0F, 0xA0, Integer.MIN_VALUE};
        for (int i = 0; i < masks.length; i++) {
            Bits bits = new Bits(masks[i]);
            int data = 0x5A5A5A5A;
            int value = bits.mask & 5;
            int set = bits.set(data, value);
            System.out.println("{\"case\":\"bits-" + i + "\",\"shift\":" + bits.scroll
                + ",\"mask\":" + bits.mask + ",\"set\":" + set
                + ",\"get\":" + bits.get(set) + ",\"inc\":" + bits.inc(set, 2)
                + ",\"max\":" + bits.isMaximum(set)
                + ",\"distance\":" + Bits.getDistance(5, 1, bits.mask) + "}");
        }
    }

    private static void emit(String name, long value) {
        System.out.println("{\"case\":\"" + name + "\",\"value\":" + value + "}");
    }
}
