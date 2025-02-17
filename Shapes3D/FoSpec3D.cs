using System;
using System.Numerics;
namespace FoundryBlazor.Shape;
public class FoSpec3D
{
    public string Name { get; set; } = "";
    public string GlyphId { get; set; } = "";
    public string Color { get; set; } = "Green";
    public double Opacity { get; set; } = 1.0;
    public string Type { get; set; } = "";
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Z { get; set; } = 0;

    public double Rx { get; set; } = 0;
    public double Ry { get; set; } = 0;
    public double Rz { get; set; } = 0;

    public double W { get; set; } = 0;
    public double H { get; set; } = 0;
    public double D { get; set; } = 0;

    public double Px { get; set; } = 0;
    public double Py { get; set; } = 0;
    public double Pz { get; set; } = 0;
}