using System;
using System.Collections.Generic;

namespace DTARClient.Model;

public class Node
{
    public int Id { get; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Mass { get; }
    public double Dx { get; set; }
    public double Dy { get; set; }

    public Node(int id, double mass = 1.0)
    {
        Id = id;
        X = 0;
        Y = 0;
        Mass = mass;
        Dx = 0;
        Dy = 0;
    }
}

public class Link
{
    public Node Source { get; }
    public Node Target { get; }
    public double Length { get; }

    public Link(Node source, Node target)
    {
        Source = source;
        Target = target;
        Length = Math.Sqrt(Math.Pow(Target.X - Source.X, 2) + Math.Pow(Target.Y - Source.Y, 2));
    }
}

public class ForceLayout
{
    private List<Node> nodes;
    private List<Link> links;
    private double k;
    private double temperature;
    private double coolingFactor;

    public ForceLayout(List<Node> nodes, List<Link> links, double k = 0.1, double temperature = 200, double coolingFactor = 0.99)
    {
        this.nodes = nodes;
        this.links = links;
        this.k = k;
        this.temperature = temperature;
        this.coolingFactor = coolingFactor;
    }

    private double CalculateDistance(Node a, Node b)
    {
        return Math.Max(Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2)), 0.001);
    }

    private void ApplyCoulombForces(Node node, List<Node> allNodes)
    {
        foreach (Node otherNode in allNodes)
        {
            if (node != otherNode)
            {
                double dx = otherNode.X - node.X;
                double dy = otherNode.Y - node.Y;
                double distance = CalculateDistance(node, otherNode);
                double force = k * k / distance;
                node.Dx += dx / distance * force / node.Mass;
                node.Dy += dy / distance * force / node.Mass;
            }
        }
    }

    private void ApplyHookesLaw(Link link)
    {
        double dx = link.Target.X - link.Source.X;
        double dy = link.Target.Y - link.Source.Y;
        double distance = CalculateDistance(link.Target, link.Source);
        double force = distance * distance / k;
        double fx = dx / distance * force;
        double fy = dy / distance * force;
        link.Source.Dx += fx / link.Source.Mass;
        link.Source.Dy += fy / link.Source.Mass;
        link.Target.Dx -= fx / link.Target.Mass;
        link.Target.Dy -= fy / link.Target.Mass;
    }

    public void Layout()
    {
        Random random = new();
        foreach (Node node in nodes)
        {
            node.X = random.NextDouble();
            node.Y = random.NextDouble();
        }

        while (temperature > 0.1)
        {
            foreach (Node node in nodes)
            {
                node.Dx = 0;
                node.Dy = 0;
                ApplyCoulombForces(node, nodes);
            }

            foreach (Link link in links)
            {
                ApplyHookesLaw(link);
            }

            // Move nodes based on the accumulated forces
            foreach (Node node in nodes)
            {
                // Damping factor helps to stabilize the layout
                node.X += node.Dx * temperature;
                node.Y += node.Dy * temperature;

                // Avoid node collisions
                foreach (Node otherNode in nodes)
                {
                    if (node != otherNode)
                    {
                        double dx = otherNode.X - node.X;
                        double dy = otherNode.Y - node.Y;
                        double distance = CalculateDistance(node, otherNode);
                        double collisionDistance = 0.1; // Minimum distance to avoid collision
                        if (distance < collisionDistance)
                        {
                            double moveDistance = (collisionDistance - distance) / distance * 0.5;
                            node.X -= dx * moveDistance;
                            node.Y -= dy * moveDistance;
                        }
                    }
                }
            }

            // Cool down the system by reducing the temperature
            temperature *= coolingFactor;
        }
    }
}

public class SampleProgram
{
    static void Main()
    {
        Node node1 = new Node(1);
        Node node2 = new Node(2);
        Node node3 = new Node(3);
        List<Node> nodes = new List<Node> { node1, node2, node3 };

        Link link1 = new Link(node1, node2);
        Link link2 = new Link(node2, node3);
        List<Link> links = new List<Link> { link1, link2 };

        ForceLayout forceLayout = new ForceLayout(nodes, links, k: 0.1, temperature: 200, coolingFactor: 0.99);
        forceLayout.Layout();

        foreach (Node node in nodes)
        {
            Console.WriteLine($"Node {node.Id} - X: {node.X}, Y: {node.Y}");
        }
    }
}
