// this is a tool to load/unload knowledge modules that define projects
using System.Collections.Generic;
using System.Linq;
using IoBTMessage.Extensions;



namespace IoBTMessage.Models
{

	public class DO_World3D : DO_Hero
	{
		public string systemName { get; set; }

		public List<SPEC_Platform> platforms { get; set; } = new List<SPEC_Platform>();
		public List<SPEC_Body> bodies { get; set; } = new List<SPEC_Body>();
		public List<SPEC_Label> labels { get; set; } = new List<SPEC_Label>();
		public List<SPEC_Relationship> relationships { get; set; } = new List<SPEC_Relationship>();
	}

	public class DT_World3D : DT_Hero, ISystem
	{
		public string systemName;

		public List<UDTO_Platform> platforms = new List<UDTO_Platform>();
		public List<UDTO_Body> bodies = new List<UDTO_Body>();
		public List<UDTO_Label> labels = new List<UDTO_Label>();
		public List<UDTO_Relationship> relationships = new List<UDTO_Relationship>();

		public DT_World3D()
		{
		}

		public DT_World3D ShallowCopy()
		{
			var result = (DT_World3D)this.MemberwiseClone();
			result.platforms = null;
			result.bodies = null;
			result.labels = null;
			result.relationships = null;
			result.assetReferences = null;
			return result;
		}

		public T Find<T>(string name) where T : UDTO_3D
		{
			if (typeof(T).Name.Matches(nameof(UDTO_Body)))
				return bodies?.FirstOrDefault(item => item.name.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Label)))
				return labels?.FirstOrDefault(item => item.name.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Platform)))
				return platforms?.FirstOrDefault(item => item.name.Matches(name)) as T;

			return null;
		}

		public T FindReferenceDesignation<T>(string name) where T : UDTO_3D
		{
			if (typeof(T).Name.Matches(nameof(UDTO_Body)))
				return bodies?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Label)))
				return labels?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			if (typeof(T).Name.Matches(nameof(UDTO_Platform)))
				return platforms?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

			return null;
		}

		public DT_World3D FlushPlatforms()
		{
			platforms.ForEach(platform => platform.Flush());
			return this;
		}

		public List<UDTO_Platform> FillPlatforms()
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

		public DT_World3D FillWorldFromPlatform(UDTO_Platform platform)
		{
			platforms.Add(platform);
			bodies.AddRange(platform.bodies);
			labels.AddRange(platform.labels);
			relationships.AddRange(platform.relationships);
			return RemoveDuplicates();
		}

		public DT_World3D FillWorldFromWorld(DT_World3D world)
		{
			platforms.AddRange(world.platforms);
			bodies.AddRange(world.bodies);
			labels.AddRange(world.labels);
			relationships.AddRange(world.relationships);
			return RemoveDuplicates();
		}

		public DT_World3D RemoveDuplicates()
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
}