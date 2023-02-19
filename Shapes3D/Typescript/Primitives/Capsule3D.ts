import { Mesh, MeshBuilder, Scene } from 'babylonjs';
import { HighResBox } from '../models';


import { Base3D } from './Base3D';

export class Capsule3D extends Base3D {
    box: HighResBox;

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        this.mesh =  MeshBuilder.CreateCapsule('Capsule3D', { ...this.box }, scene);
        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox()) {
        super();
        this.box = box;
    }
}
