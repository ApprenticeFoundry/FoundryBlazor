
# FoundryBlazor

A comprehensive C# / Blazor diagramming library that combines 2D and 3D visualization capabilities for developers.

## Overview

FoundryBlazor is a powerful diagramming and visualization library that brings together the best features of Visio, Three.js, and CesiumJS into the Blazor ecosystem. Originally demonstrated at NDC Oslo 2023, it's now available as a NuGet package supporting both Blazor Server and WebAssembly applications.

## Features

- **2D Diagramming**: Complete shape library with advanced layout algorithms
- **3D Integration**: Seamless 2D/3D visualization combining
- **Glued Connections**: Dynamic connections that maintain relationships during layout changes
- **Multi-page Diagrams**: Scaled diagrams across multiple pages
- **Performance Optimized**: Object pooling and efficient rendering
- **Cross-platform**: Works in both Blazor Server and WebAssembly

## Installation

```bash
dotnet add package ApprenticeFoundryBlazor
```

## Quick Start

### 1. Configure Services

Add FoundryBlazor services to your `Program.cs`:

```csharp
using FoundryBlazor;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // For Blazor Server

// Add FoundryBlazor services
builder.Services.AddFoundryBlazorServices();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseStaticFiles(); // Essential for static assets
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

### 2. Add Required Static Assets

Add these references to your `_Host.cshtml` (Blazor Server) or `index.html` (WebAssembly):

```html
<!-- CSS References -->
<link href="_content/ApprenticeFoundryBlazor/css/bootstrap/bootstrap.min.css" rel="stylesheet" />
<link href="_content/ApprenticeFoundryBlazor/css/site.css" rel="stylesheet" />
<link href="_content/Radzen.Blazor/css/material-base.css" rel="stylesheet" />

<!-- JavaScript References (before closing </body> tag) -->
<script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
<script src="_content/ApprenticeFoundryBlazorThreeJS/js/blazor-three-js.js"></script>
<script src="_content/ApprenticeFoundryBlazor/js/app-lib.js"></script>
<script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
```

### 3. Using 2D Canvas Component

```razor
@page "/canvas2d-demo"
@using FoundryBlazor.Shared
@using FoundryBlazor.Shape
@inject IWorkspace Workspace

<h3>2D Canvas Diagramming</h3>

<div class="container-fluid">
    <div class="row">
        <!-- Shape Tree View -->
        <div class="col-3">
            <div style="height: 600px; border: 1px solid #ccc;">
                <ShapeTreeView />
            </div>
        </div>
        
        <!-- 2D Canvas -->
        <div class="col-9">
            <Canvas2DComponent SceneName="MainCanvas2D"
                               CanvasWidth="1200" 
                               CanvasHeight="600"
                               WithAnimations="true" />
        </div>
    </div>
</div>

<div class="mt-3">
    <button class="btn btn-primary" @onclick="AddRectangle">Add Rectangle</button>
    <button class="btn btn-success" @onclick="AddCircle">Add Circle</button>
    <button class="btn btn-warning" @onclick="ClearCanvas">Clear Canvas</button>
</div>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Initialize with some default shapes
        var drawing = Workspace.GetDrawing();
        
        // Add a rectangle
        var rect = new FoShape2D("Rectangle1", 120, 80, "Blue");
        rect.PinX = 200;
        rect.PinY = 150;
        drawing?.AddShape(rect);
        
        // Add a circle (using rectangle with equal width/height for simplicity)
        var circle = new FoShape2D("Circle1", 100, 100, "Red");
        circle.PinX = 400;
        circle.PinY = 200;
        drawing?.AddShape(circle);
        
        await base.OnInitializedAsync();
    }
    
    private void AddRectangle()
    {
        var drawing = Workspace.GetDrawing();
        var random = new Random();
        
        var rect = new FoShape2D($"Rectangle_{DateTime.Now.Ticks}", 
                                 80 + random.Next(40), 
                                 60 + random.Next(40), 
                                 "Green");
        rect.PinX = 100 + random.Next(800);
        rect.PinY = 100 + random.Next(400);
        
        drawing?.AddShape(rect);
        StateHasChanged();
    }
    
    private void AddCircle()
    {
        var drawing = Workspace.GetDrawing();
        var random = new Random();
        var size = 60 + random.Next(40);
        
        var circle = new FoShape2D($"Circle_{DateTime.Now.Ticks}", size, size, "Orange");
        circle.PinX = 100 + random.Next(800);
        circle.PinY = 100 + random.Next(400);
        
        drawing?.AddShape(circle);
        StateHasChanged();
    }
    
    private void ClearCanvas()
    {
        var drawing = Workspace.GetDrawing();
        drawing?.ClearAll();
        StateHasChanged();
    }
}
```

### 4. Using 3D Canvas Component

```razor
@page "/canvas3d-demo"
@using FoundryBlazor.Shared
@using FoundryBlazor.Shape
@inject IWorkspace Workspace

<h3>3D Canvas Visualization</h3>

