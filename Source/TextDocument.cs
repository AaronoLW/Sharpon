public class TextDocument : TextContainer
{
    public override string Text => string.Join("\n", Lines);

    public TextDocument(string text)
    {
        Lines = [.. text.Split('\n')];
    }

    // hiiiiiii
    // hi but not
}
