using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;

// https://happycoding.io/tutorials/processing/collision-detection
// https://bytes.com/topic/c-sharp/insights/880968-generic-quadtree-implementation

namespace FoundryBlazor.Shape;

public static class QuadTargetExtensions
{
    private static readonly Queue<QuadHitTarget> targets = new();

    public static QuadHitTarget NewHitTarget(ICanHitTarget target, Rectangle rect)
    {
        if (targets.Count == 0)
            return new QuadHitTarget(rect, target);

        //$"Recycle QuadTree {cashe.Count}".WriteInfo();
        var result = targets.Dequeue();
        result.Update(rect, target);
        return result;
    }
    public static QuadHitTarget NewHitTarget(ICanHitTarget target, Point point1, Point point2)
    {
        if (targets.Count == 0)
            return new QuadHitTarget(point1, point2, target);

        //$"Recycle QuadTree {cashe.Count}".WriteInfo();
        var result = targets.Dequeue();
        result.Update(point1, point2, target);
        return result;
    }

    public static void PurgeHitTarget(QuadHitTarget source)
    {
        if (source == null) return;

        source.Purge();
        targets.Enqueue(source);
    }
}


public class QuadTree<T> where T : QuadHitTarget
{
    
    private const int MAX_OBJECTS_PER_NODE = 2;

    private List<QuadHitTarget>? m_objects = null;       // The objects in this QuadTree
    private Rectangle m_rect;               // The area this QuadTree represents

    private QuadTree<T>? m_childTL = null;   // Top Left Child
    private QuadTree<T>? m_childTR = null;   // Top Right Child
    private QuadTree<T>? m_childBL = null;   // Bottom Left Child
    private QuadTree<T>? m_childBR = null;   // Bottom Right Child


    private static readonly Queue<QuadTree<T>> quadTreeCache = new();
    public static QuadTree<T> NewQuadTree(int x, int y, int width, int height)
    {
        if (quadTreeCache.Count == 0)
            return new QuadTree<T>(x, y, width, height);
    
        //$"Recycle QuadTree {cashe.Count}".WriteInfo();
        var result = quadTreeCache.Dequeue();
        result.Reset(x, y, width, height);
        return result;
    }

    public static QuadTree<T>? SmashQuadTree(QuadTree<T>? source)
    {
        if (source == null) return null;

        source.Clear(true);
        quadTreeCache.Enqueue(source);
       // $"Smash QuadTree {cashe.Count}".WriteNote();
        return null;
    }





 

    public Rectangle QuadRect { get { return m_rect; } }

    public QuadTree<T>? TopLeftChild { get { return m_childTL; } }
    public QuadTree<T>? TopRightChild { get { return m_childTR; } }
    public QuadTree<T>? BottomLeftChild { get { return m_childBL; } }
    public QuadTree<T>? BottomRightChild { get { return m_childBR; } }
    public List<QuadHitTarget> Members() {return m_objects ?? new List<QuadHitTarget>(); }

    public List<Rectangle> HitRectangles() 
    { 
        return Members().Where(obj => obj.IsRectangle()).Select(obj => obj.rectangle).ToList(); 
    }

    public int Count { get { return this.ObjectCount(); } }




    public QuadTree(Rectangle rect)
    {
        m_rect = rect;
    }
    public QuadTree(int x, int y, int width, int height)
    {
        m_rect = new Rectangle(x, y, width, height);
    }



    public QuadTree<T> Reset(int x, int y, int width, int height)
    {
        m_rect.X = x;   
        m_rect.Y = y;
        m_rect.Width = width;
        m_rect.Height = height;
        return this;
    }

    public bool IsSmashed()
    {
        var count = Members()?.Count(obj => obj.target.IsSmashed());
        var smashed = count > 0;
        if ( !HasSubTrees() ) 
            return smashed;

        if ( m_childTL!.IsSmashed() || m_childTR!.IsSmashed() || m_childBL!.IsSmashed() || m_childBR!.IsSmashed() )
            smashed = true;

        return smashed;
    }

 

    public async Task DrawQuadTree(Canvas2DContext ctx, bool drawMembers = true)
    {

        await ctx.SetStrokeStyleAsync("Green");
        await ctx.StrokeRectAsync(m_rect.X+1, m_rect.Y+1, m_rect.Width-2, m_rect.Height-2);

        if ( drawMembers )
        {
            await ctx.SaveAsync();
            await ctx.SetFillStyleAsync("Yellow");
            await ctx.SetGlobalAlphaAsync(0.5f);

            Members().ForEach(async item =>
            {
                await item.DrawQuadHitTarget(ctx);
            });
            await ctx.RestoreAsync();
        }

        if ( !HasSubTrees() ) 
            return;

        await TopLeftChild!.DrawQuadTree(ctx,drawMembers);
        await TopRightChild!.DrawQuadTree(ctx,drawMembers);
        await BottomLeftChild!.DrawQuadTree(ctx,drawMembers);
        await BottomRightChild!.DrawQuadTree(ctx,drawMembers);
    }


