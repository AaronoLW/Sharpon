public static class StringHelper
{
    public static void ApplyTextAction(TextDocument document, TextAction action)
    {
        switch (action)
        {
            case TextAction.DeleteCharacter:
                if (document.CharIndex != document.Text.Length &&
                    document.CharIndex != 0 &&
                    TextDocument.BracketsClosed.Contains(document.Text[document.CharIndex]) &&
                    TextDocument.BracketsOpen.Contains(document.Text[document.CharIndex - 1]))
                {
                    document.CharIndex++;
                    document.TryRemoveBackwards(2);
                }
                else
                    document.TryRemoveBackwards(1);

                break;

            case TextAction.DeleteWord:
                document.TryRemoveBackwards(document.GetJumpBackLength());
                break;

            case TextAction.MoveLeft:
                if (document.CharIndex > 0)
                    document.CharIndex--;
                break;

            case TextAction.MoveRight:
                if (document.CharIndex < document.Text.Length)
                    document.CharIndex++;
                break;

            case TextAction.JumpLeft:
                document.CharIndex -= document.GetJumpBackLength();
                break;

            case TextAction.JumpRight:
                document.CharIndex += document.GetJumpForwardLength();
                break;

            case TextAction.MoveUp:
                document.MoveUp();
                break;

            case TextAction.MoveDown:
                document.MoveDown();
                break;

            case TextAction.InsertNewline:
                InsertNewLine(document);
                break;

            case TextAction.InsertTab:
                document.Insert("    ");
                break;

            case TextAction.GoToStartOfLine:
                document.CharIndex = document.GetLineStartIndex();
                break;

            case TextAction.GoToEndOfLine:
                document.CharIndex = document.GetLineStartIndex() + document.GetLineLength();
                break;

            case TextAction.DeleteCharacterForward:
                if (document.CharIndex != document.Text.Length)
                {
                    document.CharIndex++;
                    document.TryRemoveBackwards(1);
                }
                break;

            case TextAction.DeleteWordForward:
                int length = document.GetJumpForwardLength();
                document.CharIndex += length;
                document.TryRemoveBackwards(length);
                break;
        }
    }

    private static void InsertNewLine(TextDocument document)
    {
        string indent = "";

        int startIndent = document.GetLineStartIndent();
        for (int i = 0; i < startIndent; i++)
            indent += ' ';

        if (document.IsEnclosedInBrackets())
        {
            document.CharIndex--;
            document.Insert('\n' + indent);
            document.CharIndex++;
            document.Insert('\n' + indent);
            document.Insert('\n' + indent);
            document.MoveUp();
            document.Insert("    ");
        }
        else
        {
            document.Insert('\n' + indent);
        }

    }
}
