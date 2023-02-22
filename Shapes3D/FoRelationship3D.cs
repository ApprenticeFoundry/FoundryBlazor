using System.Collections.Generic;

namespace FoundryBlazor.Shape;

	public class FoRelationship3D : FoGlyph3D
	{
		public string relationship;
		public string source;
		public List<string> sink = new List<string>();


		public FoRelationship3D() : base()
		{
		}
		public override UDTO_3D CopyFrom(UDTO_3D obj)
		{
			base.CopyFrom(obj);
			var rel = obj as FoRelationship3D;
			this.source = rel.source;
			this.relationship = rel.relationship;
			return this;
		}

		public FoRelationship3D Build(string source, string relationship, string target)
		{
			this.source = source;
			this.relationship = relationship;
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

