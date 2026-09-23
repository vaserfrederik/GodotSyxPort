import com.sun.source.tree.ClassTree;
import com.sun.source.tree.BlockTree;
import com.sun.source.tree.CatchTree;
import com.sun.source.tree.CompilationUnitTree;
import com.sun.source.tree.EnhancedForLoopTree;
import com.sun.source.tree.ForLoopTree;
import com.sun.source.tree.IdentifierTree;
import com.sun.source.tree.LambdaExpressionTree;
import com.sun.source.tree.MemberSelectTree;
import com.sun.source.tree.MethodInvocationTree;
import com.sun.source.tree.MethodTree;
import com.sun.source.tree.NewClassTree;
import com.sun.source.tree.ParameterizedTypeTree;
import com.sun.source.tree.StatementTree;
import com.sun.source.tree.Tree;
import com.sun.source.tree.TryTree;
import com.sun.source.tree.VariableTree;
import com.sun.source.util.JavacTask;
import com.sun.source.util.Trees;
import com.sun.source.util.TreePathScanner;
import com.sun.source.util.TreeScanner;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.HashSet;
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

    private record Call(String caller, String kind, String receiver, String receiverTypeHint,
        String name, int arity)
        implements Comparable<Call> {
        @Override public int compareTo(Call other) {
            int r = caller.compareTo(other.caller);
            if (r == 0) r = kind.compareTo(other.kind);
            if (r == 0) r = receiver.compareTo(other.receiver);
            if (r == 0) r = receiverTypeHint.compareTo(other.receiverTypeHint);
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
        var positions = Trees.instance(task).getSourcePositions();
        for (CompilationUnitTree unit : task.parse()) {
            String path = URI.create(unit.getSourceFile().toUri().toString()).getPath().substring(1);
            String pkg = unit.getPackageName() == null ? "" : unit.getPackageName().toString();
            Map<Call, Integer> calls = new TreeMap<>();
            Set<Method> methods = new TreeSet<>();
            Map<String, List<String>> parents = new TreeMap<>();
            new TreePathScanner<Void, Void>() {
                String owner = "";
                String method = "<initializer>/0";
                ClassTree ownerNode;
                MethodTree methodNode;
                Set<String> methodLocalNames = Set.of();

                @Override public Void visitClass(ClassTree node, Void unused) {
                    String previous = owner;
                    String previousMethod = method;
                    ClassTree previousNode = ownerNode;
                    MethodTree previousMethodNode = methodNode;
                    Set<String> previousLocals = methodLocalNames;
                    String name = node.getSimpleName().toString();
                    if (!name.isEmpty()) owner = owner.isEmpty() ? name : owner + "." + name;
                    List<String> bases = new ArrayList<>();
                    if (node.getExtendsClause() != null)
                        bases.add(identifierPath(node.getExtendsClause()));
                    for (Tree base : node.getImplementsClause())
                        bases.add(identifierPath(base));
                    if (!name.isEmpty())
                        parents.put(owner, bases.stream().filter(base -> !base.isEmpty()).toList());
                    ownerNode = node;
                    method = "<initializer>/0";
                    methodNode = null;
                    methodLocalNames = Set.of();
                    try { return super.visitClass(node, unused); }
                    finally {
                        owner = previous;
                        method = previousMethod;
                        ownerNode = previousNode;
                        methodNode = previousMethodNode;
                        methodLocalNames = previousLocals;
                    }
                }

                @Override public Void visitMethod(MethodTree node, Void unused) {
                    String previous = method;
                    MethodTree previousNode = methodNode;
                    Set<String> previousLocals = methodLocalNames;
                    method = node.getName() + "/" + node.getParameters().size();
                    methodNode = node;
                    methodLocalNames = new HashSet<>();
                    if (node.getBody() != null) new TreeScanner<Void, Void>() {
                        @Override public Void visitVariable(VariableTree variable, Void ignored) {
                            methodLocalNames.add(variable.getName().toString());
                            return super.visitVariable(variable, ignored);
                        }
                    }.scan(node.getBody(), null);
                    methods.add(new Method(owner, node.getName().toString(), node.getParameters().size()));
                    try { return super.visitMethod(node, unused); }
                    finally {
                        method = previous;
                        methodNode = previousNode;
                        methodLocalNames = previousLocals;
                    }
                }

                private String receiverTypeHint(String receiver) {
                    if (receiver.startsWith("this.") && receiver.indexOf('.', 5) < 0 &&
                        ownerNode != null) {
                        String fieldName = receiver.substring(5);
                        for (Tree member : ownerNode.getMembers())
                            if (member instanceof VariableTree field &&
                                field.getName().contentEquals(fieldName))
                                return declaredType(field);
                    }
                    if (receiver.isEmpty() || receiver.contains(".") ||
                        receiver.equals("this") || receiver.equals("super")) return "";
                    long callStart = positions.getStartPosition(unit, getCurrentPath().getLeaf());
                    for (var path = getCurrentPath(); path != null; path = path.getParentPath()) {
                        Tree scope = path.getLeaf();
                        if (scope instanceof LambdaExpressionTree lambda)
                            for (VariableTree parameter : lambda.getParameters())
                                if (parameter.getName().contentEquals(receiver))
                                    return parameter.getType() == null ? "?" : declaredType(parameter);
                        if (scope instanceof BlockTree block)
                            for (StatementTree statement : block.getStatements())
                                if (statement instanceof VariableTree local &&
                                    local.getName().contentEquals(receiver) &&
                                    positions.getStartPosition(unit, local) < callStart)
                                    return declaredType(local);
                        if (scope instanceof ForLoopTree loop)
                            for (StatementTree initializer : loop.getInitializer())
                                if (initializer instanceof VariableTree local &&
                                    local.getName().contentEquals(receiver) &&
                                    positions.getEndPosition(unit, local) < callStart)
                                    return declaredType(local);
                        if (scope instanceof EnhancedForLoopTree loop &&
                            loop.getVariable().getName().contentEquals(receiver) &&
                            positions.getStartPosition(unit, loop.getStatement()) <= callStart)
                            return declaredType(loop.getVariable());
                        if (scope instanceof CatchTree clause &&
                            clause.getParameter().getName().contentEquals(receiver) &&
                            positions.getStartPosition(unit, clause.getBlock()) <= callStart)
                            return declaredType(clause.getParameter());
                        if (scope instanceof TryTree tryTree &&
                            positions.getStartPosition(unit, tryTree.getBlock()) <= callStart &&
                            callStart <= positions.getEndPosition(unit, tryTree.getBlock()))
                            for (Tree resource : tryTree.getResources())
                                if (resource instanceof VariableTree local &&
                                    local.getName().contentEquals(receiver))
                                    return declaredType(local);
                        if (scope == ownerNode) break;
                    }
                    // An unknown local declaration anywhere in the method can hide a
                    // field. Defer the whole name rather than assign a false type.
                    if (methodLocalNames.contains(receiver)) return "?";
                    if (methodNode != null) for (VariableTree parameter : methodNode.getParameters())
                        if (parameter.getName().contentEquals(receiver))
                            return declaredType(parameter);
                    if (ownerNode != null) for (Tree member : ownerNode.getMembers())
                        if (member instanceof VariableTree field && field.getName().contentEquals(receiver))
                            return declaredType(field);
                    return "";
                }

                private String declaredType(VariableTree variable) {
                    String name = identifierPath(variable.getType());
                    if (!name.isEmpty() && !name.equals("var")) return name;
                    if (variable.getInitializer() instanceof NewClassTree creation)
                        return identifierPath(creation.getIdentifier());
                    return "?";
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
                        receiverTypeHint(receiver),
                        name, node.getArguments().size()));
                    return super.visitMethodInvocation(node, unused);
                }

                @Override public Void visitNewClass(NewClassTree node, Void unused) {
                    Tree type = node.getIdentifier();
                    if (type instanceof ParameterizedTypeTree generic) type = generic.getType();
                    String target = identifierPath(type);
                    if (!target.isEmpty()) record(calls, new Call(owner + "#" + method,
                        "construct", target, "", "<init>", node.getArguments().size()));
                    return super.visitNewClass(node, unused);
                }
            }.scan(unit, null);
            StringBuilder out = new StringBuilder("{\"JavaSource\":").append(json(path))
                .append(",\"Package\":").append(json(pkg)).append(",\"Parents\":{");
            for (var entry : parents.entrySet()) {
                if (out.charAt(out.length() - 1) != '{') out.append(',');
                out.append(json(entry.getKey())).append(':').append('[');
                for (String base : entry.getValue()) {
                    if (out.charAt(out.length() - 1) != '[') out.append(',');
                    out.append(json(base));
                }
                out.append(']');
            }
            out.append("},\"Methods\":[");
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
                    .append(",\"receiverTypeHint\":").append(json(c.receiverTypeHint))
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
        if (expression instanceof ParameterizedTypeTree generic)
            return identifierPath(generic.getType());
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
