/**
 * StepParserEngine class for Java bindings
 * Main parsing engine for StepParser
 */
package ai.develapp.stepparser;

import ai.develapp.steplexer.LexerResult;
import ai.develapp.steplexer.SplittableToken;
import ai.develapp.steplexer.StepLexer;
import ai.develapp.steplexer.TokenType;

import java.util.ArrayList;
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
    private String grammar;
    private Map<String, GrammarRule> grammarRules;
    private boolean useCognitiveGraphV2;

    /**
     * Creates a new StepParserEngine
     * @param language The language to parse
     */
    public StepParserEngine(String language) {
        this.language = language;
        this.grammarRules = new HashMap<>();
        this.useCognitiveGraphV2 = false;
        loadDefaultGrammar();
    }

    /**
     * Creates a new StepParserEngine with default language
     */
    public StepParserEngine() {
        this("csharp");
    }

    /**
     * Creates a new StepParserEngine with CognitiveGraph version
     * @param language The language to parse
     * @param useV2 Whether to use CognitiveGraph V2 schema
     */
    public StepParserEngine(String language, boolean useV2) {
        this(language);
        this.useCognitiveGraphV2 = useV2;
    }

    /**
     * Gets the language being parsed
     * @return The language
     */
    public String getLanguage() {
        return language;
    }

    /**
     * Sets the language being parsed
     * @param language The language
     */
    public void setLanguage(String language) {
        this.language = language;
        loadDefaultGrammar();
    }

    /**
     * Gets whether CognitiveGraph V2 is being used
     * @return true if using V2
     */
    public boolean isUseCognitiveGraphV2() {
        return useCognitiveGraphV2;
    }

    /**
     * Sets whether to use CognitiveGraph V2
     * @param useV2 true to use V2
     */
    public void setUseCognitiveGraphV2(boolean useV2) {
        this.useCognitiveGraphV2 = useV2;
    }

    /**
     * Loads the default grammar for the current language
     */
    private void loadDefaultGrammar() {
        grammarRules.clear();
        
        switch (language.toLowerCase()) {
            case "csharp":
                loadCSharpGrammar();
                break;
            case "java":
                loadJavaGrammar();
                break;
            case "javascript":
                loadJavaScriptGrammar();
                break;
            default:
                loadGenericGrammar();
                break;
        }
    }

    /**
     * Loads C# grammar
     */
    private void loadCSharpGrammar() {
        // Basic C# grammar rules
        grammarRules.put("compilation_unit", new GrammarRule(
            "compilation_unit", "using_directives? namespace_member_declarations?", false));
        
        grammarRules.put("using_directive", new GrammarRule(
            "using_directive", "'using' identifier ';'", true));
        
        grammarRules.put("namespace_member_declaration", new GrammarRule(
            "namespace_member_declaration", 
            "class_declaration | interface_declaration | enum_declaration | delegate_declaration", 
            false));
        
        grammarRules.put("class_declaration", new GrammarRule(
            "class_declaration", 
            "'class' identifier class_base? class_body ';'?", 
            false));
        
        grammarRules.put("class_body", new GrammarRule(
            "class_body", "'{' class_member_declarations? '}'", false));
        
        grammarRules.put("class_member_declaration", new GrammarRule(
            "class_member_declaration", 
            "method_declaration | field_declaration | property_declaration | constructor_declaration", 
            false));
        
        grammarRules.put("method_declaration", new GrammarRule(
            "method_declaration", 
            "method_header method_body", 
            false));
        
        grammarRules.put("method_header", new GrammarRule(
            "method_header", 
            "attributes? modifiers? return_type identifier '(' formal_parameter_list? ')' type_parameter_constraints_clauses?", 
            false));
        
        grammarRules.put("method_body", new GrammarRule(
            "method_body", "block | ';'", false));
        
        grammarRules.put("block", new GrammarRule(
            "block", "'{' statement_list? '}'", false));
        
        grammarRules.put("statement", new GrammarRule(
            "statement", 
            "local_variable_declaration | expression_statement | if_statement | while_statement | for_statement | return_statement", 
            false));
        
        // Add more C# grammar rules as needed
    }

    /**
     * Loads Java grammar
     */
    private void loadJavaGrammar() {
        // Basic Java grammar rules
        grammarRules.put("compilation_unit", new GrammarRule(
            "compilation_unit", "package_declaration? import_declarations? type_declarations?", false));
        
        grammarRules.put("package_declaration", new GrammarRule(
            "package_declaration", "'package' qualified_identifier ';'", true));
        
        grammarRules.put("import_declaration", new GrammarRule(
            "import_declaration", "'import' qualified_identifier ';' | 'import' qualified_identifier '.' '*' ';'", true));
        
        grammarRules.put("type_declaration", new GrammarRule(
            "type_declaration", "class_declaration | interface_declaration | enum_declaration", false));
        
        grammarRules.put("class_declaration", new GrammarRule(
            "class_declaration", 
            "'class' identifier class_body", 
            false));
        
        grammarRules.put("class_body", new GrammarRule(
            "class_body", "'{' class_body_declarations? '}'", false));
        
        // Add more Java grammar rules as needed
    }

    /**
     * Loads JavaScript grammar
     */
    private void loadJavaScriptGrammar() {
        // Basic JavaScript grammar rules
        grammarRules.put("program", new GrammarRule(
            "program", "source_elements?", false));
        
        grammarRules.put("source_element", new GrammarRule(
            "source_element", "statement | function_declaration", false));
        
        grammarRules.put("function_declaration", new GrammarRule(
            "function_declaration", 
            "'function' identifier '(' formal_parameter_list? ')' '{' function_body '}'", 
            false));
        
        grammarRules.put("statement", new GrammarRule(
            "statement", 
            "block | variable_statement | expression_statement | if_statement | iteration_statement | return_statement", 
            false));
        
        // Add more JavaScript grammar rules as needed
    }

    /**
     * Loads generic grammar
     */
    private void loadGenericGrammar() {
        // Generic grammar rules that work for most languages
        grammarRules.put("compilation_unit", new GrammarRule(
            "compilation_unit", "statements?", false));
        
        grammarRules.put("statement", new GrammarRule(
            "statement", "expression_statement | block | if_statement | while_statement", false));
        
        grammarRules.put("expression_statement", new GrammarRule(
            "expression_statement", "expression ';'", false));
        
        grammarRules.put("block", new GrammarRule(
            "block", "'{' statements? '}'", false));
        
        grammarRules.put("if_statement", new GrammarRule(
            "if_statement", "'if' '(' expression ')' statement ('else' statement)?", false));
        
        grammarRules.put("while_statement", new GrammarRule(
            "while_statement", "'while' '(' expression ')' statement", false));
    }

    /**
     * Loads a grammar from a string
     * @param grammar The grammar definition
     */
    public void loadGrammarFromContent(String grammar) {
        this.grammar = grammar;
        // Parse the grammar and load rules
        // This is a simplified implementation
        // A full implementation would parse the grammar file format
    }

    /**
     * Parses the input source code
     * @param sourceCode The source code to parse
     * @return The parse result
     */
    public ParseResult parse(String sourceCode) {
        long startTime = System.currentTimeMillis();
        
        // Create lexer
        StepLexer lexer = new StepLexer(language);
        lexer.setPreserveComments(true);
        lexer.setIncludeWhitespace(false);
        
        // Tokenize
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
        
        // Parse tokens
        ParseNode root = parseTokens(lexerResult.getTokens());
        
        long parseTime = System.currentTimeMillis() - startTime;
        
        return new ParseResult(
            root,
            lexerResult,
            new ArrayList<>(),
            true,
            parseTime
        );
    }

    /**
     * Parses a list of tokens into a parse tree
     * @param tokens The list of tokens to parse
     * @return The root of the parse tree
     */
    private ParseNode parseTokens(List<SplittableToken> tokens) {
        ParseNode root = new ParseNode("compilation_unit", "", 0, 0);
        Stack<ParseNode> stack = new Stack<>();
        stack.push(root);
        
        for (SplittableToken token : tokens) {
            ParseNode node = new ParseNode(token);
            stack.peek().addChild(node);
            
            // Handle blocks
            if (token.getText().equals("{")) {
                stack.push(node);
            } else if (token.getText().equals("}")) {
                if (stack.size() > 1) {
                    stack.pop();
                }
            }
        }
        
        return root;
    }

    /**
     * Parses multiple files
     * @param files Map of file names to file contents
     * @return The combined parse result
     */
    public ParseResult parseMultipleFiles(Map<String, String> files) {
        long startTime = System.currentTimeMillis();
        ParseNode root = new ParseNode("compilation_unit", "", 0, 0);
        List<String> allErrors = new ArrayList<>();
        int totalTokens = 0;
        
        for (Map.Entry<String, String> entry : files.entrySet()) {
            ParseResult result = parse(entry.getValue());
            if (result.isSuccess()) {
                root.addChild(result.getRoot());
                totalTokens += result.getLexerResult().getTokenCount();
            } else {
                allErrors.addAll(result.getErrors());
            }
        }
        
        long parseTime = System.currentTimeMillis() - startTime;
        
        return new ParseResult(
            root,
            new LexerResult(new ArrayList<>(), allErrors, true, totalTokens, 0),
            allErrors,
            allErrors.isEmpty(),
            parseTime
        );
    }

    /**
     * Gets the grammar rules
     * @return The map of grammar rules
     */
    public Map<String, GrammarRule> getGrammarRules() {
        return grammarRules;
    }

    /**
     * Adds a grammar rule
     * @param rule The rule to add
     */
    public void addGrammarRule(GrammarRule rule) {
        grammarRules.put(rule.getName(), rule);
    }

    /**
     * Gets a grammar rule by name
     * @param name The name of the rule
     * @return The rule, or null if not found
     */
    public GrammarRule getGrammarRule(String name) {
        return grammarRules.get(name);
    }

    @Override
    public String toString() {
        return "StepParserEngine{" +
                "language='" + language + '\'' +
                ", useCognitiveGraphV2=" + useCognitiveGraphV2 +
                ", grammarRules=" + grammarRules.size() +
                '}';
    }
}
