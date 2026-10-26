using System.Numerics;

public interface ITextContainer
{
    public const int MIN_POINT_SIZE = 4;

    public int PointSize { get; set; }
    public int LineSpacing { get; set; }

    public string Text { get; }

    public int CharIndex { get; }
    public int LineIndex { get; }

    public SelectionMode SelectionMode { get; }

    public int SelectionCharIndex { get; }
    public int SelectionLineIndex { get; }

    public CaretStyle CaretStyle { get; set; }

    public string SelectedLine { get; }

    public int LineLength { get; }

    public Vector2 GetCaretPosition(Vector2 offset);
    public void Insert(string text);
    public void Insert(string[] lines);
    public void Append(string text);
    public void Append(string[] texts);
    public void RemoveBackwards(int count);
    public void DeleteLine();
    public void MoveLeft(int count = 1);
    public void MoveRight(int count = 1);
    public void MoveUp();
    public void MoveDown();
    public void InsertNewLine(bool fancyStuff = true);
    public void InsertNewLineOnPreviousLine();
    public void InsertTab();
    public void JoinLineUpwards();
    public void JumpToEndOfLine();
    public void JumpToStartOfLine();
    public int GetJumpLeftLength();
    public int GetJumpRightLength();
    public int GetLineStartIndent();
    public void PasteClipboard();
    public void StartSelection(SelectionMode selectionMode);
    public void EndSelection();

    /// <summary>
    /// Automatically adjusts the CharIndex and LineIndex to not be out of bounds
    /// </summary>
    public void DeleteSelection();

    /// <summary>
    /// (from, to)
    /// </summary
    public (int, int)? GetSelectionRangeOnLine(int lineIndex);
    public void JumpToMousePosition(Vector2 offset);
    public string GetSelectedText();
    public void CopySelection();
    public void Clear();
    public void JumpToEndOfFile();
}