<div class="container-fluid">
    <div class="row">
        <!-- 3D Canvas -->
        <div class="col-8">
            <Canvas3DComponent SceneName="MainCanvas3D"
                               CanvasWidth="800"
                               CanvasHeight="600"
                               WithAnimations="true" />
        </div>
        
        <!-- Controls -->
        <div class="col-4">
            <div class="card">
                <div class="card-header">
                    <h5>3D Controls</h5>
                </div>
                <div class="card-body">
                    <button class="btn btn-primary mb-2 w-100" @onclick="Add3DShape">Add 3D Shape</button>
                    <button class="btn btn-success mb-2 w-100" @onclick="AnimateShapes">Animate Shapes</button>
                    <button class="btn btn-warning mb-2 w-100" @onclick="ResetCamera">Reset Camera</button>
                    <button class="btn btn-danger mb-2 w-100" @onclick="Clear3DScene">Clear Scene</button>
                    
                    <hr />
                    
                    <h6>Camera Controls:</h6>
                    <p><small>
                        • Left Mouse: Rotate<br/>
                        • Right Mouse: Pan<br/>
                        • Scroll: Zoom
                    </small></p>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private int shapeCounter = 0;
    
    protected override async Task OnInitializedAsync()
    {
        // Initialize 3D scene with some basic shapes
        var arena = Workspace.GetArena();
        
        // Add some initial 3D shapes programmatically
        // Note: Actual 3D shape creation depends on your 3D shape classes
        
        await base.OnInitializedAsync();
    }
    
    private void Add3DShape()
    {
        var arena = Workspace.GetArena();
        var random = new Random();
        
        // Example of adding 3D shapes (adjust based on your actual 3D shape classes)
        shapeCounter++;
        
        // This is conceptual - replace with actual 3D shape creation
        // var shape3D = new FoShape3D($"Shape3D_{shapeCounter}");
        // shape3D.Position = new Vector3(
        //     random.Next(-5, 5), 
        //     random.Next(-5, 5), 
        //     random.Next(-5, 5)
        // );
        // arena?.AddShape(shape3D);
        
        StateHasChanged();
    }
    
    private void AnimateShapes()
    {
        // Trigger animations on 3D shapes
        // Implementation depends on your animation system
        StateHasChanged();
    }
    
    private void ResetCamera()
    {
        // Reset 3D camera to default position
        // Implementation depends on your camera system
        StateHasChanged();
    }
    
    private void Clear3DScene()
    {
        var arena = Workspace.GetArena();
        arena?.ClearAll();
        StateHasChanged();
    }
}
```

### 5. Combined 2D and 3D Canvas Example

```razor
@page "/combined-canvas"
@using FoundryBlazor.Shared
@using FoundryBlazor.Shape
@inject IWorkspace Workspace

<h3>Combined 2D & 3D Visualization</h3>

<div class="container-fluid">
    <div class="row">
        <!-- Tree View -->
        <div class="col-2">
            <div style="height: 500px;">
                <RadzenShapeTreeView />
            </div>
        </div>
        
        <!-- 2D Canvas -->
        <div class="col-5">
            <h5>2D Diagram</h5>
            <Canvas2DComponent SceneName="Combined2D"
                               CanvasWidth="500" 
                               CanvasHeight="400"
                               WithAnimations="true" />
        </div>
        
        <!-- 3D Canvas -->
        <div class="col-5">
            <h5>3D Visualization</h5>
            <Canvas3DComponent SceneName="Combined3D"
                               CanvasWidth="500"
                               CanvasHeight="400"
                               WithAnimations="true" />
        </div>
    </div>
    
    <div class="row mt-3">
        <div class="col-12">
            <button class="btn btn-primary me-2" @onclick="SyncViews">Sync 2D → 3D</button>
            <button class="btn btn-success me-2" @onclick="AddToBoth">Add to Both</button>
            <button class="btn btn-warning me-2" @onclick="ClearBoth">Clear Both</button>
        </div>
    </div>
</div>

@code {
    private void SyncViews()
    {
        // Example: Convert 2D shapes to 3D representation
        var drawing = Workspace.GetDrawing();
        var arena = Workspace.GetArena();
        
        // Implementation would depend on your specific shape conversion logic
        // This demonstrates the concept of syncing between 2D and 3D
        
        StateHasChanged();
    }
    
    private void AddToBoth()
    {
        var drawing = Workspace.GetDrawing();
        var arena = Workspace.GetArena();
        var random = new Random();
        
        // Add to 2D
        var rect2D = new FoShape2D($"Synced2D_{DateTime.Now.Ticks}", 100, 60, "Purple");
        rect2D.PinX = 100 + random.Next(300);
        rect2D.PinY = 100 + random.Next(200);
        drawing?.AddShape(rect2D);
        
        // Add corresponding 3D shape
        // var shape3D = new FoShape3D($"Synced3D_{DateTime.Now.Ticks}");
        // arena?.AddShape(shape3D);
        
        StateHasChanged();
    }
    
    private void ClearBoth()
    {
        Workspace.GetDrawing()?.ClearAll();
        Workspace.GetArena()?.ClearAll();
        StateHasChanged();
    }
}
```

## Advanced Features

### Event Handling and Inter-Component Communication

```csharp
@using BlazorComponentBus
@using FoundryBlazor.PubSub
@inject ComponentBus PubSub
@implements IDisposable

