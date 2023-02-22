// this is a tool to load/unload knowledge modules that define projects
using System.Collections.Generic;
using System.Linq;
using IoBTMessage.Extensions;


namespace FoundryBlazor.Shape;


	public class FoWorld3D : FoGlyph3D
	{
		public string systemName;

		public List<FoGroup3D> platforms = new List<FoGroup3D>();
		public List<UDTO_Body> bodies = new List<UDTO_Body>();
		public List<UDTO_Label> labels = new List<UDTO_Label>();
		public List<FoRelationship3D> relationships = new List<FoRelationship3D>();

		public FoWorld3D()
		{
		}

		public FoWorld3D ShallowCopy()
		{
			var result = (FoWorld3D)this.MemberwiseClone();
			result.platforms = null;
			result.bodies = null;
			result.labels = null;
			result.relationships = null;
			result.assetReferences = null;
			return result;
		}

		public T Find<T>(string name) where T : FoGlyph3D
		{
			if (typeof(T).Name.Matches(nameof(UDTO_Body)))
				return bodies?.FirstOrDefault(item => item.name.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Label)))
				return labels?.FirstOrDefault(item => item.name.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(FoGroup3D)))
				return platforms?.FirstOrDefault(item => item.name.Matches(name)) as T;

			return null;
		}

		public T FindReferenceDesignation<T>(string name) where T : FoGlyph3D
		{
			if (typeof(T).Name.Matches(nameof(UDTO_Body)))
				return bodies?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Label)))
				return labels?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(FoGroup3D)))
				return platforms?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			return null;
		}

		public FoWorld3D FlushPlatforms()
		{
			platforms.ForEach(platform => platform.Flush());
			return this;
		}

		public List<FoGroup3D> FillPlatforms()
		{
			platforms.ForEach(platform =>
			{
				platform.Flush();
				var platformName = platform.platformName;

				bodies.Where(obj => obj.platformName.Matches(platformName))
						.Select(obj => platform.Add<UDTO_Body>(obj)).ToList();


				labels.Where(obj => obj.platformName.Matches(platformName))
						.Select(obj => platform.Add<UDTO_Label>(obj)).ToList();
			});
			return this.platforms;
		}

		public FoWorld3D FillWorldFromPlatform(FoGroup3D platform)
		{
			platforms.Add(platform);
			bodies.AddRange(platform.bodies);
			labels.AddRange(platform.labels);
			relationships.AddRange(platform.relationships);
			return RemoveDuplicates();
		}

		public FoWorld3D FillWorldFromWorld(FoWorld3D world)
		{
			platforms.AddRange(world.platforms);
			bodies.AddRange(world.bodies);
			labels.AddRange(world.labels);
			relationships.AddRange(world.relationships);
			return RemoveDuplicates();
		}

		public FoWorld3D RemoveDuplicates()
		{
			platforms = platforms.DistinctBy(i => i.uniqueGuid).ToList();
			bodies = bodies.DistinctBy(i => i.uniqueGuid).ToList();
			labels = labels.DistinctBy(i => i.uniqueGuid).ToList();
			relationships = relationships.DistinctBy(i => i.uniqueGuid).ToList();

			// platforms = platforms.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
			// bodies = bodies.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
			// labels = labels.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
			// relationships = relationships.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
			return this;
		}
	}
