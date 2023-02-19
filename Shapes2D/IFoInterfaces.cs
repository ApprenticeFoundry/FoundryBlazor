using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoundryBlazor.Shape;


public interface IFoButton
{
    public Action ClickAction();
    public string DisplayText();
}
public interface IFoMenu
{
    public string DisplayText();
    List<IFoButton> Buttons();
}

public interface IFoCommand
{
    List<IFoButton> Buttons();
}
