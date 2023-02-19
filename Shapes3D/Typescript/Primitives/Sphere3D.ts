import { MeshBuilder, Scene } from 'babylonjs';
import { HighResBox } from '../models';
import { Base3D } from './Base3D';

export class Sphere3D extends Base3D {
    box: HighResBox;

    createMesh(scene: Scene) {
        if (this.mesh) return this.mesh;

        this.mesh = MeshBuilder.CreateSphere('Sphere3D', {
            diameterX: this.box.width,
            diameterY: this.box.height,
            diameterZ: this.box.depth
            }, scene);
        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox()) {
        super();
        this.box = box;
    }
}
