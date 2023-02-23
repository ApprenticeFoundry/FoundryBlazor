// this is a tool to load/unload knowledge modules that define projects
using System.Collections.Generic;
using System.Linq;
using FoundryBlazor.Extensions;



namespace FoundryBlazor.Shape;


	public class FoWorld3D : FoGlyph3D
	{
		public List<FoGroup3D> platforms = new();
		public List<FoShape3D> bodies = new();
		public List<FoText3D> labels = new();
		public List<FoRelationship3D> relationships = new();

		public FoWorld3D():base()
		{
		}



		// public T Find<T>(string name) where T : FoGlyph3D
		// {
		// 	if (typeof(T).Name.Matches(nameof(FoShape3D)))
		// 		return bodies?.FirstOrDefault(item => item.name.Matches(name)) as T;

		// 	if (typeof(T).Name.Matches(nameof(FoText3D)))
		// 		return labels?.FirstOrDefault(item => item.name.Matches(name)) as T;

		// 	if (typeof(T).Name.Matches(nameof(FoGroup3D)))
		// 		return platforms?.FirstOrDefault(item => item.name.Matches(name)) as T;

		// 	return null;
		// }

		// public T FindReferenceDesignation<T>(string name) where T : FoGlyph3D
		// {
		// 	if (typeof(T).Name.Matches(nameof(FoShape3D)))
		// 		return bodies?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

		// 	if (typeof(T).Name.Matches(nameof(FoText3D)))
		// 		return labels?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

		// 	if (typeof(T).Name.Matches(nameof(FoGroup3D)))
		// 		return platforms?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

		// 	return null;
		// }

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
						.Select(obj => platform.Add<FoShape3D>(obj)).ToList();


				labels.Where(obj => obj.platformName.Matches(platformName))
						.Select(obj => platform.Add<FoText3D>(obj)).ToList();
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
		//platforms = platforms.DistinctBy(i => i.uniqueGuid).ToList();
		//bodies = bodies.DistinctBy(i => i.uniqueGuid).ToList();
		//labels = labels.DistinctBy(i => i.uniqueGuid).ToList();
		//relationships = relationships.DistinctBy(i => i.uniqueGuid).ToList();

		platforms = platforms.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
		bodies = bodies.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
		labels = labels.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
		relationships = relationships.GroupBy(i => i.uniqueGuid).Select(g => g.First()).ToList();
		return this;
		}
	}
