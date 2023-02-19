using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;


// Washington Monument/Coordinates
// 38.8895° N, 77.0353° W

namespace BlazorBoids;

public interface IBoidField
{

}
public class BoidField: IBoidField
{
    public int PredatorCount = 0;
    public double Width { get; private set; }
    public double Height { get; private set; }
    public readonly List<Boid> Boids = new();
    
    public readonly Dictionary<string,Boid> ForeignBoids = new();
    private readonly Random Rand = new();

    public bool IsRunning = false;

    private ICommand Command { get; set; }
    private IPageManagement PageManager { get; set; }

    public BoidField(ICommand cmd, IPageManagement manager)
    {
        (Command, PageManager) = (cmd, manager);

        var page = PageManager.CurrentPage();
        (Width, Height) = (page.Width, page.Height);
    }

    public void CreateBoids(int boidCount, string color)
    {

        for (int i = 0; i < boidCount; i++)
        {
            var boid = new Boid(Rand, Width, Height, color);
            Boids.Add(boid);
        }
    }

    public Dictionary<string, Action> MenuItems()
    {
        var menu = new Dictionary<string, Action>()
        {
            { "Boids", () => ToggleBoids()},
            { "Boids +5", () => BoidsAdd5()},
            { "Boids -5", () => BoidsSub5()},
        };
        return menu;
    }

    protected void SendModelCreated<T>(T model) where T : Boid
    {
        var create = new D2D_Create()
        {
            PayloadType = model.GetType().Name,
            Payload = StorageHelpers.Dehydrate<T>(model, false)
        };

        Task.Run(async () => { await Command.Send(create); });
    }

    protected void SendModelMoved<T>(T model) where T : Boid
    {
        var move = new D2D_Move()
        {
            TargetId = model.BoidId,
            PayloadType = model.GetType().Name,
            PinX = (int)model.X,
            PinY = (int)model.Y,
            Angle = model.GetAngle()
        };
        // $"Send___ D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message".WriteLine(ConsoleColor.Yellow);

        Task.Run(async () => { await Command.Send(move); });
    }

    protected void SendModelDestroy<T>(T model) where T : Boid
    {
        var destroy = new D2D_Destroy()
        {
            TargetId = model.BoidId,
            PayloadType = model.GetType().Name
        };
        // $"Send___ D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message".WriteLine(ConsoleColor.Yellow);

         Task.Run(async () => { await Command.Send(destroy); });
    }

    private void BoidsSub5()
    {
        var removed = AdjustBoidCountBy(-5);
        var ids = removed.Select(item => item.BoidId).ToList();

        removed.ForEach(boid => SendModelDestroy<Boid>(boid));

        IsRunning = ids.Count > 0;

        var page = PageManager.CurrentPage();
        page.Slot<FoShape2D>().ExtractWhere(item => ids.Contains(item.GlyphId));
    }

   private void BoidsAdd5()
    {
        var added = AdjustBoidCountBy(5);
        added.ForEach(boid => SendModelCreated<Boid>(boid));

        IsRunning = true;

        //only create local shape
        CreateShapesForBoids(added, PageManager);
    }

    private void ToggleBoids()
    {
        var page = PageManager.CurrentPage();
        if ( !IsRunning) 
        {
            var BoidCount = 10;
            CreateBoids(BoidCount, RandomColor());
            Boids.ForEach(boid => SendModelCreated<Boid>(boid));
            CreateShapesForBoids(Boids, PageManager);
        }

        IsRunning = !IsRunning;
    }
    public List<Boid> AdjustBoidCountBy(int count)
    {
        var change = new List<Boid>();

        if ( count < 0 ){
            var extract = Math.Abs(count);
            extract = extract < Boids.Count ? extract : Boids.Count;
            for (int i = 0; i < extract; i++)
            {
                change.Add(Boids[0]);
                Boids.RemoveAt(0);
            }
        } 
        else 
        {
            var color = RandomColor();
            for (int i = 0; i < count; i++)
            {
                var boid = new Boid(Rand, Width, Height, color);
                change.Add(boid);
            }
            Boids.AddRange(change);
        }
        return change;
    }

    public void DoDestroy(D2D_Destroy destroy)
    {
        ForeignBoids.Remove(destroy.TargetId);
        PageManager.ExtractShapes(destroy.TargetId);
        PageManager.CurrentPage().ExtractWhere<FoShape2D>(item => item.GlyphId == destroy.TargetId);
    }

    public void DoMovement(D2D_Move move)
    {
        if (ForeignBoids.TryGetValue(move.TargetId, out Boid? boid) == true)
        {
            boid.Move(move.PinX, move.PinY, move.Angle);
        }
    }

