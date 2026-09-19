using System.Text;

public static class StringBuilderEx
{
    public static int Insert(StringBuilder stringBuilder, string text, int charIndex)
    {
        stringBuilder.Insert(charIndex, text);
        charIndex += text.Length;
        return charIndex;
    }
}
