import { Color3, Mesh, MeshBuilder, Scene, StandardMaterial } from 'babylonjs';
import { HighResBox } from '../models';

import { Base3D } from './Base3D';

export class Boundary3D extends Base3D {
    box: HighResBox;

    private get material() {
        const mat = new StandardMaterial('Boundary3D');
        mat.diffuseColor = Color3.Red();
        mat.alpha = 0.3;
        mat.wireframe = true;
        return mat;
    }

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        this.mesh = MeshBuilder.CreateBox('Boundary3D', { ...this.box }, scene);
        this.mesh.material = this.material;
        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox({ width: 1, height: 1, depth: 1 })) {
        super();
        this.box = box;
    }
}
