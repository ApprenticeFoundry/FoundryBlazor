using FoundryBlazor.Extensions;
using IoBTMessage.Models;

namespace BlazorBoids;
public class Boid
{
    private UDTO_Position? position;
    public double X { get; set; }
    public double Y { get; set; }
    public double Angle { get; set; }
    public double Xvel { get; set; }
    public double Yvel { get; set; }

    public string Color { get; set; } = "Yellow";
    public string BoidId { get; set;  } = Guid.NewGuid().ToString();


    public Boid()
    {
        BoidId = "ABCD";
    }

    public Boid(Random rand, double width, double height, string color)
    {
        X = rand.NextDouble() * width;
        Y = rand.NextDouble() * height;
        Xvel = (rand.NextDouble() - .5);
        Yvel = (rand.NextDouble() - .5);
        Color = color;
        Angle = GetAngle();
    }

    public bool Move(int x, int y, double angle)
    {
        (X, Y, Angle) = (x, y, angle);
        return true;
    }

    public void MoveForward(double minSpeed = 1, double maxSpeed = 5)
    {
        X += Xvel;
        Y += Yvel;

        var speed = GetSpeed();
        if (speed > maxSpeed)
        {
            Xvel = (Xvel / speed) * maxSpeed;
            Yvel = (Yvel / speed) * maxSpeed;
        }
        else if (speed < minSpeed)
        {
            Xvel = (Xvel / speed) * minSpeed;
            Yvel = (Yvel / speed) * minSpeed;
        }

        if (double.IsNaN(Xvel))
            Xvel = 0;
        if (double.IsNaN(Yvel))
            Yvel = 0;
    }

    public (double x, double y) GetPosition(double time)
    {
        return (X + Xvel * time, Y + Yvel * time);
    }

    public void Accelerate(double scale = 1.0)
    {
        Xvel *= scale;
        Yvel *= scale;
    }

    public double GetAngle()
    {
        if (double.IsNaN(Xvel) || double.IsNaN(Yvel))
            return 0;

        if (Xvel == 0 && Yvel == 0)
            return 0;

        double angle = Math.Atan(Yvel / Xvel) * 180 / Math.PI - 90;
        if (Xvel < 0)
            angle += 180;

        return angle;
    }

    public UDTO_Position GetLocation(double lat, double lng) 
    {
        position ??= new UDTO_Position();
        position.lat = lat + X / 10;
        position.lng = lng + Y / 10;
        return position;
    }
    public double GetSpeed()
    {
        return Math.Sqrt(Xvel * Xvel + Yvel * Yvel);
    }

    public double GetDistance(Boid otherBoid)
    {
        double dX = otherBoid.X - X;
        double dY = otherBoid.Y - Y;
        double dist = Math.Sqrt(dX * dX + dY * dY);
        return dist;
    }
}
