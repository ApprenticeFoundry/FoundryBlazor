# FoundryBlazor Library - Copilot Reference Guide

please create a reference document for this library that a copilot could use to make decisions about calling functions or integrating this library

## Overview

FoundryBlazor is a sophisticated Blazor library that provides Visio-like diagramming and graphics capabilities with both 2D and 3D rendering support. It integrates Three.js for 3D graphics and provides a comprehensive framework for building interactive diagramming applications in Blazor.

## Core Architecture

### Key Components
- **2D Graphics Engine**: Canvas-based 2D rendering with shape management
- **3D Graphics Engine**: Three.js integration for 3D scene rendering
- **Blazor Integration**: Native Blazor components and services
- **Workspace Management**: Hierarchical workspace/workbook architecture
- **Shape System**: Comprehensive 2D and 3D shape libraries
- **Interaction System**: User interaction handling (drag, resize, connect, etc.)
- **Animation Engine**: Tween-based animation system
- **Persistence Layer**: Save/load functionality for diagrams

### Primary Use Cases
1. **Diagramming Applications**: Flowcharts, org charts, network diagrams
2. **CAD/Design Tools**: 2D and 3D design interfaces
3. **Interactive Dashboards**: Data visualization with interactive elements
4. **Educational Tools**: Interactive learning environments
5. **Process Modeling**: Business process visualization

## Core Interfaces and Classes

### Foundational Interfaces

#### IWorkspace
Central interface for workspace management
```csharp
public interface IWorkspace
{
    string Name { get; set; }
    ICollection<IWorkbook> Workbooks { get; }
    void AddWorkbook(IWorkbook workbook);
    void RemoveWorkbook(IWorkbook workbook);
}
```

#### IDrawing
Base interface for all drawing surfaces
```csharp
public interface IDrawing
{
    string Id { get; set; }
    string Name { get; set; }
    ICollection<IShape> Shapes { get; }
    void AddShape(IShape shape);
    void RemoveShape(IShape shape);
}
```

#### IShape2D / IShape3D
Core shape interfaces for 2D and 3D objects
```csharp
public interface IShape2D : IShape
{
    Vector2 Position { get; set; }
    Vector2 Size { get; set; }
    float Rotation { get; set; }
    // Drawing and hit-testing methods
}

public interface IShape3D : IShape
{
    Vector3 Position { get; set; }
    Vector3 Scale { get; set; }
    Vector3 Rotation { get; set; }
    // 3D-specific methods
}
```

### Key Services

#### FoundryService
Main service for library initialization and coordination
- **Purpose**: Central service coordination and lifecycle management
- **When to use**: Always required for library initialization
- **Key methods**: Initialize(), GetWorkspace(), CreateDrawing()

#### SelectionService
Manages shape selection state
- **Purpose**: Track and manage selected shapes across the application
- **When to use**: When implementing selection functionality
- **Key methods**: Select(), Deselect(), GetSelected(), ClearSelection()

#### HitTestService
Handles mouse/touch interaction detection
- **Purpose**: Determine which shapes are under cursor/touch points
- **When to use**: For implementing custom interactions or tools
- **Key methods**: HitTest(), GetShapeAt(), GetShapesInArea()

#### PanZoomService
Manages viewport navigation
- **Purpose**: Handle panning and zooming of the drawing canvas
- **When to use**: For implementing navigation controls
- **Key methods**: Pan(), Zoom(), FitToView(), ResetView()

#### CommandService
Implements command pattern for undo/redo
- **Purpose**: Manage command execution and history
- **When to use**: When implementing undo/redo functionality
- **Key methods**: Execute(), Undo(), Redo(), CanUndo(), CanRedo()

## Shape Libraries

### 2D Shapes

#### Basic Shapes
- **FoRectangle2D**: Rectangle shapes with rounded corners support
- **FoEllipse2D**: Circles and ellipses
- **FoLine2D**: Lines with styling options
- **FoText2D**: Text rendering with formatting
- **FoImage2D**: Bitmap image display
- **FoPath2D**: Custom vector paths

