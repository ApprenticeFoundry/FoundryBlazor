using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public class MoDataNode<S,T> : MoComponent where S : SPEC_Base where T: UDTO_Base
{
    
    public S? Spec { get;  set; }
    public T? Source { get;  set; }
    public MoDataNode(string name) : base(name)
    {
        Spec = Activator.CreateInstance(typeof(S)) as S;
        Source = Activator.CreateInstance(typeof(T)) as T;
    }

    public override void Start() 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.Start());
    }
    public override void Stop() 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.Stop());
    }
    public override void OnNext(int tick) 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.OnNext(tick));
    }
    
    public virtual void ApplyExternalMethods(FoCompound2D shape)
    {
        var model = this;
        shape.GlyphId = model.ModelId;

        GenerateValues2D(shape);
 
        shape.ContextLink = (obj,tick) =>
        {
            model.Tick = tick;
            SetDrawModelText(shape);
        };
    }

    public  void GenerateValues2D(FoCompound2D shape)
    {
        var source = this.Spec;
        if ( source == null) return;

        var spec = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        var plist = from prop in source.GetType().GetProperties(spec) where prop.CanRead && prop.CanWrite select prop;

        foreach (PropertyInfo prop in plist)
        {
            var value = prop.GetValue(source, null);
            //"20px Segoe UI"
            var valueShape = new FoCompoundValue2D(prop)
            {
                IsVisible = false,
                Text = prop.Name,
                Value = value?.ToString() ?? ""
            };  
            shape.Add<FoCompoundValue2D>(valueShape);
        }

    }


    public  void UpdateValues2DUsingSpec(FoCompound2D shape, object model)
    {
        shape.GetMembers<FoCompoundValue2D>()?.ForEach( item => item.UpdateValue(model));    
    }
}
