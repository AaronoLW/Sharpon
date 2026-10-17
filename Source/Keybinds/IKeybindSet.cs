public interface IKeybindSet
{
    /// <summary>
    /// Returns if changes have been made to the text document
    /// </summary>
    public bool HandleKeybinds(TextDocument document);
}
