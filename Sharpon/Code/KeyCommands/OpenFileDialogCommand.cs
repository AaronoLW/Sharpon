using System;
using Microsoft.Xna.Framework.Input;

public class OpenFileDialogCommand : KeyCommand
{
    public override bool RequiresControl { get; } = true;
    public override bool RequiresShift { get; } = true;
    public override bool Repeats { get; } = false;
    public override Keys AssignedKey { get; } = Keys.P;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public OpenFileDialogCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        FileDialog dialog = uiElement as FileDialog ?? throw new InvalidOperationException("A non-file-dialog cant open a file dialog " + uiElement.GetType());
        if (dialog.IsOpened) dialog.Close();
        else 
        { 
            dialog.Open();
            
        }
    }
}