#### Composite Shapes
- **FoGroup2D**: Container for grouping multiple shapes
- **FoCompound2D**: Complex shapes with multiple visual elements
- **FoButton2D**: Interactive button controls
- **FoMenu2D**: Context menu implementations

#### Diagramming Shapes
- **FoConnector1D**: Connecting lines between shapes
- **FoConnectionPoint2D**: Connection endpoints
- **FoGlue2D**: Shape connection management
- **FoHandle2D**: Resize and manipulation handles

#### Layout Shapes
- **FoLayoutGroup**: Container with automatic layout
- **FoLayoutTree**: Tree-based hierarchical layout
- **FoLayoutNetwork**: Network/graph layout
- **FoLayoutSwimLanes**: Swimlane diagram layout

### 3D Shapes
- **FoMesh3D**: 3D mesh objects
- **FoCube3D**: 3D cube primitives
- **FoSphere3D**: 3D sphere primitives
- **FoGroup3D**: 3D shape grouping
- **FoModel3D**: Complex 3D model loading

## JavaScript Interop

### JSInteropHelper
Utility class for Blazor-JavaScript communication
- **Purpose**: Simplify JavaScript function calls from Blazor
- **Key methods**: InvokeAsync(), RegisterCallback(), GetElementById()

### Three.js Integration
- Native Three.js scene management
- WebGL rendering pipeline
- Camera and lighting controls
- Material and texture management
- Animation and physics integration

## Animation System

### Tween Engine
Based on GlideTween library for smooth animations
- **Ease**: Easing function definitions
- **Tween**: Individual animation definitions
- **Tweener**: Animation execution engine

#### Common Animation Patterns
```csharp
// Position animation
var tween = new Tween(shape, "Position", targetPosition, duration);
Tweener.Instance.AddTween(tween);

// Scale animation with easing
var scaleTween = new Tween(shape, "Scale", targetScale, duration)
    .SetEase(Ease.OutQuad);
```

## Event System

### Core Events
- **ShapeUIEvent**: Shape interaction events
- **RefreshUIEvent**: UI refresh notifications
- **SelectionChanged**: Selection state changes
- **UploadFileEvent**: File upload handling

### Message System
Comprehensive message passing for distributed scenarios:
- **D2D_Create**: Shape creation messages
- **D2D_Move**: Shape movement messages
- **D2D_UserMove**: User-initiated movements
- **D2D_ModelUpdate**: Model change notifications

## Integration Patterns

### Basic Setup
```csharp
// In Program.cs or Startup.cs
services.AddFoundryBlazor();
services.AddScoped<FoundryService>();
services.AddScoped<SelectionService>();
```

### Component Usage
```razor
@page "/diagram"
@inject FoundryService FoundryService

<FoundryCanvas2D @ref="canvas" 
                 Width="800" 
                 Height="600" 
                 OnShapeClicked="HandleShapeClick" />

@code {
    private FoundryCanvas2D canvas;
    
    protected override async Task OnInitializedAsync()
    {
        await FoundryService.InitializeAsync();
    }
    
    private void HandleShapeClick(IShape2D shape)
    {
        // Handle shape interaction
    }
}
```

### Creating Custom Shapes
```csharp
public class CustomShape2D : FoShape2D
{
    public override void Draw(ICanvas2D canvas)
    {
        // Custom drawing logic
    }
    
    public override bool HitTest(Vector2 point)
    {
        // Custom hit testing
    }
}
```

## Decision Guidelines for Copilots

### When to Use FoundryBlazor
✅ **Recommended for**:
- Interactive diagramming applications
- Data visualization with user interaction
- CAD/design tool interfaces
- Educational interactive content
- Process modeling applications
- Dashboard applications with custom graphics

