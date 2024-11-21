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


public interface IShape1D
{
    void AddAction(string name, string color, Action action);
}

public interface IShape2D
{
    void AddAction(string name, string color, Action action);
}
public interface IImage2D: IShape2D
{
    
}

public interface IShape3D
{
    void AddAction(string name, string color, Action action);
}

public interface IPipe3D
{
    void AddAction(string name, string color, Action action);
}

public interface ISnap3D
{
    
}