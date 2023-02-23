using System.Collections.Generic;
using System.Reflection.Emit;

namespace FoundryBlazor.Shape;
	public class FoDatum3D : FoGlyph3D
	{

		public string shape { get; set; }
		public string text { get; set; }
		public List<string> details { get; set; }
		public FoVector3D position { get; set; }
		public FoVector3D boundingBox { get; set; }

		public FoDatum3D() : base()
		{
		}

		public FoDatum3D CreateTextAt(string text, double xLoc = 0.0, double yLoc = 0.0, double zLoc = 0.0, string units = "m")
		{
			this.text = text.Trim();

			position = new FoVector3D()
			{
				units = units,
				X = xLoc,
				Y = yLoc,
				Z = zLoc
			};
			return this;
		}

		public FoDatum3D CreateLabelAt(string text, List<string> details = null, double xLoc = 0.0, double yLoc = 0.0, double zLoc = 0.0, string units = "m")
		{
			this.text = text.Trim();
			this.details = details;

			position = new FoVector3D()
			{
				units = units,
                X = xLoc,
                Y = yLoc,
                Z = zLoc
            };
			return this;
		}
		public FoDatum3D EstablishBox(double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
		{
			boundingBox ??= new FoVector3D();


        	boundingBox.X = width;
		    boundingBox.Y = height;
			boundingBox.Z = depth;
			boundingBox.units = units;

			return this;
		}

		public FoDatum3D CreateShape(string shape, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
		{
			this.shape = shape;
			return EstablishBox(width, height, depth, units);
		}
	}

