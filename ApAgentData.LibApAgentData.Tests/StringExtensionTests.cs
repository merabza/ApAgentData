using Xunit;

namespace ApAgentData.LibApAgentData.Tests;

public class StringExtensionTests
{
    #region PrepareFileName Tests

    [Fact]
    public void PrepareFileName_RemovesAllRestrictedSymbols()
    {
        // Arrange
        const string fileName = "test<>:\"/\\|?*'«»file.txt";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("testfile.txt", result);
    }

    [Fact]
    public void PrepareFileName_LeavesValidCharactersUnchanged()
    {
        // Arrange
        const string fileName = "valid_file-name.123.txt";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("valid_file-name.123.txt", result);
    }

    [Fact]
    public void PrepareFileName_HandlesEmptyString()
    {
        // Arrange
        const string fileName = "";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void PrepareFileName_HandlesStringWithOnlyRestrictedSymbols()
    {
        // Arrange
        const string fileName = "<>:\"/\\|?*'«»";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void PrepareFileName_HandlesFileNameWithSpaces()
    {
        // Arrange
        const string fileName = "my file name.txt";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("my file name.txt", result);
    }

    [Fact]
    public void PrepareFileName_RemovesMultipleOccurrencesOfSameSymbol()
    {
        // Arrange
        const string fileName = "test<test>test:test.txt";

        // Act
        string result = fileName.PrepareFileName();

        // Assert
        Assert.Equal("testtesttesttest.txt", result);
    }

    #endregion

    #region PreparedFileNameConsideringLength Tests

    [Fact]
    public void PreparedFileNameConsideringLength_HandlesFileNameUnderMaxLength()
    {
        // Arrange
        const string fileName = "shortfile.txt";
        const int maxLength = 50;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.Equal("shortfile.txt", result);
    }

    [Fact]
    public void PreparedFileNameConsideringLength_TruncatesLongFileName()
    {
        // Arrange
        const string fileName = "verylongfilenamethatshouldbetruncated.txt";
        const int maxLength = 20;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void PreparedFileNameConsideringLength_RemovesRestrictedSymbolsAndTruncates()
    {
        // Arrange
        const string fileName = "test<file>:name|with*symbols.txt";
        const int maxLength = 15;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain("|", result);
        Assert.DoesNotContain("*", result);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void PreparedFileNameConsideringLength_TrimsWhitespace()
    {
        // Arrange
        const string fileName = "  test file.txt  ";
        const int maxLength = 50;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.Equal("test file.txt", result);
    }

    [Fact]
    public void PreparedFileNameConsideringLength_HandlesFileWithoutExtension()
    {
        // Arrange
        const string fileName = "testfile";
        const int maxLength = 5;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
    }

    [Fact]
    public void PreparedFileNameConsideringLength_HandlesVeryShortMaxLength()
    {
        // Arrange
        const string fileName = "testfile.txt";
        const int maxLength = 5;

        // Act
        string result = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
    }

    #endregion

    #region GetNewFileName Tests

    [Fact]
    public void GetNewFileName_ReturnsFileNameWithoutCounterWhenIndexIsZero()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(0, fileExtension);

        // Assert
        Assert.Equal("testfile.txt", result);
    }

    [Fact]
    public void GetNewFileName_ReturnsFileNameWithCounterWhenIndexIsOne()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(1, fileExtension);

        // Assert
        Assert.Equal("testfile(1).txt", result);
    }

    [Fact]
    public void GetNewFileName_ReturnsFileNameWithCounterWhenIndexIsGreaterThanOne()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(5, fileExtension);

        // Assert
        Assert.Equal("testfile(5).txt", result);
    }

    [Fact]
    public void GetNewFileName_HandlesEmptyExtension()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = "";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(3, fileExtension);

        // Assert
        Assert.Equal("testfile(3)", result);
    }

