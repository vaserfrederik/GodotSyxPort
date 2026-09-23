import com.sun.source.tree.ClassTree;
import com.sun.source.tree.CompilationUnitTree;
import com.sun.source.tree.ImportTree;
import com.sun.source.util.JavacTask;
import com.sun.source.util.TreePathScanner;
import java.io.IOException;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;
import javax.tools.Diagnostic;
import javax.tools.DiagnosticCollector;
import javax.tools.JavaCompiler;
import javax.tools.JavaFileObject;
import javax.tools.SimpleJavaFileObject;
import javax.tools.ToolProvider;

/** Syntax-only Java AST inventory. No game code is compiled or copied into the port. */
public final class JavaAstInventory {
    private static final class Source extends SimpleJavaFileObject {
        final String path;
        final String body;

        Source(String path, String body) {
            super(URI.create("string:///" + path), Kind.SOURCE);
            this.path = path;
            this.body = body;
        }

        @Override public CharSequence getCharContent(boolean ignoreEncodingErrors) {
            return body;
        }
    }

    private static String json(String s) {
        StringBuilder out = new StringBuilder("\"");
        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);
            if (c == '"' || c == '\\') out.append('\\').append(c);
            else if (c == '\n') out.append("\\n");
            else if (c == '\r') out.append("\\r");
            else if (c == '\t') out.append("\\t");
            else if (c < 32) out.append(String.format("\\u%04x", (int)c));
            else out.append(c);
        }
        return out.append('"').toString();
    }

    private static String array(List<String> strings) {
        StringBuilder out = new StringBuilder("[");
        for (String s : strings) {
            if (out.length() > 1) out.append(',');
            out.append(json(s));
        }
        return out.append(']').toString();
    }

    public static void main(String[] args) throws Exception {
        if (args.length != 1) throw new IllegalArgumentException("usage: java JavaAstInventory.java sources.jar");
        JavaCompiler compiler = ToolProvider.getSystemJavaCompiler();
        if (compiler == null) throw new IllegalStateException("JDK compiler module is required");
        List<Source> sources = new ArrayList<>();
        try (ZipFile jar = new ZipFile(args[0])) {
            var entries = jar.entries();
            while (entries.hasMoreElements()) {
                ZipEntry entry = entries.nextElement();
                if (!entry.isDirectory() && entry.getName().endsWith(".java")) {
                    String body = new String(jar.getInputStream(entry).readAllBytes(), StandardCharsets.UTF_8);
                    sources.add(new Source(entry.getName(), body));
                }
            }
        }
        DiagnosticCollector<JavaFileObject> diagnostics = new DiagnosticCollector<>();
        JavacTask task = (JavacTask)compiler.getTask(null, null, diagnostics,
            List.of("-proc:none"), null, sources);
        for (CompilationUnitTree unit : task.parse()) {
            String path = URI.create(unit.getSourceFile().toUri().toString()).getPath().substring(1);
            String pkg = unit.getPackageName() == null ? "" : unit.getPackageName().toString();
            List<String> imports = new ArrayList<>();
            for (ImportTree imp : unit.getImports())
                imports.add((imp.isStatic() ? "static " : "") + imp.getQualifiedIdentifier());
            List<String> types = new ArrayList<>();
            new TreePathScanner<Void, String>() {
                @Override public Void visitClass(ClassTree node, String parent) {
                    String name = node.getSimpleName().toString();
                    if (!name.isEmpty()) {
                        String qualified = parent.isEmpty() ? name : parent + "." + name;
                        types.add(node.getKind() + " " + qualified);
                        return super.visitClass(node, qualified);
                    }
                    return super.visitClass(node, parent);
                }
            }.scan(unit, "");
            System.out.println("{\"JavaSource\":" + json(path) + ",\"Package\":" + json(pkg)
                + ",\"JavaImports\":" + array(imports) + ",\"JavaTypes\":" + array(types) + "}");
        }
        long errors = diagnostics.getDiagnostics().stream()
            .filter(d -> d.getKind() == Diagnostic.Kind.ERROR).count();
        if (errors > 0) {
            System.err.println("Java AST parse diagnostics: " + errors);
            for (Diagnostic<? extends JavaFileObject> d : diagnostics.getDiagnostics()) {
                if (d.getKind() == Diagnostic.Kind.ERROR)
                    System.err.println(d.getSource().toUri() + ":" + d.getLineNumber() + " " + d.getMessage(null));
            }
            System.exit(2);
        }
    }
}