    private void AddToQuad(QuadHitTarget obj)
    {
        m_objects ??= new List<QuadHitTarget>();
        m_objects.Add(obj);
        //$"Tree AddToQuad {m_rect} {m_objects.Count} {obj}".WriteInfo(2);
    }


    private void RemoveFromQuad(QuadHitTarget item)
    {
        if (m_objects == null) 
            return;
        
        m_objects.RemoveAll(obj => obj.Equals(item));
    }


    private int ObjectCount()
    {
        // Add the objects at this level
        int count = Members().Count();

        // Add the objects that are contained in the children
        if ( HasSubTrees() )
        {
            count += m_childTL!.ObjectCount();
            count += m_childTR!.ObjectCount();
            count += m_childBL!.ObjectCount();
            count += m_childBR!.ObjectCount();
        }

        return count;
    }



    private void Subdivide()
    {
        //$"Tree Subdivide items".WriteInfo(2);

        // We've reached capacity, subdivide...
        Point size = new(m_rect.Width / 2, m_rect.Height / 2);
        Point mid = new(m_rect.X + size.X, m_rect.Y + size.Y);

        m_childTL = NewQuadTree(m_rect.Left, m_rect.Top, size.X, size.Y);
        m_childTR = NewQuadTree(mid.X, m_rect.Top, size.X, size.Y);
        m_childBL = NewQuadTree(m_rect.Left, mid.Y, size.X, size.Y);
        m_childBR = NewQuadTree(mid.X, mid.Y, size.X, size.Y);

        // If they're completely contained by the quad, bump objects down
        for (int i = 0; i < m_objects?.Count; i++)
        {
            var item = m_objects[i];
            var destTree = GetDestinationTree(item);

            if (destTree != this)
            {
                // Insert to the appropriate tree, remove the object, and back up one in the loop
                destTree.Insert(item);
                RemoveFromQuad(item);
                i--;
            }
        }
    }


    private QuadTree<T> GetDestinationTree(QuadHitTarget obj)
    {
        // If a child can't contain an object, it will live in this Quad
        QuadTree<T> destTree = this;

        if ( obj.IsContainedBy(m_childTL!.QuadRect) )
        {
            destTree = m_childTL;
        }
        else if ( obj.IsContainedBy(m_childTR!.QuadRect) )
        {
            destTree = m_childTR;
        }
        else if ( obj.IsContainedBy(m_childBL!.QuadRect) )
        {
            destTree = m_childBL;
        }
        else if ( obj.IsContainedBy(m_childBR!.QuadRect) )
        {
            destTree = m_childBR;
        }

        return destTree;
    }



    public QuadTree<T> Clear(bool force)
    {
        // the question is:  do we want to clear the tree if it has no smashed objects?

        //$"Tree Clear items {m_rect} {Count} {force}".WriteInfo(2);
        var smashed = IsSmashed();

        //no smashed objects, no need to clear
        if ( smashed && !force && !HasSubTrees()) 
            return this;

        // Clear any objects at this level
        if ( smashed || force)
        {
            m_objects?.ForEach(obj => QuadTargetExtensions.PurgeHitTarget(obj));
            m_objects?.Clear();
        }

        if ( HasSubTrees() )
        {
            m_childTL?.Clear(force);
            m_childTR?.Clear(force);
            m_childBL?.Clear(force);
            m_childBR?.Clear(force);
            
            // Set the children to null
            if ( force )
            {
                m_childTL = SmashQuadTree(m_childTL);
                m_childTR = SmashQuadTree(m_childTR);
                m_childBL = SmashQuadTree(m_childBL);
                m_childBR = SmashQuadTree(m_childBR);
            }
        }

        return this;
    }


    public void Delete(QuadHitTarget item)
    {
        // If this level contains the object, remove it
        bool objectRemoved = false;
        if (Members().Count(obj => obj.Equals(item)) > 0)
        {
            RemoveFromQuad(item);
            objectRemoved = true;
        }

        // If we didn't find the object in this tree, try to delete from its children
        if (!objectRemoved)
        {
            m_childTL?.Delete(item);
            m_childTR?.Delete(item);
            m_childBL?.Delete(item);
            m_childBR?.Delete(item);
        }

        if (HasSubTrees())
        {
            // If all the children are empty, delete all the children
            if (m_childTL!.Count == 0 &&
                m_childTR!.Count == 0 &&
                m_childBL!.Count == 0 &&
                m_childBR!.Count == 0)
            {
                m_childTL = SmashQuadTree(m_childTL);
                m_childTR = SmashQuadTree(m_childTR);
                m_childBL = SmashQuadTree(m_childBL);
                m_childBR = SmashQuadTree(m_childBR);
            }
        }
    }


