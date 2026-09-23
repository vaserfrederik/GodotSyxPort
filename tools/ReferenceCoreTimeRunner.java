import java.lang.reflect.Constructor;
import java.lang.reflect.Method;
import snake2d.CoreTime;

/** Exercises the original compiled engine clock, including package-private update. */
public final class ReferenceCoreTimeRunner {
    public static void main(String[] args) throws Exception {
        Constructor<CoreTime> constructor = CoreTime.class.getDeclaredConstructor();
        constructor.setAccessible(true);
        CoreTime clock = constructor.newInstance();
        Method update = CoreTime.class.getDeclaredMethod("update", float.class, long.class, long.class);
        update.setAccessible(true);
        float[] deltas = {0f, 0.25f, 0.75f, 0.2f, 0.8f, 0.4f, 2.4f, 0.6f, -0.3f, -1.5f, 0.1f};
        emit(clock, 0);
        for (int i = 0; i < deltas.length; i++) {
            update.invoke(clock, deltas[i], 1010L + i * 31L, 999999999L + i * 1000000L);
            emit(clock, i + 1);
        }
    }

    private static void emit(CoreTime clock, int step) {
        System.out.println("{\"step\":" + step
            + ",\"secondsBits\":" + Double.doubleToLongBits(clock.getSecondsSinceFirstUpdate())
            + ",\"millis\":" + clock.getNowMillis()
            + ",\"nanos\":" + clock.getNowNanos()
            + ",\"pendulumBits\":" + Float.floatToIntBits(clock.getPendulum0To1s1()) + "}");
    }
}
