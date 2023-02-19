import { Color4, Vector4 } from 'babylonjs';

// Stinks that we need to repeat options from MeshBuilder.CreateCylinder
export type CylinderOptions = {
    height?: number;
    diameterTop?: number;
    diameterBottom?: number;
    diameter?: number;
    tessellation?: number;
    subdivisions?: number;
    arc?: number;
    faceColors?: Color4[];
    faceUV?: Vector4[];
    updatable?: boolean;
    hasRings?: boolean;
    enclose?: boolean;
    cap?: number;
    sideOrientation?: number;
    frontUVs?: Vector4;
    backUVs?: Vector4;
};