<Canvas2DComponent SceneName="EventCanvas" />

@code {
    protected override async Task OnInitializedAsync()
    {
        // Subscribe to shape selection events
        PubSub.SubscribeTo<ShapeUIEvent>(OnShapeSelected);
        
        // Subscribe to refresh events
        PubSub.SubscribeTo<RefreshUIEvent>(OnUIRefresh);
        
        await base.OnInitializedAsync();
    }
    
    private async Task OnShapeSelected(ShapeUIEvent eventArgs)
    {
        Console.WriteLine($"Shape selected: {eventArgs.Shape?.Name}");
        // Handle shape selection logic
        StateHasChanged();
    }
    
    private async Task OnUIRefresh(RefreshUIEvent eventArgs)
    {
        // Handle UI refresh
        StateHasChanged();
    }
    
    public void Dispose()
    {
        PubSub?.UnSubscribeFrom<ShapeUIEvent>(OnShapeSelected);
        PubSub?.UnSubscribeFrom<RefreshUIEvent>(OnUIRefresh);
    }
}
```

### Custom Shape Creation

```csharp
// Create custom 2D shapes
public class CustomShape : FoShape2D
{
    public CustomShape(string name) : base(name, "CustomColor")
    {
        // Custom initialization
        Width = 150;
        Height = 100;
    }
    
    // Override drawing behavior if needed
    // Custom rendering logic can be implemented here
}

// Usage in component
private void AddCustomShape()
{
    var drawing = Workspace.GetDrawing();
    var customShape = new CustomShape("MyCustomShape");
    customShape.PinX = 300;
    customShape.PinY = 200;
    drawing?.AddShape(customShape);
}
```

### Performance Optimization

```csharp
// For large datasets, disable animations
<Canvas2DComponent SceneName="LargeDataset"
                   CanvasWidth="1200" 
                   CanvasHeight="800"
                   WithAnimations="false" />

// Batch operations for better performance
private void AddMultipleShapes()
{
    var drawing = Workspace.GetDrawing();
    
    // Batch add multiple shapes
    for (int i = 0; i < 100; i++)
    {
        var shape = new FoShape2D($"BatchShape_{i}", 50, 30, "Blue");
        shape.PinX = (i % 10) * 60 + 50;
        shape.PinY = (i / 10) * 40 + 50;
        drawing?.AddShape(shape);
    }
    
    // Trigger single refresh instead of multiple
    StateHasChanged();
}
```

## Troubleshooting

### Common Issues

**Canvas not rendering:**
- Ensure all required JavaScript files are loaded in the correct order
- Verify `app.UseStaticFiles()` is called in Program.cs
- Check browser console for JavaScript errors

**Services not found:**
```
Unable to resolve service for type 'FoundryBlazor.IWorkspace'
```
**Solution:** Add `services.AddFoundryBlazorServices()` to Program.cs

**Static assets not loading:**
- Verify the `_content/ApprenticeFoundryBlazor/` path is accessible
- Check that static files middleware is properly configured
- Ensure NuGet package is properly restored

### Browser Compatibility

- **Recommended:** Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Canvas2D:** Supported by all modern browsers
- **WebGL (3D):** Required for 3D components, supported by 97%+ of browsers

## Demo & Examples

Visit our interactive demo: https://apprenticefoundry.github.io/

## Architecture Documentation

### Matrix3D Compatibility System
FoundryBlazor uses a compatibility wrapper architecture for 3D matrix operations:

- **[Matrix3D Compatibility Documentation](./MATRIX3D_COMPATIBILITY_DOCUMENTATION.md)** - Complete technical documentation of the Matrix3D wrapper pattern that delegates to BlazorThreeJS.Matrix3 while maintaining API compatibility

### Key Features
- **Delegation Pattern**: Matrix3D serves as a compatibility bridge to BlazorThreeJS mathematics
- **Type Conversion**: Seamless bridging between `FoVector3D` (double precision) and `Vector3` (float precision)
- **Object Pooling**: Performance optimization through matrix instance reuse
- **Zero Breaking Changes**: Existing code works unchanged while benefiting from enhanced math

### Related Projects
- **BlazorThreeJS**: Provides the core mathematical foundation for 3D operations
- **Three2025**: Application project demonstrating integrated 3D capabilities

## Package Information

- **Package Name:** `ApprenticeFoundryBlazor`
- **Current Version:** 23.3.0
- **Target Framework:** .NET 9.0
- **Compatible With:** Blazer Server and WebAssembly

## Requirements

- .NET 9.0 or higher
- Blazor Server or WebAssembly
- Modern web browser with Canvas2D and WebGL support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## Support

- **Demo**: https://apprenticefoundry.github.io/
- **Issues**: Submit issues on GitHub for bug reports and feature requests
- **Documentation**: See the Matrix3D compatibility documentation for technical details