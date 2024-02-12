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

}

public interface IShape2D
{
    
}
public interface IImage2D: IShape2D
{
    
}

public interface IShape3D
{

}

public interface IPipe3D
{
    
}

public interface ISnap3D
{
    
}