using BlazorThreeJS.Maths;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shape;
public class FoDatum3D : FoGlyph3D
	{

		public string? Shape { get; set; }
		public string? Text { get; set; }
		public List<string>? Details { get; set; }



		public FoDatum3D() : base()
		{
		}

		public FoDatum3D CreateTextAt(string text, double xLoc = 0.0, double yLoc = 0.0, double zLoc = 0.0)
		{
			this.Text = text.Trim();

			Position = new Vector3(xLoc, yLoc, zLoc);

			return this;
		}

		public FoDatum3D CreateLabelAt(string text, List<string>? details = null, double xLoc = 0.0, double yLoc = 0.0, double zLoc = 0.0)
		{
			this.Details = details;
            return CreateTextAt(text, xLoc, yLoc, zLoc);
		}
		public FoDatum3D EstablishBox(double width = 1.0, double height = 1.0, double depth = 1.0)
		{
			BoundingBox ??= new Vector3();

        	BoundingBox.Set(width, height, depth);

			return this;
		}

		public FoDatum3D CreateShape(string shape, double width = 1.0, double height = 1.0, double depth = 1.0)
		{
			this.Shape = shape;
			return EstablishBox(width, height, depth);
		}
	}

