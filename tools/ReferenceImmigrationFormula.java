import java.nio.file.Files;
import java.nio.file.Path;

/** Isolated numerical extraction of Immigration.getImmigrants(Race) from the supplied Java source. */
class ReferenceImmigrationFormula {
    public static void main(String[] args) throws Exception {
        var lines = Files.readAllLines(Path.of(args[0]));
        for (String line : lines.subList(1, lines.size())) {
            String[] v = line.split(",");
            double happiness = Double.parseDouble(v[1]);
            double expectedPopulation = Double.parseDouble(v[2]);
            double raceMaximum = Double.parseDouble(v[3]);
            int currentAndIncoming = Integer.parseInt(v[4]);
            boolean campAvailable = Boolean.parseBoolean(v[5]);
            int campPopulation = Integer.parseInt(v[6]);
            double standingPower = Double.parseDouble(v[7]);
            int wanted;
            if (campAvailable) {
                wanted = Math.max(campPopulation - currentAndIncoming, 0);
            } else if (expectedPopulation == 0) {
                wanted = (int)Math.ceil(happiness - 0.1);
            } else {
                happiness = Math.max(0, Math.min(2, happiness));
                happiness -= 0.9;
                if (happiness <= 0) {
                    wanted = (int)(expectedPopulation * happiness / 0.9);
                } else {
                    happiness *= 0.5;
                    double amount = happiness * raceMaximum * expectedPopulation;
                    if (amount > 1) {
                        double blend = amount / (amount + expectedPopulation);
                        amount = amount * (1 - blend)
                            + blend * Math.pow(amount, 1 / standingPower);
                    }
                    wanted = Math.max((int)Math.ceil(amount), 0);
                }
            }
            System.out.println("{\"case\":\"" + v[0] + "\",\"wanted\":" + wanted + "}");
        }
    }
}
