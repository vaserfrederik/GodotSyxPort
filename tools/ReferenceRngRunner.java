import java.io.OutputStream;
import java.io.PrintStream;
import java.lang.reflect.Method;

/** Narrow reference scenario against the supplied game JAR's RND class. */
public final class ReferenceRngRunner {
    public static void main(String[] args) throws Exception {
        if (args.length != 3) throw new IllegalArgumentException("seed count bound");
        int seed = Integer.parseInt(args[0]);
        int count = Integer.parseInt(args[1]);
        int bound = Integer.parseInt(args[2]);
        if (count < 1 || count > 10000) throw new IllegalArgumentException("count out of range");
        if (bound < 1) throw new IllegalArgumentException("bound must be positive");
        PrintStream stdout = System.out;
        Class<?> rnd;
        try (PrintStream muted = new PrintStream(OutputStream.nullOutputStream())) {
            System.setOut(muted); // RND's static initializer prints a nondeterministic startup seed.
            try { rnd = Class.forName("snake2d.util.rnd.RND"); }
            finally { System.setOut(stdout); }
        }
        Method setSeed = rnd.getMethod("setSeed", int.class);
        Method nextInt = rnd.getMethod("rInt");
        Method nextBounded = rnd.getMethod("rInt", int.class);
        Method nextBoolean = rnd.getMethod("rBoolean");
        Method nextFloat = rnd.getMethod("rFloat");
        Method nextLong = rnd.getMethod("rLong");
        setSeed.invoke(null, seed);
        for (int tick = 0; tick < count; tick++) {
            int integer = (int) nextInt.invoke(null);
            int bounded = (int) nextBounded.invoke(null, bound);
            boolean flag = (boolean) nextBoolean.invoke(null);
            float decimal = (float) nextFloat.invoke(null);
            long longValue = (long) nextLong.invoke(null);
            System.out.println("{\"tick\":" + tick + ",\"int\":" + integer
                + ",\"bounded\":" + bounded + ",\"boolean\":" + flag
                + ",\"float_bits\":" + Float.floatToIntBits(decimal)
                + ",\"long\":" + longValue + "}");
        }
    }
}
