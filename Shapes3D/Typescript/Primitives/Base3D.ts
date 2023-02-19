import { CreateBox, Mesh, MeshBuilder, Scene } from 'babylonjs';
import { retry } from 'rxjs';
import { ModelBase } from '../shared/model-base';

export class Base3D extends ModelBase {
    mesh: Mesh;
    //parent: Base3D;

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        this.mesh = MeshBuilder.CreateBox('Base3D', { size: 0 }, scene);
        return this.mesh;
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
    }
}
