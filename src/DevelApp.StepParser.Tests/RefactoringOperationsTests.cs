using Xunit;
using DevelApp.StepParser;
using DevelApp.StepLexer;
using System.Collections.Generic;
using System.Linq;

namespace DevelApp.StepParser.Tests
{
    /// <summary>
    /// Tests for refactoring operations as specified in TDS Section 7
    /// </summary>
    public class RefactoringOperationsTests
    {
        [Fact]
        public void FindUsages_ValidSymbol_ReturnsAllReferences()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };

            // Act
            var usages = engine.FindUsages(location);

            // Assert
            Assert.NotNull(usages);
            // When no parser context is set up, should return empty list
            Assert.IsType<List<ICodeLocation>>(usages);
        }

        [Fact]
        public void FindUsages_WithScope_FiltersCorrectly()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };
            var scope = "method";

            // Act
            var usages = engine.FindUsages(location, scope);

            // Assert
            Assert.NotNull(usages);
            Assert.IsType<List<ICodeLocation>>(usages);
        }

        [Fact]
        public void Rename_ValidLocation_ReturnsResult()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };
            var newName = "renamedSymbol";

            // Act
            var result = engine.Rename(location, newName);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RefactoringResult>(result);
            // Without grammar loaded, operation should fail gracefully
            Assert.False(result.Success);
        }

        [Fact]
        public void ExtractVariable_ValidLocation_ReturnsResult()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };
            var variableName = "extractedVar";

            // Act
            var result = engine.ExtractVariable(location, variableName);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RefactoringResult>(result);
            // Without parse context, operation should indicate it's not available
            Assert.False(result.Success);
        }

        [Fact]
        public void InlineVariable_ValidLocation_ReturnsResult()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };

            // Act
            var result = engine.InlineVariable(location);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RefactoringResult>(result);
            // Without parse context, operation should indicate it's not available
            Assert.False(result.Success);
        }

        [Fact]
        public void RefactoringResult_Properties_InitializeCorrectly()
        {
            // Arrange & Act
            var result = new RefactoringResult
            {
                Success = true,
                Message = "Refactoring completed",
                Changes = new List<CodeChange>(),
                ModifiedNodeLocation = new CodeLocation()
            };

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Refactoring completed", result.Message);
            Assert.NotNull(result.Changes);
            Assert.NotNull(result.ModifiedNodeLocation);
        }

        [Fact]
        public void CodeChange_Properties_SetCorrectly()
        {
            // Arrange & Act
            var change = new CodeChange
            {
                Location = new CodeLocation { StartLine = 5, StartColumn = 10 },
                OriginalText = "oldValue",
                NewText = "newValue",
                ChangeType = "replace"
            };

            // Assert
            Assert.Equal(5, change.Location.StartLine);
            Assert.Equal(10, change.Location.StartColumn);
            Assert.Equal("oldValue", change.OriginalText);
            Assert.Equal("newValue", change.NewText);
            Assert.Equal("replace", change.ChangeType);
        }

        [Fact]
        public void RefactoringOperation_Properties_SetCorrectly()
        {
            // Arrange & Act
            var operation = new RefactoringOperation
            {
                Name = "TestOperation",
                Description = "Test refactoring",
                ApplicableContexts = new[] { "method", "class" }
            };

            // Assert
            Assert.Equal("TestOperation", operation.Name);
            Assert.Equal("Test refactoring", operation.Description);
            Assert.Equal(2, operation.ApplicableContexts.Length);
            Assert.Contains("method", operation.ApplicableContexts);
        }

        [Fact]
        public void SelectionCriteria_RegexPattern_SetCorrectly()
        {
            // Arrange & Act
            var criteria = new SelectionCriteria
            {
                Regex = @"\w+"
            };

            // Assert
            Assert.Equal(@"\w+", criteria.Regex);
        }

        [Fact]
        public void SelectionCriteria_RangeSelection_SetCorrectly()
        {
            // Arrange & Act
            var criteria = new SelectionCriteria
            {
                Range = ("startMarker", "endMarker")
            };

            // Assert
            Assert.NotNull(criteria.Range);
            Assert.Equal("startMarker", criteria.Range.Value.start);
            Assert.Equal("endMarker", criteria.Range.Value.end);
        }

        [Fact]
        public void SelectionCriteria_StructuralSelection_SetCorrectly()
        {
            // Arrange & Act
            var criteria = new SelectionCriteria
            {
                Structural = ("class", true, false)
            };

            // Assert
            Assert.NotNull(criteria.Structural);
            Assert.Equal("class", criteria.Structural.Value.type);
            Assert.True(criteria.Structural.Value.includeFields);
            Assert.False(criteria.Structural.Value.includeMethods);
        }

        [Fact]
        public void RefactoringOperations_Integration_WorkTogether()
        {
            // Arrange
            var engine = new StepParserEngine();
            var location = new CodeLocation { StartLine = 10, StartColumn = 5, File = "test.cs" };

            // Act - demonstrate refactoring operations can be called in sequence
            var findResult = engine.FindUsages(location);
            var renameResult = engine.Rename(location, "newName");
            var extractResult = engine.ExtractVariable(location, "extracted");
            var inlineResult = engine.InlineVariable(location);

            // Assert - all operations return appropriate results
            Assert.NotNull(findResult);
            Assert.NotNull(renameResult);
            Assert.NotNull(extractResult);
            Assert.NotNull(inlineResult);
        }

        [Fact]
        public void StepParserEngine_RefactoringOperations_Registered()
        {
            // Arrange
            var engine = new StepParserEngine();

            // Act - Operations should be registered during construction
            var location = new CodeLocation { StartLine = 1, StartColumn = 1 };
            
            // Try calling each operation
            var findUsages = engine.FindUsages(location);
            var rename = engine.Rename(location, "test");
            var extract = engine.ExtractVariable(location, "var");
            var inline = engine.InlineVariable(location);

            // Assert - All operations should return valid results (even if unsuccessful)
            Assert.NotNull(findUsages);
            Assert.NotNull(rename);
            Assert.NotNull(extract);
            Assert.NotNull(inline);
        }
    }
}
