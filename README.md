
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
dotnet add package FoundryBlazor
```

## Quick Start

```csharp
// Add to your Program.cs
builder.Services.AddFoundryBlazor();

// Use in your Blazor component
<FoundryDiagram @ref="diagram">
    // Your diagram content
</FoundryDiagram>
```

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

## Requirements

- .NET 8 or higher
- Blazor Server or WebAssembly

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## Support

- **Demo**: https://apprenticefoundry.github.io/
- **Issues**: Submit issues on GitHub for bug reports and feature requests
- **Documentation**: See the Matrix3D compatibility documentation for technical details