    public bool HasSubTrees()
    {
        return m_childTL != null;
    }

    public void Insert(QuadHitTarget obj)
    {
        //$"Tree Inserting {obj} items".WriteInfo(2);

        // If this quad doesn't intersect the items rectangle, do nothing
        if ( !obj.IsContainedBy(QuadRect) )
            return;


        if (m_objects == null )
            AddToQuad(obj); // If there's room to add the object, just add it   

        else if ( m_objects.Count + 1 <= MAX_OBJECTS_PER_NODE && HasSubTrees() == false)
            AddToQuad(obj); // If there's room to add the object, just add it

        else
        {
            // No quads, create them and bump objects down where appropriate
            if ( !HasSubTrees() )
                Subdivide();
            

            // Find out which tree this object should go in and add it there
            var destTree = GetDestinationTree(obj);
            if (destTree == this)
                AddToQuad(obj);
            else
                destTree.Insert(obj);
        }
    }

    // public QuadHitTarget? FindNearestMember()
    // {
    //     QuadHitTarget? nearestLine = null;
    //     double minDistance = double.MaxValue;

    //     foreach (var member in Members())
    //     {
    //         if (member.LastDistance < minDistance)
    //         {
    //             minDistance = member.LastDistance;
    //             nearestLine = member;
    //         }
    //     }
    //     return nearestLine;
    // }


    public void QueryObjects(Rectangle range, ref List<QuadHitTarget> results)
    {
        // We can't do anything if the results list doesn't exist
        if (results == null) return;
        //$"Tree Query Objects {m_rect} CT:{Members().Count} {range}".WriteInfo(2);
        
        if (range.Contains(QuadRect))
        {
            // If the search area completely contains this quad, just get every object this quad and all it's children have
            GetAllObjects(ref results);
            foreach (var member in Members())
            {
                // if line hit is withing range select that QuadHitTarget
                member.IsIntersectedBy(range, 10.0);
            }           
        }
        else if (range.IntersectsWith(QuadRect))
        {
            // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle

            //$"Tree Search Objects {m_rect} {m_objects.Count} {rect}".WriteInfo(2);
            foreach (var member in Members())
            {
                // if line hit is withing range select that QuadHitTarget
                if (member.IsIntersectedBy(range, 10.0))
                    results.Add(member);
            }


                    
            // Get the objects for the search rectangle from the children
            if ( HasSubTrees() )
            {
                m_childTL?.QueryObjects(range, ref results);
                m_childTR?.QueryObjects(range, ref results);
                m_childBL?.QueryObjects(range, ref results);
                m_childBR?.QueryObjects(range, ref results);
            }
        }
        
    }


    public void GetAllObjects(ref List<QuadHitTarget> results)
    {
        // If this Quad has objects, add them
        results.AddRange(Members());

        // If we have children, get their objects too
        if ( HasSubTrees() )
        {
            m_childTL?.GetAllObjects(ref results);
            m_childTR?.GetAllObjects(ref results);
            m_childBL?.GetAllObjects(ref results);
            m_childBR?.GetAllObjects(ref results);
        }

    }


    public void QueryRange(Rectangle range, ref List<QuadHitTarget> results)
    {

        if (!range.IntersectsWith(QuadRect))
            return;

        foreach (var segment in Members())
        {
            if (segment.SegmentIntersects(range))
                results.Add(segment);
        }

        if ( HasSubTrees() )
        {
            m_childTL?.QueryRange(range, ref results);
            m_childTR?.QueryRange(range, ref results);
            m_childBL?.QueryRange(range, ref results);
            m_childBR?.QueryRange(range, ref results);
        }
    }   

    public void QueryPoint(Point point, ref List<QuadHitTarget> results)
    {

        if (!QuadRect.Contains(point))
            return;

        foreach (var segment in Members())
        {
            if (segment.SegmentContainsPoint(point))
                results.Add(segment);
        }

        if ( HasSubTrees() )
        {
            m_childTL?.QueryPoint(point, ref results);
            m_childTR?.QueryPoint(point, ref results);
            m_childBL?.QueryPoint(point, ref results);
            m_childBR?.QueryPoint(point, ref results);
        };
    }

}

