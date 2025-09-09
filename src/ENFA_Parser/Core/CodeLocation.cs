using System;
using System.Collections.Generic;
using System.Linq;

namespace ENFA_Parser.Core
{
    /// <summary>
    /// Represents a precise location in source code for surgical operations
    /// </summary>
    public interface ICodeLocation
    {
        string File { get; }
        int StartLine { get; }
        int StartColumn { get; }
        int EndLine { get; }
        int EndColumn { get; }
        string Context { get; }
    }

    /// <summary>
    /// Implementation of precise code location
    /// </summary>
    public class CodeLocation : ICodeLocation
    {
        public string File { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public string Context { get; set; } = string.Empty;

        public CodeLocation() { }

        public CodeLocation(string file, int startLine, int startColumn, int endLine, int endColumn, string context = "")
        {
            File = file;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Context = context;
        }

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
        void Push(string context, string? identifier = null);
        string? Pop();
        string? Current();
        string[] GetPath();
        bool InScope(string scope);
        int Depth();
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

        public void Push(string context, string? identifier = null)
        {
            _stack.Push(new ContextFrame(context, identifier));
        }

        public string? Pop()
        {
            return _stack.Count > 0 ? _stack.Pop().Context : null;
        }

        public string? Current()
        {
            return _stack.Count > 0 ? _stack.Peek().Context : null;
        }

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

        public bool InScope(string scope)
        {
            return _stack.Any(frame => frame.Context == scope);
        }

        public int Depth()
        {
            return _stack.Count;
        }

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
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public ICodeLocation Location { get; set; } = new CodeLocation();
        public bool CanInline { get; set; }
        public string? Value { get; set; }
        public List<Reference> References { get; set; } = new();
    }

    /// <summary>
    /// Reference to a symbol
    /// </summary>
    public class Reference
    {
        public ICodeLocation Location { get; set; } = new CodeLocation();
        public string Scope { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty; // read, write, call, etc.
    }

    /// <summary>
    /// Scope-aware symbol table for tracking declarations and references
    /// </summary>
    public interface IScopeAwareSymbolTable
    {
        void Declare(string symbol, string type, string scope, ICodeLocation location);
        SymbolInfo? Lookup(string symbol, string scope);
        SymbolInfo[] GetSymbolsInScope(string scope);
        Reference[] FindAllReferences(string symbol);
        void AddReference(string symbol, string scope, ICodeLocation location, string usage = "read");
    }

    /// <summary>
    /// Implementation of scope-aware symbol table
    /// </summary>
    public class ScopeAwareSymbolTable : IScopeAwareSymbolTable
    {
        private readonly Dictionary<string, SymbolInfo> _symbols = new();
        private readonly Dictionary<string, List<SymbolInfo>> _scopeIndex = new();

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

        public SymbolInfo[] GetSymbolsInScope(string scope)
        {
            return _scopeIndex.TryGetValue(scope, out var symbols) ? symbols.ToArray() : Array.Empty<SymbolInfo>();
        }

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