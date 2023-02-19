import { Mesh, MeshBuilder, Scene, StandardMaterial, Vector3 } from 'babylonjs';
import { Base3D } from './Base3D';
import { CylinderOptions } from '.';
import { HighResBox } from '../models';

export class Boid3D extends Base3D {
    box: HighResBox;

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        const { width, height, depth }= this.box

        const mat = new StandardMaterial('Boid3D', scene);
        // mat.diffuseColor = color;

        const nose = MeshBuilder.CreateSphere('nose', { diameter: height, segments: 16 }, scene);
        nose.material = mat;
        nose.position = new Vector3(width/2, 0, 0);

        const fuselage = MeshBuilder.CreateBox('fuselage', { width: width, height: height/2, depth: height/2 }, scene);
        fuselage.position = new Vector3(0, 0, 0);
        fuselage.material = mat;

        const wing = MeshBuilder.CreateBox('wing', { width: 1.5*height, height: height/4, depth:depth }, scene);
        wing.position = new Vector3(height/4,0,0);
        wing.material = mat;

        this.mesh = Mesh.MergeMeshes([nose, wing, fuselage], true, true, undefined, false, true);

        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox()) {
        super();
        this.box = box;
    }
}