❌ **Not recommended for**:
- Simple static charts (use Chart.js instead)
- Basic form layouts (use standard Blazor components)
- Text-heavy content (use standard HTML/CSS)
- Performance-critical real-time graphics (consider WebGL directly)

### Architecture Decisions

#### 2D vs 3D Components
- Use **2D components** for: Flowcharts, org charts, UI mockups, process diagrams
- Use **3D components** for: CAD models, architectural visualization, game-like interfaces
- Use **both** for: Mixed reality applications, complex data visualization

#### Shape Selection Strategy
- **Built-in shapes**: Use for rapid prototyping and standard diagrams
- **Custom shapes**: Implement when unique visual requirements exist
- **Composite shapes**: Use for reusable complex elements

#### Performance Considerations
- **Canvas2D**: Better for many small shapes, text-heavy diagrams
- **Canvas3D**: Better for complex 3D scenes, fewer objects with high detail
- **Virtualization**: Consider for diagrams with >1000 shapes

### Common Implementation Patterns

#### Diagram Editor Pattern
```csharp
// Service registration
services.AddScoped<SelectionService>();
services.AddScoped<CommandService>();
services.AddScoped<HitTestService>();

// Component structure
<ToolPalette OnToolSelected="HandleToolSelection" />
<FoundryCanvas2D @ref="canvas" OnShapeInteraction="HandleInteraction" />
<PropertyPanel SelectedShape="@selectedShape" />
```

#### Data Binding Pattern
```csharp
// Bind diagram to data model
public void UpdateDiagramFromData(List<NodeData> nodes)
{
    foreach (var node in nodes)
    {
        var shape = new FoRectangle2D
        {
            Position = node.Position,
            Text = node.Label,
            Data = node // Bind data to shape
        };
        canvas.AddShape(shape);
    }
}
```

#### Real-time Collaboration Pattern
```csharp
// Using SignalR hub integration
[Hub]
public class DrawingHub : Hub<IDrawingClient>
{
    public async Task SendShapeUpdate(string drawingId, ShapeUpdate update)
    {
        await Clients.Group(drawingId).ReceiveShapeUpdate(update);
    }
}
```

## Best Practices

### Performance
1. **Shape Batching**: Group shape operations for better performance
2. **Virtualization**: Use viewport culling for large diagrams
3. **Memory Management**: Dispose shapes and resources properly
4. **Update Throttling**: Batch UI updates during animations

### User Experience
1. **Progressive Loading**: Load complex diagrams incrementally
2. **Responsive Design**: Adapt to different screen sizes
3. **Keyboard Navigation**: Implement accessibility features
4. **Touch Support**: Handle mobile interactions

### Code Organization
1. **Separation of Concerns**: Keep data models separate from visual shapes
2. **Command Pattern**: Use for all user actions that modify state
3. **Event-Driven**: Use events for loose coupling between components
4. **Configuration**: Make visual styles configurable

## Common Pitfalls

### JavaScript Interop Issues
- Always check for null references in JS interop calls
- Handle async operations properly
- Manage JavaScript object lifecycle

### Memory Leaks
- Dispose event handlers and subscriptions
- Clear shape collections when removing from canvas
- Manage JavaScript object references

### Performance Problems
- Avoid frequent canvas redraws
- Use shape caching for complex graphics
- Implement proper hit-testing optimization

## Version Compatibility

### Dependencies
- **.NET**: 6.0 or higher
- **Blazor**: Server or WebAssembly
- **Three.js**: Bundled version (check package.json)
- **Browser**: Modern browsers with WebGL support

### Breaking Changes
- Check NUGETREADME.md for version-specific changes
- Review migration guides for major version updates

## Support and Resources

### Documentation
- API documentation in XML comments
- Sample projects in Solutions/ folder
- README.md for quick start guide

### Troubleshooting
- Check browser console for JavaScript errors
- Verify WebGL support for 3D features
- Use browser dev tools for performance profiling

This reference guide provides the essential information needed for making informed decisions about integrating and using the FoundryBlazor library effectively.
