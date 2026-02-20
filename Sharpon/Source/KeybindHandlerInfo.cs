public struct KeybindHandlerInfo
{
    public string NewText;
    public int NewCharIndex;
    public bool MayAddTextInput;
    public EditorCommand? EditorCommand;

    public KeybindHandlerInfo(string newText, int newCharIndex, bool mayAddTextInput, EditorCommand? editorCommand)
    {
        NewText = newText;
        NewCharIndex = newCharIndex;
        MayAddTextInput = mayAddTextInput;
        EditorCommand = editorCommand;
    } 
}