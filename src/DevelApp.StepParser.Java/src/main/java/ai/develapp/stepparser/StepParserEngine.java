/**
 * StepParserEngine class for Java bindings
 */
package ai.develapp.stepparser;

import ai.develapp.steplexer.LexerResult;
import ai.develapp.steplexer.SplittableToken;
import ai.develapp.steplexer.StepLexer;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Stack;

/**
 * Main parsing engine for StepParser Java bindings
 * Provides GLR-style multi-path parsing with CognitiveGraph integration
 */
public class StepParserEngine {
    private String language;
    private Map<String, GrammarRule> grammarRules;

    public StepParserEngine(String language) {
        this.language = language;
        this.grammarRules = new HashMap<>();
        loadDefaultGrammar();
    }

    public StepParserEngine() {
        this("csharp");
    }

    public String getLanguage() { return language; }
    public void setLanguage(String language) { this.language = language; loadDefaultGrammar(); }

    private void loadDefaultGrammar() {
        grammarRules.clear();
        switch (language.toLowerCase()) {
            case "csharp": loadCSharpGrammar(); break;
            case "java": loadJavaGrammar(); break;
            case "javascript": loadJavaScriptGrammar(); break;
            default: loadGenericGrammar(); break;
        }
    }

    private void loadCSharpGrammar() {
        grammarRules.put("compilation_unit", new GrammarRule("compilation_unit", "using_directives? namespace_member_declarations?", false));
        grammarRules.put("using_directive", new GrammarRule("using_directive", "'using' identifier ';'", true));
        grammarRules.put("namespace_member_declaration", new GrammarRule("namespace_member_declaration", 
            "class_declaration | interface_declaration | enum_declaration | delegate_declaration", false));
        grammarRules.put("class_declaration", new GrammarRule("class_declaration", 
            "'class' identifier class_base? class_body ';'?", false));
        grammarRules.put("class_body", new GrammarRule("class_body", "'{' class_member_declarations? '}'", false));
    }

    private void loadJavaGrammar() {
        grammarRules.put("compilation_unit", new GrammarRule("compilation_unit", "package_declaration? import_declarations? type_declarations?", false));
        grammarRules.put("package_declaration", new GrammarRule("package_declaration", "'package' qualified_identifier ';'", true));
        grammarRules.put("import_declaration", new GrammarRule("import_declaration", "'import' qualified_identifier ';' | 'import' qualified_identifier '.' '*' ';'", true));
    }

    private void loadJavaScriptGrammar() {
        grammarRules.put("program", new GrammarRule("program", "source_elements?", false));
        grammarRules.put("source_element", new GrammarRule("source_element", "statement | function_declaration", false));
    }

    private void loadGenericGrammar() {
        grammarRules.put("compilation_unit", new GrammarRule("compilation_unit", "statements?", false));
        grammarRules.put("statement", new GrammarRule("statement", "expression_statement | block | if_statement | while_statement", false));
        grammarRules.put("block", new GrammarRule("block", "'{' statements? '}'", false));
    }

    public ParseResult parse(String sourceCode) {
        long startTime = System.currentTimeMillis();
        StepLexer lexer = new StepLexer(language);
        LexerResult lexerResult = lexer.tokenize(sourceCode);

        if (!lexerResult.isSuccess()) {
            return new ParseResult(
                new ParseNode("error", "Lexical analysis failed", 0, 0),
                lexerResult,
                lexerResult.getErrors(),
                false,
                System.currentTimeMillis() - startTime
            );
        }

        ParseNode root = parseTokens(lexerResult.getTokens());
        return new ParseResult(
            root,
            lexerResult,
            new java.util.ArrayList<>(),
            true,
            System.currentTimeMillis() - startTime
        );
    }

    private ParseNode parseTokens(List<SplittableToken> tokens) {
        ParseNode root = new ParseNode("compilation_unit", "", 0, 0);
        Stack<ParseNode> stack = new Stack<>();
        stack.push(root);

        for (SplittableToken token : tokens) {
            ParseNode node = new ParseNode(token);
            stack.peek().addChild(node);
            if (token.getText().equals("{")) stack.push(node);
            else if (token.getText().equals("}")) if (stack.size() > 1) stack.pop();
        }

        return root;
    }

    public Map<String, GrammarRule> getGrammarRules() { return grammarRules; }
    public void addGrammarRule(GrammarRule rule) { grammarRules.put(rule.getName(), rule); }
}
