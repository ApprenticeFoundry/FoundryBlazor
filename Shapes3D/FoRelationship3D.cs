using System.Collections.Generic;

namespace FoundryBlazor.Shape;

	public class FoRelationship3D : FoGlyph3D
	{
		public string? Relationship { get; set; }
    	public string? Source { get; set; }
		public List<string> sink = new ();


		public FoRelationship3D() : base()
		{
		}


		public FoRelationship3D Build(string source, string relationship, string target)
		{
			this.Source = source;
			this.Relationship = relationship;
			this.sink.Add(target);
			return this;
		}

		public FoRelationship3D Relate(string target)
		{
			this.sink.Add(target);
			return this;
		}

		public FoRelationship3D Unrelate(string target)
		{
			this.sink.Remove(target);
			return this;
		}

	}

