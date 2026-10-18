public interface IKeybindSet
{
    /// <summary>
    /// Returns if changes have been made to the text document
    /// </summary>
    public bool HandleKeybinds(TextDocument document);

    /// <summary>
    /// Pretty self explanatory if you have a brain
    /// </summary>
    public CaretStyle GetCaretStyle();
}
