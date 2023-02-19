import { Mesh, MeshBuilder, Scene, Vector3 } from 'babylonjs';
import { HighResBox } from '../models';


import { Base3D } from './Base3D';

export class Box3D extends Base3D {
    box: HighResBox;

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        this.mesh =  MeshBuilder.CreateBox('Box3D', { ...this.box }, scene);
        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox()) {
        super();
        this.box = box;
    }
}