    public void ApplyExternalMethods(Boid boid, FoShape2D shape)
    {
        shape.GlyphId = boid.BoidId;
        shape.ShapeDraw = async (ctx,  obj) =>  
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(0, 0);
            await ctx.LineToAsync(8, -4);
            await ctx.LineToAsync(0, 20);
            await ctx.LineToAsync(-8, -4);
            await ctx.LineToAsync(0, 0);
            await ctx.ClosePathAsync();
            await ctx.FillAsync();
        };
        //this is using a clouser 
        shape.ContextLink = (obj,tick) =>
        {
            obj.PinX = (int)boid.X;
            obj.PinY = (int)boid.Y;
            obj.Angle = boid.Angle;
            obj.Color = boid.Color ?? "Black";
        };
    }
    public FoShape2D Capture(Boid boid, IPageManagement manage)
    {
        ForeignBoids.TryAdd(boid.BoidId,boid);
        var shape = new FoShape2D(10, 10, boid.Color);
        shape.MoveTo((int)boid.X, (int)boid.Y);

        manage.Add<FoShape2D>(shape);
        ApplyExternalMethods(boid, shape);
        
        return shape;
    }

    public List<FoShape2D> CreateShapesForBoids(List<Boid> boids, IPageManagement manage)
    {
        var list = boids.Select(boid => {
                        var shape = new FoShape2D(10, 10, boid.Color);
                        manage.Add(shape).MoveTo((int)boid.X, (int)boid.Y);
                        ApplyExternalMethods(boid, shape);
                        return shape;
                    }).ToList();

        return list;
    }

    public static string RandomColor()
    {
        Random rnd = new();
        var colors = new List<string> { "White", "Indigo", "SaddleBrown", "Salmon", "Red", "Purple", "LightGreen", "SteelBlue", "Red", "Black" };
        var index = rnd.Next(colors.Count);
        return colors[index];
    }

    public void Resize(double width, double height) => (Width, Height) = (width, height);

    public List<Boid> Advance(bool bounceOffWalls = true, bool wrapAroundEdges = false)
    {
        if ( !IsRunning ) return Boids;

        // update void speed and direction (velocity) based on rules
        foreach (var boid in Boids)
        {
            (double flockXvel, double flockYvel) = Flock(boid, 50, .0003);
            (double alignXvel, double alignYvel) = Align(boid, 50, .01);
            (double avoidXvel, double avoidYvel) = Avoid(boid, 20, .001);
            (double predXvel, double predYval) = Predator(boid, 150, .00005);
            boid.Xvel += flockXvel + avoidXvel + alignXvel + predXvel;
            boid.Yvel += flockYvel + avoidYvel + alignYvel + predYval;
        }

        // move all boids forward in time
        foreach (var boid in Boids)
        {
            boid.MoveForward();
            if (bounceOffWalls)
                BounceOffWalls(boid);
            if (wrapAroundEdges)
                WrapAround(boid);

            boid.Angle = boid.GetAngle();
            SendModelMoved<Boid>(boid);
        }


        return Boids;
    }

    private (double xVel, double yVel) Flock(Boid boid, double distance, double power)
    {
        // point toward the center of the flock (mean flock boid position)
        var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
        double meanX = neighbors.Sum(x => x.X) / neighbors.Count();
        double meanY = neighbors.Sum(x => x.Y) / neighbors.Count();
        double deltaCenterX = meanX - boid.X;
        double deltaCenterY = meanY - boid.Y;
        return (deltaCenterX * power, deltaCenterY * power);
    }

    private (double xVel, double yVel) Avoid(Boid boid, double distance, double power)
    {
        // point away as boids get close
        var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
        (double sumClosenessX, double sumClosenessY) = (0, 0);
        foreach (var neighbor in neighbors)
        {
            double closeness = distance - boid.GetDistance(neighbor);
            sumClosenessX += (boid.X - neighbor.X) * closeness;
            sumClosenessY += (boid.Y - neighbor.Y) * closeness;
        }
        return (sumClosenessX * power, sumClosenessY * power);
    }

    private (double xVel, double yVel) Predator(Boid boid, double distance, double power)
    {
        // point away as predators get close
        (double sumClosenessX, double sumClosenessY) = (0, 0);
        for (int i = 0; i < PredatorCount; i++)
        {
            Boid predator = Boids[i];
            double distanceAway = boid.GetDistance(predator);
            if (distanceAway < distance)
            {
                double closeness = distance - distanceAway;
                sumClosenessX += (boid.X - predator.X) * closeness;
                sumClosenessY += (boid.Y - predator.Y) * closeness;
            }
        }
        return (sumClosenessX * power, sumClosenessY * power);
    }

    private (double xVel, double yVel) Align(Boid boid, double distance, double power)
    {
        // point toward the center of the flock (mean flock boid position)
        var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
        double meanXvel = neighbors.Sum(x => x.Xvel) / neighbors.Count();
        double meanYvel = neighbors.Sum(x => x.Yvel) / neighbors.Count();
        double dXvel = meanXvel - boid.Xvel;
        double dYvel = meanYvel - boid.Yvel;
        return (dXvel * power, dYvel * power);
    }

    private void BounceOffWalls(Boid boid)
    {
        double pad = 50;
        double turn = .5;
        if (boid.X < pad)
            boid.Xvel += turn;
        if (boid.X > Width - pad)
            boid.Xvel -= turn;
        if (boid.Y < pad)
            boid.Yvel += turn;
        if (boid.Y > Height - pad)
            boid.Yvel -= turn;
    }

    private void WrapAround(Boid boid)
    {
        if (boid.X < 0)
            boid.X += Width;
        if (boid.X > Width)
            boid.X -= Width;
        if (boid.Y < 0)
            boid.Y += Height;
        if (boid.Y > Height)
            boid.Y -= Height;
    }
}

