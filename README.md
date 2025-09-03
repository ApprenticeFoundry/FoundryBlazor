
# FoundryBlazor

Working to create a complete C# / Blazor diagraming experience for developers.

The Blazor demo built for NDC Oslo 2023 is now available as a NuGet package. The library includes .NET 8 and Blazor features. There are new 2D shapes, layout algorithms with glued connections, scaled multi-page diagrams and better 2D/3D integration. The features of Visio, Three.js and CesiumJS are now available in Blazor, both client and server.

https://apprenticefoundry.github.io/

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