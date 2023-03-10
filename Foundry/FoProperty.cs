namespace FoundryBlazor;
public class FoProperty : FoBase
{
    public FoProperty(string name) : base(name)
    {
    }
}

public class FoScaleProperty<T,U> : FoProperty where U: UnitFamily
{
    private dynamic? _value { get; set; }
    private U _units { get; set; }
    private Func<T>? _formula { get; set; }
    private string _type { get; set; }
    public T? Value
    {
        set
        {
            _value = value;
        }
        get
        {
            if (_formula != null)
            {
                _value = _formula();
            }
            return _value != null ? (T)Convert.ChangeType(_value, typeof(T)) : default(T);
        }
    }

    public FoProperty SetFormula(Func<T> func)
    {
        _formula = func;
        return this;
    }

    public Boolean HasFormula
    {
        get
        {
            return _formula != null;
        }
    }

    public FoScaleProperty(string name, UnitFamily units, object value): base(name)
    {
        _value = value;
        _units = (U)units;
        _type = typeof(T).Name;
    }

    public FoScaleProperty(string name, UnitFamily units, Func<T> formula) : base(name)
    {
        _formula = formula;
        _units = (U)units;
        _type = typeof(T).Name;
    }
}

