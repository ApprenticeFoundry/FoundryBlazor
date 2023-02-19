import { Color3, Mesh, MeshBuilder, Scene, StandardMaterial } from 'babylonjs';
import { Base3D } from './Base3D';
import { Tools } from '../shared/foTools';
import { HighResBox } from '../models';

export class Pin3D extends Base3D {
    box: HighResBox;
    visible: boolean = true;

    private get material() {
        const mat = new StandardMaterial('mat');
        mat.diffuseColor = Color3.Green();
        mat.diffuseColor = new Color3(...Tools.rgbDecimal(255, 191, 0));
        return mat;
    }

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;
        const diameter = (this.box.width + this.box.height + this.box.depth) /3.0;
        this.mesh = MeshBuilder.CreateSphere('Pin3D', { diameter: diameter/10.0 }, scene);
        this.mesh.material = this.material;
        this.mesh.visibility = this.visible ? 0.5 : 0;
        return this.mesh;
    }


    constructor(box: HighResBox = new HighResBox()) {
        super();
        this.box = box;
    }
}
