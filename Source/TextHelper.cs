using System.Text;

public static class TextHelper
{
    public static readonly char[] BracketsOpen = [
        '{',
        '(',
        '[',
        '"',
        '<',
    ];

    public static readonly char[] BracketsClosed = [
        '}',
        ')',
        ']',
        '"',
        '>',
    ];

    public static readonly char[] SpecialCharacters = [
        '.',
        ',',
        '/',
        '\\',
    ];

    public static int GetJumpLeftLength(ITextContainer textContainer)
    {
        if (textContainer.CharIndex == 0)
        {
            if (textContainer.LineIndex > 0)
                return 1;
            else
                return 0;
        }

        if (IsSpecialCharacter(textContainer.SelectedLine[textContainer.CharIndex - 1]))
        {
            if (IsEnclosedInBrackets(textContainer))
            {
                textContainer.MoveRight();
                return 2;
            }
            else return 1;
        }

        bool foundWord = true;
        if (char.IsWhiteSpace(textContainer.SelectedLine[textContainer.CharIndex - 1]))
            foundWord = false;

        int length = 0;
        for (int i = textContainer.CharIndex - 1; i > 0; i--)
        {
            if (IsSpecialOrWhiteSpaceCharacter(textContainer.SelectedLine[i]))
            {
                if (foundWord)
                    return length;
            }
            else
            {
                foundWord = true;
            }

            length++;
        }

        return textContainer.CharIndex;
    }

    public static int GetJumpRightLength(ITextContainer textContainer)
    {
        if (textContainer.CharIndex == textContainer.LineLength)
        {
            if (textContainer.LineIndex == textContainer.LineCount)
                return 0;
            else
                return 1;
        }

        if (IsSpecialCharacter(textContainer.SelectedLine[textContainer.CharIndex]))
            return 1;

        bool foundWord = true;
        if (char.IsWhiteSpace(textContainer.SelectedLine[textContainer.CharIndex]))
            foundWord = false;

        int length = 0;
        for (int i = textContainer.CharIndex; i < textContainer.LineLength; i++)
        {
            if (IsSpecialOrWhiteSpaceCharacter(textContainer.SelectedLine[i]))
            {
                if (foundWord)
                    return length;
            }
            else
            {
                foundWord = true;
            }

            length++;
        }

        return length;
    }

    public static bool IsEnclosedInBrackets(ITextContainer textContainer)
    {
        if (textContainer.CharIndex >= textContainer.LineLength)
            return false;

        if (textContainer.CharIndex == 0)
            return false;

        if (BracketsClosed.Contains(textContainer.SelectedLine[textContainer.CharIndex]) &&
            BracketsOpen.Contains(textContainer.SelectedLine[textContainer.CharIndex - 1]))
        {
            return true;
        }

        return false;
    }

    public static bool IsSpecialCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character);
    }

    public static bool IsSpecialOrWhiteSpaceCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character) ||
               character == ' ';
    }

    public static string JoinStringArrayToSingleString(string[] lines)
    {
        StringBuilder sb = new();
        foreach (string line in lines)
        {
            sb.Append(line);
        }

        return sb.ToString();
    }

    public static string GetSpacesString(int amount)
    {
        StringBuilder fuckyou = new();
        for (int i = 0; i < amount; i++)
        {
            fuckyou.Append(' ');
        }

        return fuckyou.ToString();
    }
}
