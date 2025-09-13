using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Represents a precise location in source code for surgical operations
    /// </summary>
    public interface ICodeLocation
    {
        /// <summary>
        /// Gets the file path
        /// </summary>
        string File { get; }
        
        /// <summary>
        /// Gets the starting line number
        /// </summary>
        int StartLine { get; }
        
        /// <summary>
        /// Gets the starting column number
        /// </summary>
        int StartColumn { get; }
        
        /// <summary>
        /// Gets the ending line number
        /// </summary>
        int EndLine { get; }
        
        /// <summary>
        /// Gets the ending column number
        /// </summary>
        int EndColumn { get; }
        
        /// <summary>
        /// Gets the context information
        /// </summary>
        string Context { get; }
    }

    /// <summary>
    /// Implementation of precise code location
    /// </summary>
    public class CodeLocation : ICodeLocation
    {
        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string File { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the starting line number
        /// </summary>
        public int StartLine { get; set; }
        
        /// <summary>
        /// Gets or sets the starting column number
        /// </summary>
        public int StartColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the ending line number
        /// </summary>
        public int EndLine { get; set; }
        
        /// <summary>
        /// Gets or sets the ending column number
        /// </summary>
        public int EndColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the context information
        /// </summary>
        public string Context { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the CodeLocation class
        /// </summary>
        public CodeLocation() { }

        /// <summary>
        /// Initializes a new instance of the CodeLocation class with specified parameters
        /// </summary>
        /// <param name="file">The file path</param>
        /// <param name="startLine">The starting line number</param>
        /// <param name="startColumn">The starting column number</param>
        /// <param name="endLine">The ending line number</param>
        /// <param name="endColumn">The ending column number</param>
        /// <param name="context">The context information</param>
        public CodeLocation(string file, int startLine, int startColumn, int endLine, int endColumn, string context = "")
        {
            File = file;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Context = context;
        }

        /// <summary>
        /// Returns a string representation of the code location
        /// </summary>
        /// <returns>A string in the format [file line:column-line:column]</returns>
        public override string ToString()
        {
            return $"[{File} {StartLine}:{StartColumn}-{EndLine}:{EndColumn}]";
        }
    }

    /// <summary>
    /// Context stack for hierarchical context management
    /// </summary>
    public interface IContextStack
    {
        /// <summary>
        /// Pushes a new context onto the stack
        /// </summary>
        /// <param name="context">The context name</param>
        /// <param name="identifier">Optional identifier for the context</param>
        void Push(string context, string? identifier = null);
        
        /// <summary>
        /// Pops the current context from the stack
        /// </summary>
        /// <returns>The popped context name, or null if stack is empty</returns>
        string? Pop();
        
        /// <summary>
        /// Gets the current context without removing it
        /// </summary>
        /// <returns>The current context name, or null if stack is empty</returns>
        string? Current();
        
        /// <summary>
        /// Gets the full path from root to current context
        /// </summary>
        /// <returns>Array of context names from root to current</returns>
        string[] GetPath();
        
        /// <summary>
        /// Checks if currently in the specified scope
        /// </summary>
        /// <param name="scope">The scope to check</param>
        /// <returns>True if in scope, false otherwise</returns>
        bool InScope(string scope);
        
        /// <summary>
        /// Gets the current depth of the context stack
        /// </summary>
        /// <returns>The number of contexts in the stack</returns>
        int Depth();
        
        /// <summary>
        /// Checks if the stack contains the specified context
        /// </summary>
        /// <param name="context">The context to look for</param>
        /// <returns>True if context is found, false otherwise</returns>
        bool Contains(string context);
    }

    /// <summary>
    /// Implementation of hierarchical context stack
    /// </summary>
    public class ContextStack : IContextStack
    {
        private readonly Stack<ContextFrame> _stack = new();

        private class ContextFrame
        {
            public string Context { get; set; } = string.Empty;
            public string? Identifier { get; set; }

            public ContextFrame(string context, string? identifier = null)
            {
                Context = context;
                Identifier = identifier;
            }
        }

        /// <summary>
        /// Pushes a new context onto the stack
        /// </summary>
        /// <param name="context">The context name</param>
        /// <param name="identifier">Optional identifier for the context</param>
        public void Push(string context, string? identifier = null)
        {
            _stack.Push(new ContextFrame(context, identifier));
        }

        /// <summary>
        /// Pops the current context from the stack
        /// </summary>
        /// <returns>The popped context name, or null if stack is empty</returns>
        public string? Pop()
        {
            return _stack.Count > 0 ? _stack.Pop().Context : null;
        }

        /// <summary>
        /// Gets the current context without removing it
        /// </summary>
        /// <returns>The current context name, or null if stack is empty</returns>
        public string? Current()
        {
            return _stack.Count > 0 ? _stack.Peek().Context : null;
        }

        /// <summary>
        /// Gets the full path from root to current context
        /// </summary>
        /// <returns>Array of context names from root to current</returns>
        public string[] GetPath()
        {
            var path = new List<string>();
            foreach (var frame in _stack)
            {
                path.Add(frame.Context);
            }
            path.Reverse();
            return path.ToArray();
        }

        /// <summary>
        /// Checks if currently in the specified scope
        /// </summary>
        /// <param name="scope">The scope to check</param>
        /// <returns>True if in scope, false otherwise</returns>
        public bool InScope(string scope)
        {
            return _stack.Any(frame => frame.Context == scope);
        }

        /// <summary>
        /// Gets the current depth of the context stack
        /// </summary>
        /// <returns>The number of contexts in the stack</returns>
        public int Depth()
        {
            return _stack.Count;
        }

        /// <summary>
        /// Checks if the stack contains the specified context
        /// </summary>
        /// <param name="context">The context to look for</param>
        /// <returns>True if context is found, false otherwise</returns>
        public bool Contains(string context)
        {
            return _stack.Any(frame => frame.Context == context);
        }
    }

    /// <summary>
    /// Symbol information for scope-aware symbol tracking
    /// </summary>
    public class SymbolInfo
    {
        /// <summary>
        /// Gets or sets the symbol name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the symbol type
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the scope in which the symbol is declared
        /// </summary>
        public string Scope { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the location where the symbol is declared
        /// </summary>
        public ICodeLocation Location { get; set; } = new CodeLocation();
        
        /// <summary>
        /// Gets or sets whether this symbol can be inlined
        /// </summary>
        public bool CanInline { get; set; }
        
        /// <summary>
        /// Gets or sets the symbol value (for constants)
        /// </summary>
        public string? Value { get; set; }
        
        /// <summary>
        /// Gets or sets the list of references to this symbol
        /// </summary>
        public List<Reference> References { get; set; } = new();
    }

    /// <summary>
    /// Reference to a symbol
    /// </summary>
    public class Reference
    {
        /// <summary>
        /// Gets or sets the location of the reference
        /// </summary>
        public ICodeLocation Location { get; set; } = new CodeLocation();
        
        /// <summary>
        /// Gets or sets the scope in which the reference occurs
        /// </summary>
        public string Scope { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the usage type (read, write, call, etc.)
        /// </summary>
        public string Usage { get; set; } = string.Empty; // read, write, call, etc.
    }

    /// <summary>
    /// Scope-aware symbol table for tracking declarations and references
    /// </summary>
    public interface IScopeAwareSymbolTable
    {
        /// <summary>
        /// Declares a new symbol in the specified scope
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="type">The symbol type</param>
        /// <param name="scope">The scope in which to declare the symbol</param>
        /// <param name="location">The location of the declaration</param>
        void Declare(string symbol, string type, string scope, ICodeLocation location);
        
        /// <summary>
        /// Looks up a symbol in the specified scope
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="scope">The scope to search</param>
        /// <returns>The symbol information, or null if not found</returns>
        SymbolInfo? Lookup(string symbol, string scope);
        
        /// <summary>
        /// Gets all symbols declared in the specified scope
        /// </summary>
        /// <param name="scope">The scope to search</param>
        /// <returns>Array of symbols in the scope</returns>
        SymbolInfo[] GetSymbolsInScope(string scope);
        
        /// <summary>
        /// Finds all references to the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <returns>Array of references to the symbol</returns>
        Reference[] FindAllReferences(string symbol);
        
        /// <summary>
        /// Adds a reference to the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="scope">The scope in which the reference occurs</param>
        /// <param name="location">The location of the reference</param>
        /// <param name="usage">The usage type (read, write, call, etc.)</param>
        void AddReference(string symbol, string scope, ICodeLocation location, string usage = "read");
    }

    /// <summary>
    /// Implementation of scope-aware symbol table
    /// </summary>
    public class ScopeAwareSymbolTable : IScopeAwareSymbolTable
    {
        private readonly Dictionary<string, SymbolInfo> _symbols = new();
        private readonly Dictionary<string, List<SymbolInfo>> _scopeIndex = new();

        /// <summary>
        /// Declares a new symbol in the specified scope
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="type">The symbol type</param>
        /// <param name="scope">The scope in which to declare the symbol</param>
        /// <param name="location">The location of the declaration</param>
        public void Declare(string symbol, string type, string scope, ICodeLocation location)
        {
            var key = $"{scope}::{symbol}";
            var symbolInfo = new SymbolInfo
            {
                Name = symbol,
                Type = type,
                Scope = scope,
                Location = location
            };

            _symbols[key] = symbolInfo;

            if (!_scopeIndex.ContainsKey(scope))
                _scopeIndex[scope] = new List<SymbolInfo>();
            
            _scopeIndex[scope].Add(symbolInfo);
        }

        /// <summary>
        /// Looks up a symbol in the specified scope and parent scopes
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="scope">The scope to search</param>
        /// <returns>The symbol information, or null if not found</returns>
        public SymbolInfo? Lookup(string symbol, string scope)
        {
            // Try current scope first, then walk up the scope hierarchy
            var currentScope = scope;
            while (!string.IsNullOrEmpty(currentScope))
            {
                var key = $"{currentScope}::{symbol}";
                if (_symbols.TryGetValue(key, out var symbolInfo))
                    return symbolInfo;

                // Move to parent scope (simplified - could be more sophisticated)
                var lastDot = currentScope.LastIndexOf('.');
                currentScope = lastDot > 0 ? currentScope.Substring(0, lastDot) : string.Empty;
            }

            // Try global scope
            var globalKey = $"::{symbol}";
            return _symbols.TryGetValue(globalKey, out var globalSymbol) ? globalSymbol : null;
        }

        /// <summary>
        /// Gets all symbols declared in the specified scope
        /// </summary>
        /// <param name="scope">The scope to search</param>
        /// <returns>Array of symbols in the scope</returns>
        public SymbolInfo[] GetSymbolsInScope(string scope)
        {
            return _scopeIndex.TryGetValue(scope, out var symbols) ? symbols.ToArray() : Array.Empty<SymbolInfo>();
        }

        /// <summary>
        /// Finds all references to the specified symbol across all scopes
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <returns>Array of references to the symbol</returns>
        public Reference[] FindAllReferences(string symbol)
        {
            var references = new List<Reference>();
            foreach (var symbolInfo in _symbols.Values)
            {
                if (symbolInfo.Name == symbol)
                {
                    references.AddRange(symbolInfo.References);
                }
            }
            return references.ToArray();
        }

        /// <summary>
        /// Adds a reference to the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol name</param>
        /// <param name="scope">The scope in which the reference occurs</param>
        /// <param name="location">The location of the reference</param>
        /// <param name="usage">The usage type (read, write, call, etc.)</param>
        public void AddReference(string symbol, string scope, ICodeLocation location, string usage = "read")
        {
            var symbolInfo = Lookup(symbol, scope);
            if (symbolInfo != null)
            {
                symbolInfo.References.Add(new Reference
                {
                    Location = location,
                    Scope = scope,
                    Usage = usage
                });
            }
        }
    }
}