import { Mesh, MeshBuilder, Scene, Vector3 } from 'babylonjs';
import { HighResBox } from '../models';


import { Base3D } from './Base3D';

export class Stub3D extends Base3D {
    box: HighResBox;

    createMesh(scene: Scene) {
        if (this.mesh) return this.mesh;

        this.mesh = MeshBuilder.CreateSphere('Stub3D', {
            diameterX: this.box.width,
            diameterY: this.box.height,
            diameterZ: this.box.depth
            }, scene);
        return this.mesh;
    }

    constructor() {
        super();
        this.box = new HighResBox({ width: 0.001, height: 0.001, depth: 0.001 });
    }
}