    [Fact]
    public void GetNewFileName_HandlesEmptyFileNameWithoutExtension()
    {
        // Arrange
        const string fileNameWithoutExtension = "";
        const string fileExtension = ".txt";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(2, fileExtension);

        // Assert
        Assert.Equal("(2).txt", result);
    }

    [Fact]
    public void GetNewFileName_HandlesMultipleExtensions()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile.backup";
        const string fileExtension = ".zip";

        // Act
        string result = fileNameWithoutExtension.GetNewFileName(1, fileExtension);

        // Assert
        Assert.Equal("testfile.backup(1).zip", result);
    }

    #endregion

    #region GetNewFileNameWithMaxLength Tests

    [Fact]
    public void GetNewFileNameWithMaxLength_ReturnsFullNameWhenUnderMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";
        const int maxLength = 50;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, maxLength);

        // Assert
        Assert.Equal("testfile.txt", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_TruncatesWhenExceedsMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "verylongfilenamethatshouldbetruncated";
        const string fileExtension = ".txt";
        const int maxLength = 20;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_TruncatesWithCounterWhenExceedsMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "verylongfilenamethatshouldbetruncated";
        const string fileExtension = ".txt";
        const int maxLength = 25;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(5, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
        Assert.Contains("(5)", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_UsesDefaultMaxLengthOf255()
    {
        // Arrange
        string fileNameWithoutExtension = new('a', 300);
        const string fileExtension = ".txt";

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension);

        // Assert
        Assert.True(result.Length <= 255);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_HandlesShortMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";
        const int maxLength = 10;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_HandlesTruncationWithIndex()
    {
        // Arrange
        const string fileNameWithoutExtension = "verylongfilename";
        const string fileExtension = ".txt";
        const int maxLength = 15;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(10, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
        Assert.Contains("(10)", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_HandlesExactMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";
        // "testfile.txt" is exactly 12 characters
        const int maxLength = 12;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, maxLength);

        // Assert
        Assert.Equal("testfile.txt", result);
        Assert.Equal(maxLength, result.Length);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_HandlesOneLessThanMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "testfile";
        const string fileExtension = ".txt";
        // "testfile.txt" is 12 characters, max is 11
        const int maxLength = 11;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void GetNewFileNameWithMaxLength_HandlesVeryLargeIndex()
    {
        // Arrange
        const string fileNameWithoutExtension = "file";
        const string fileExtension = ".txt";
        const int maxLength = 15;

        // Act
        string result = fileNameWithoutExtension.GetNewFileNameWithMaxLength(999, fileExtension, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength);
        Assert.EndsWith(".txt", result);
        Assert.Contains("(999)", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void IntegrationTest_CompleteFileNameProcessing()
    {
        // Arrange
        const string fileName = "test<file>:name.txt";
        const int maxLength = 20;

        // Act
        string prepared = fileName.PrepareFileName();
        string withLength = fileName.PreparedFileNameConsideringLength(maxLength);

        // Assert
        Assert.DoesNotContain("<", prepared);
        Assert.DoesNotContain(">", prepared);
        Assert.DoesNotContain(":", prepared);
        Assert.True(withLength.Length <= maxLength);
    }

    [Fact]
    public void IntegrationTest_FileNameWithCounterAndMaxLength()
    {
        // Arrange
        const string fileNameWithoutExtension = "verylongfilenamethatshouldbetruncated";
        const string fileExtension = ".txt";

        // Act
        string result1 = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, fileExtension, 20);
        string result2 = fileNameWithoutExtension.GetNewFileNameWithMaxLength(1, fileExtension, 20);
        string result3 = fileNameWithoutExtension.GetNewFileNameWithMaxLength(2, fileExtension, 20);

        // Assert
        Assert.True(result1.Length <= 20);
        Assert.True(result2.Length <= 20);
        Assert.True(result3.Length <= 20);
        Assert.DoesNotContain("(", result1);
        Assert.Contains("(1)", result2);
        Assert.Contains("(2)", result3);
    }

    #endregion
}
