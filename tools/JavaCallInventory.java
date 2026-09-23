import com.sun.source.tree.ClassTree;
import com.sun.source.tree.CompilationUnitTree;
import com.sun.source.tree.IdentifierTree;
import com.sun.source.tree.MemberSelectTree;
import com.sun.source.tree.MethodInvocationTree;
import com.sun.source.tree.MethodTree;
import com.sun.source.tree.NewClassTree;
import com.sun.source.tree.ParameterizedTypeTree;
import com.sun.source.tree.Tree;
import com.sun.source.util.JavacTask;
import com.sun.source.util.TreePathScanner;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import java.util.TreeSet;
import java.util.zip.ZipFile;
import javax.tools.Diagnostic;
import javax.tools.DiagnosticCollector;
import javax.tools.JavaCompiler;
import javax.tools.JavaFileObject;
import javax.tools.SimpleJavaFileObject;
import javax.tools.ToolProvider;

/** Syntax-only call inventory; identifiers are candidates, never resolved Java symbols. */
public final class JavaCallInventory {
    private static final class Source extends SimpleJavaFileObject {
        private final String body;
        Source(String path, String body) {
            super(URI.create("string:///" + path), Kind.SOURCE);
            this.body = body;
        }
        @Override public CharSequence getCharContent(boolean ignoreEncodingErrors) { return body; }
    }

    private record Call(String caller, String kind, String receiver, String name, int arity)
        implements Comparable<Call> {
        @Override public int compareTo(Call other) {
            int r = caller.compareTo(other.caller);
            if (r == 0) r = kind.compareTo(other.kind);
            if (r == 0) r = receiver.compareTo(other.receiver);
            if (r == 0) r = name.compareTo(other.name);
            return r == 0 ? Integer.compare(arity, other.arity) : r;
        }
    }

    private record Method(String owner, String name, int arity) implements Comparable<Method> {
        @Override public int compareTo(Method other) {
            int r = owner.compareTo(other.owner);
            if (r == 0) r = name.compareTo(other.name);
            return r == 0 ? Integer.compare(arity, other.arity) : r;
        }
    }

    private static String json(String value) {
        StringBuilder result = new StringBuilder("\"");
        for (int i = 0; i < value.length(); i++) {
            char c = value.charAt(i);
            if (c == '"' || c == '\\') result.append('\\').append(c);
            else if (c == '\n') result.append("\\n");
            else if (c == '\r') result.append("\\r");
            else if (c == '\t') result.append("\\t");
            else if (c < 32) result.append(String.format("\\u%04x", (int)c));
            else result.append(c);
        }
        return result.append('"').toString();
    }

    public static void main(String[] args) throws Exception {
        if (args.length != 1) throw new IllegalArgumentException("usage: JavaCallInventory.java sources.jar");
        JavaCompiler compiler = ToolProvider.getSystemJavaCompiler();
        if (compiler == null) throw new IllegalStateException("JDK compiler module is required");
        List<Source> sources = new ArrayList<>();
        try (ZipFile jar = new ZipFile(args[0])) {
            var entries = jar.entries();
            while (entries.hasMoreElements()) {
                var entry = entries.nextElement();
                if (!entry.isDirectory() && entry.getName().endsWith(".java"))
                    sources.add(new Source(entry.getName(),
                        new String(jar.getInputStream(entry).readAllBytes(), StandardCharsets.UTF_8)));
            }
        }
        DiagnosticCollector<JavaFileObject> diagnostics = new DiagnosticCollector<>();
        JavacTask task = (JavacTask)compiler.getTask(null, null, diagnostics,
            List.of("-proc:none"), null, sources);
        for (CompilationUnitTree unit : task.parse()) {
            String path = URI.create(unit.getSourceFile().toUri().toString()).getPath().substring(1);
            String pkg = unit.getPackageName() == null ? "" : unit.getPackageName().toString();
            Map<Call, Integer> calls = new TreeMap<>();
            Set<Method> methods = new TreeSet<>();
            new TreePathScanner<Void, Void>() {
                String owner = "";
                String method = "<initializer>/0";

                @Override public Void visitClass(ClassTree node, Void unused) {
                    String previous = owner;
                    String name = node.getSimpleName().toString();
                    if (!name.isEmpty()) owner = owner.isEmpty() ? name : owner + "." + name;
                    try { return super.visitClass(node, unused); }
                    finally { owner = previous; }
                }

                @Override public Void visitMethod(MethodTree node, Void unused) {
                    String previous = method;
                    method = node.getName() + "/" + node.getParameters().size();
                    methods.add(new Method(owner, node.getName().toString(), node.getParameters().size()));
                    try { return super.visitMethod(node, unused); }
                    finally { method = previous; }
                }

                @Override public Void visitMethodInvocation(MethodInvocationTree node, Void unused) {
                    String receiver = "";
                    String name;
                    Tree select = node.getMethodSelect();
                    if (select instanceof IdentifierTree identifier) {
                        name = identifier.getName().toString();
                    } else if (select instanceof MemberSelectTree member) {
                        name = member.getIdentifier().toString();
                        receiver = identifierPath(member.getExpression());
                    } else {
                        return super.visitMethodInvocation(node, unused);
                    }
                    record(calls, new Call(owner + "#" + method, "invoke", receiver,
                        name, node.getArguments().size()));
                    return super.visitMethodInvocation(node, unused);
                }

                @Override public Void visitNewClass(NewClassTree node, Void unused) {
                    Tree type = node.getIdentifier();
                    if (type instanceof ParameterizedTypeTree generic) type = generic.getType();
                    String target = identifierPath(type);
                    if (!target.isEmpty()) record(calls, new Call(owner + "#" + method,
                        "construct", target, "<init>", node.getArguments().size()));
                    return super.visitNewClass(node, unused);
                }
            }.scan(unit, null);
            StringBuilder out = new StringBuilder("{\"JavaSource\":").append(json(path))
                .append(",\"Package\":").append(json(pkg)).append(",\"Methods\":[");
            for (Method m : methods) {
                if (out.charAt(out.length() - 1) != '[') out.append(',');
                out.append("{\"owner\":").append(json(m.owner))
                    .append(",\"name\":").append(json(m.name))
                    .append(",\"arity\":").append(m.arity).append('}');
            }
            out.append("],\"Calls\":[");
            for (var entry : calls.entrySet()) {
                Call c = entry.getKey();
                if (out.charAt(out.length() - 1) != '[') out.append(',');
                out.append("{\"caller\":").append(json(c.caller))
                    .append(",\"kind\":").append(json(c.kind))
                    .append(",\"receiver\":").append(json(c.receiver))
                    .append(",\"name\":").append(json(c.name))
                    .append(",\"arity\":").append(c.arity)
                    .append(",\"count\":").append(entry.getValue()).append('}');
            }
            System.out.println(out.append("]}").toString());
        }
        long errors = diagnostics.getDiagnostics().stream()
            .filter(d -> d.getKind() == Diagnostic.Kind.ERROR).count();
        if (errors != 0) throw new IllegalStateException("Java AST parse diagnostics: " + errors);
    }

    private static String identifierPath(Tree expression) {
        if (expression instanceof IdentifierTree id) return id.getName().toString();
        if (expression instanceof MemberSelectTree member) {
            String parent = identifierPath(member.getExpression());
            return parent.isEmpty() ? "" : parent + "." + member.getIdentifier();
        }
        return "";
    }

    private static void record(Map<Call, Integer> calls, Call call) {
        calls.merge(call, 1, Integer::sum);
    }
}
