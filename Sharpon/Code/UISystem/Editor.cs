using System;
using System.IO;
using Microsoft.Xna.Framework;
using Sharpon;

public class Editor : UIElement
{
    private string _openedFilePath;

    public Editor(UISystem uiSystem, Vector2 position, string text, Color backgroundColor) : base(uiSystem, position, text, backgroundColor)
    {
        _keybindHandler = new KeybindHandler(new NewLineCommand(),
                                             new NewLineExCommand(),
                                             new BackspaceCommand(),
                                             new BackspaceExCommand(),
                                             new LeftArrowCommand(),
                                             new RightArrowCommand(),
                                             new UpArrowCommand(),
                                             new DownArrowCommand(),
                                             new TabCommand());
    }
    
    public void OpenFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                using StreamReader streamReader = new(filePath);
                string text = streamReader.ReadToEnd();
                SetText(text);

                _openedFilePath = filePath;
            }
            catch (IOException e)
            {
                Console.WriteLine("File could not be opened: ");
                Console.WriteLine(e.Message);
            }

        }
        else
        {
            Console.WriteLine("File doesnt exist " + filePath);
        }
    }
}