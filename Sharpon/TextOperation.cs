using System.Runtime.InteropServices;
using System.Text;

public static class TextOperation
{
    public static int JumpLeft(string stringBuilder, int newCharIndex)
    {
        int charAmount = 0;
        bool haltForSpace = true;

        if (newCharIndex == 0) return 0;
        if (stringBuilder.Length == 0) return 0;
        if (stringBuilder[newCharIndex - 1 < 0 ? 0 : newCharIndex - 1] == ' ') haltForSpace = false;

        if (newCharIndex == stringBuilder.Length) charAmount++;

        for (int i = newCharIndex; i > -1; i--)
        {
            if (i > stringBuilder.Length - 1) continue;
            if (i == 0) { charAmount = newCharIndex; break; }
            

            if (stringBuilder[i - 1] == '\n' && i != newCharIndex) break;

            if (stringBuilder[i - 1] == ' ' && haltForSpace) break;
            if (stringBuilder[i - 1] != ' ' && !haltForSpace) break;

            if (stringBuilder[i - 1] == '(' ||
                stringBuilder[i - 1] == '"' ||
                stringBuilder[i - 1] == '[' ||
                stringBuilder[i - 1] == '{' ||
                stringBuilder[i - 1] == '.' ||
                stringBuilder[i - 1] == '/' ||
                stringBuilder[i - 1] == '<' ||
                stringBuilder[i - 1] == ',' )
            {
                if (i != newCharIndex) break;
            }

            charAmount++;
        }

        return charAmount;
    }

    public static int JumpRight(string stringBuilder, int newCharIndex)
    {
        int charAmount = 0;
        bool haltForSpace = true;

        if (stringBuilder.Length == 0) return 0;
        if (newCharIndex == stringBuilder.Length) return 0;
        if (newCharIndex + 1 < stringBuilder.Length && stringBuilder[newCharIndex + 1] == ' ') haltForSpace = false;

        if (stringBuilder[newCharIndex] == ' ') { charAmount++; newCharIndex++; }
        if (stringBuilder[newCharIndex] == '\n') { return 1; }

        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == ' ' && haltForSpace) break;
            if (stringBuilder[i] != ' ' && !haltForSpace) break;

            if (i != newCharIndex)
            {
                if (stringBuilder[i] == ')' ||
                    stringBuilder[i] == '"' ||
                    stringBuilder[i] == ']' ||
                    stringBuilder[i] == '}' ||
                    stringBuilder[i] == '.' ||
                    stringBuilder[i] == ',' ||
                    stringBuilder[i] == '>' ||
                    stringBuilder[i] == '\n' )
                break;
            }

            charAmount++;
        }

        return charAmount;
    }

    public static int JumpDown(string stringBuilder, int newCharIndex)
    {
        bool seenNewLine = false;

        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == '\n')
            {
                if (!seenNewLine)
                {
                    seenNewLine = true;
                }
                else
                {
                    newCharIndex = i;
                    break;
                }
            }

            if (i == stringBuilder.Length - 1)
            {
                newCharIndex = i + 1;
                break;
            }
        }

        return newCharIndex;
    }

    public static int JumpUp(string stringBuilder, int newCharIndex)
    {
        for (int i = newCharIndex - 1; i > 0; i--)
        {
            if (stringBuilder[i] == '\n')
            {
                return i;
            }
        }

        return 0;
    }

    private static int GetEndOfCurrentLine(string stringBuilder, int newCharIndex)
    {
        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == '\n') return i;
        }

        return stringBuilder.Length;
    }

    public static int GetFirstLetterOnLine(string stringBuilder, int newCharIndex)
    {
        int amount = 0;

        for (int i = newCharIndex - 1; i > -1; i--)
        {
            if (i == stringBuilder.Length) continue;
            if (stringBuilder[i] == '\n') return amount;
            
            if (stringBuilder[i] != ' ')
            {
                amount = 0;
            }
            else
            {
                amount++;
            }
        }

        return amount;
    }

    public static int DeleteCharacter(string stringBuilder, int newCharIndex)
    {
        if (newCharIndex > 0)
        {
            if (newCharIndex < stringBuilder.Length)
            {
                if (stringBuilder[newCharIndex] == ')' && stringBuilder[newCharIndex - 1] == '(' ||
                    stringBuilder[newCharIndex] == '}' && stringBuilder[newCharIndex - 1] == '{' ||
                    stringBuilder[newCharIndex] == '"' && stringBuilder[newCharIndex - 1] == '"' ||
                    stringBuilder[newCharIndex] == ']' && stringBuilder[newCharIndex - 1] == '[')
                {
                    stringBuilder.Remove(newCharIndex - 1, 2);
                    return 2;
                }
                else
                {
                    stringBuilder.Remove(newCharIndex - 1, 1);
                    return 1;
                }
            }
            else
            {
                stringBuilder.Remove(newCharIndex - 1, 1);
                return 1;
            }
        }

        return 0;
    }

    public static (string, int) NewLine(string hurensohn, int newCharIndex)
    {
        StringBuilder stringBuilder = new StringBuilder(hurensohn);

        bool brackets = false;

        if (newCharIndex > 0 && newCharIndex < stringBuilder.Length)
        {
            if (stringBuilder[newCharIndex - 1] == '{' && stringBuilder[newCharIndex] == '}')
            {
                brackets = true;
            }
        }

        if (brackets)
        {
            stringBuilder.Remove(newCharIndex - 1, 2);
            newCharIndex--;
        }

        int firstLetter = GetFirstLetterOnLine(stringBuilder.ToString(), newCharIndex);
        stringBuilder.Insert(newCharIndex, "\n");
        newCharIndex++;

        if (brackets)
        {
            for (int i = 0; i < firstLetter; i++)
            {
                stringBuilder.Insert(newCharIndex, " ");
                newCharIndex++;
            }

            stringBuilder.Insert(newCharIndex, "{\n");
            newCharIndex += 2;

            for (int i = 0; i < firstLetter + 4; i++)
            {
                stringBuilder.Insert(newCharIndex, " ");
                newCharIndex++;
            }

            stringBuilder.Insert(newCharIndex, "\n");

            for (int i = 0; i < firstLetter; i++)
            {
                stringBuilder.Insert(newCharIndex + 1, " ");
            }

            stringBuilder.Insert(newCharIndex + firstLetter + 1, "}");

            return (stringBuilder.ToString(), newCharIndex);
        }
        else
        {
            for (int i = 0; i < firstLetter; i++)
            {
                stringBuilder.Insert(newCharIndex, " ");
                newCharIndex++;
            }

            return (stringBuilder.ToString(), newCharIndex);
        }
    }
}