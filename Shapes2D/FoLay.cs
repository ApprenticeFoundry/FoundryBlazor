using System;
using System.Collections.Generic;
using System.Drawing;

namespace ForceDirectedGraphLayout {

  class Node {
    public string Id { get; set; }
    public double Mass { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public Node(string id, double mass, double x, double y) {
      Id = id;
      Mass = mass;
      X = x;
      Y = y; 
    }
  }

  class Edge {
    public Node Node1 { get; set; }
    public Node Node2 { get; set; }
    public double Length { get; set; }
    public double Strength { get; set; }

    public Edge(Node node1, Node node2, double length, double strength) {
      Node1 = node1;
      Node2 = node2;
      Length = length;
      Strength = strength;
    }
  }

  class ForceDirectedGraph {
    
    private List<Node> nodes = new List<Node>();
    private List<Edge> edges = new List<Edge>();

    private double SpringConstant = 0.2;
    private double RepulsionConstant = 10000; 
    private double Damping = 0.95;
    
    public ForceDirectedGraph() {
    }

    public void AddNode(string id, double mass, double x, double y) {
      nodes.Add(new Node(id, mass, x, y));
    }

    public void AddEdge(string id1, string id2, double length, double strength) {
      var node1 = nodes.Find(n => n.Id == id1);
      var node2 = nodes.Find(n => n.Id == id2);
      edges.Add(new Edge(node1, node2, length, strength)); 
    }

    private void ApplyForces() {
      foreach (var edge in edges) {
        ApplySpringForce(edge);  
      }

      foreach (var node1 in nodes) {
        foreach (var node2 in nodes) {
          if (node1 != node2) {
            ApplyRepulsionForce(node1, node2);
          }
        }
      }
    }

    private void ApplySpringForce(Edge edge) {
      double deltaX = edge.Node1.X - edge.Node2.X;
      double deltaY = edge.Node1.Y - edge.Node2.Y;
      
      double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
      if (distance == 0) return;

      double directionX = deltaX / distance; 
      double directionY = deltaY / distance;

      double springForce = (distance - edge.Length) * (SpringConstant * edge.Strength);

      edge.Node1.X -= directionX * springForce;
      edge.Node1.Y -= directionY * springForce;

      edge.Node2.X += directionX * springForce;
      edge.Node2.Y += directionY * springForce;
    }

    private void ApplyRepulsionForce(Node node1, Node node2) {
      double deltaX = node1.X - node2.X;
      double deltaY = node1.Y - node2.Y;
      
      double distanceSquared = deltaX * deltaX + deltaY * deltaY;
      if (distanceSquared == 0) return;

      double distance = Math.Sqrt(distanceSquared);
      double directionX = deltaX / distance;
      double directionY = deltaY / distance;

      double repulsionForce = RepulsionConstant * node1.Mass * node2.Mass / distanceSquared;
      
      node1.X -= directionX * repulsionForce;
      node1.Y -= directionY * repulsionForce;

      node2.X += directionX * repulsionForce;
      node2.Y += directionY * repulsionForce;
    }

    public void Layout(int numIterations) {
      var random = new Random();
      
      foreach (var node in nodes) {
        node.X = random.NextDouble() * 1000;
        node.Y = random.NextDouble() * 1000;
      }

      for (int i = 0; i < numIterations; i++) {
        ApplyForces();
        UpdatePositions();
      }
    }

    private void UpdatePositions() {
      foreach (var node in nodes) {
        node.X += (500 - node.X) * Damping;
        node.Y += (500 - node.Y) * Damping;
      }
    }

  }

//   class Program {
//     static void Main(string[] args) {
      
//       var graph = new ForceDirectedGraph();
      
//       graph.AddNode("A", 2, 0, 0);
//       graph.AddNode("B", 1, 0, 0);
//       graph.AddNode("C", 3, 0, 0);
      
//       graph.AddEdge("A", "B", 70, 1);
//       graph.AddEdge("B", "C", 80, 1); 
//       graph.AddEdge("A", "C", 100, 1);

//       graph.Layout(100);

//       // Print node positions
//       foreach (var node in graph.nodes) {
//         Console.WriteLine($"{node.Id}: ({node.X}, {node.Y})");
//       }
//     }
//   }

}