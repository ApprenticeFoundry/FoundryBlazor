# FoundryBlazor Integration Guide

This guide shows how to properly integrate FoundryBlazor components in consuming Blazor applications.

## Installation

```bash
dotnet add package ApprenticeFoundryBlazor
```

## Service Registration

### Basic Setup (Recommended)
```csharp
// Program.cs
using FoundryBlazor;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();  // For Server
// OR
builder.Services.AddBlazorWebAssembly();  // For WASM

// Add FoundryBlazor services
builder.Services.AddFoundryBlazor();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Essential for static assets
app.UseRouting();

app.MapBlazorHub();    // For Server
app.MapFallbackToPage("/_Host");

app.Run();
```

### Advanced Setup with Custom Configuration
```csharp
// Program.cs
using FoundryBlazor;

var builder = WebApplication.CreateBuilder(args);

// Custom environment configuration
var envConfig = new EnvConfig("appsettings.json");
builder.Services.AddFoundryBlazorServices(envConfig);

// Add other services...
```

## Required Static Assets

Add these references to your `_Host.cshtml` (Blazor Server) or `index.html` (Blazor WASM):

### CSS References
```html
<!-- Required CSS files -->
<link href="_content/ApprenticeFoundryBlazor/css/bootstrap/bootstrap.min.css" rel="stylesheet" />
<link href="_content/ApprenticeFoundryBlazor/css/open-iconic/bootstrap/open-iconic-bootstrap.min.css" rel="stylesheet" />
<link href="_content/ApprenticeFoundryBlazor/css/site.css" rel="stylesheet" />

<!-- Radzen Components (included with library) -->
<link href="_content/Radzen.Blazor/css/material-base.css" rel="stylesheet" />
```

### JavaScript References
```html
<!-- Required JavaScript files - Add before closing </body> tag -->
<!-- Load in specific order to prevent conflicts -->
<script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
<script src="_content/ApprenticeFoundryBlazorThreeJS/js/blazor-three-js.js"></script>
<script src="_content/ApprenticeFoundryBlazor/js/app-lib.js"></script>
<script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
```

## Basic Component Usage

### Canvas 2D Component
```razor
@page "/canvas-demo"
@using FoundryBlazor.Shared

<h3>2D Canvas Example</h3>

<Canvas2DComponent SceneName="Demo"
                   CanvasWidth="800" 
                   CanvasHeight="600"
                   WithAnimations="true" />
```

### Canvas 3D Component
```razor
@page "/canvas3d-demo"
@using FoundryBlazor.Shared

<h3>3D Canvas Example</h3>

<Canvas3DComponent SceneName="Demo3D"
                   WithAnimations="true" />
```

### Shape Tree View
```razor
@page "/tree-demo"
@using FoundryBlazor.Shared

<h3>Shape Tree View</h3>

<div style="height: 400px;">
    <ShapeTreeView />
</div>
```

### Radzen Tree View (Alternative)
```razor
@page "/radzen-tree-demo"
@using FoundryBlazor.Shared

<h3>Radzen Tree View</h3>

<div style="height: 400px;">
    <RadzenShapeTreeView />
</div>
```

## Advanced Integration

### Working with Workspace and Drawing
```razor
@page "/advanced-demo"
@using FoundryBlazor.Shared
@using FoundryBlazor.Shape
@inject IWorkspace Workspace

<div class="container-fluid">
    <div class="row">
        <div class="col-3">
            <ShapeTreeView />
        </div>
        <div class="col-9">
            <Canvas2DComponent SceneName="MainCanvas"
                               CanvasWidth="1200" 
                               CanvasHeight="800" />
        </div>
    </div>
</div>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Initialize with custom shapes
        var drawing = Workspace.GetDrawing();
        
        // Add some shapes programmatically
        var rect = new FoShape2D("Rectangle1", 100, 50, "Blue");
        rect.PinX = 100;
        rect.PinY = 100;
        drawing?.AddShape(rect);
        
        var circle = new FoShape2D("Circle1", 80, 80, "Red");
        circle.PinX = 250;
        circle.PinY = 150;
        drawing?.AddShape(circle);
        
        await base.OnInitializedAsync();
    }
}
```

### Event Handling
```razor
@using BlazorComponentBus
@using FoundryBlazor.PubSub
@inject ComponentBus PubSub

<Canvas2DComponent SceneName="EventDemo" />

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
        // Handle shape selection
        Console.WriteLine($"Shape selected: {eventArgs.Shape?.Name}");
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

## Troubleshooting

### Common Issues

1. **Static Assets Not Loading**
   ```
   Failed to load resource: the server responded with a status of 404 (Not Found)
   _content/ApprenticeFoundryBlazor/js/app-lib.js
   ```
   
   **Solution**: Ensure `app.UseStaticFiles()` is called before `app.UseRouting()` in Program.cs

2. **Canvas Not Rendering**
   - Verify all required JavaScript references are included
   - Check browser console for JavaScript errors
   - Ensure Canvas2DContext initialization completes

3. **Service Dependency Errors**
   ```
   Unable to resolve service for type 'FoundryBlazor.IWorkspace'
   ```
   
   **Solution**: Ensure you've called `services.AddFoundryBlazor()` in Program.cs

4. **Tree View Not Displaying Data**
   - Verify Radzen.Blazor CSS is properly loaded
   - Check that the workspace has been initialized with data
   - Ensure component is properly sized with CSS height

### Performance Considerations

1. **Canvas Size**: Large canvas sizes (>2000px) may impact performance
2. **Animation**: Disable animations for large datasets using `WithAnimations="false"`
3. **Memory**: Components implement IAsyncDisposable - ensure proper disposal in consuming apps

### Browser Compatibility

- **Modern Browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Canvas2D**: Supported by all modern browsers
- **WebGL**: Required for 3D components, supported by 97%+ of browsers

## Example Projects

See the [FoundryBlazor repository](https://github.com/SteveStrong/FoundryBlazor) for complete example projects demonstrating:

- Blazor Server integration
- Blazor WebAssembly integration  
- Advanced shape manipulation
- Custom component creation
- Performance optimization patterns