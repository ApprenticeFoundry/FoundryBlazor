import { MeshBuilder, Scene } from 'babylonjs';
import { Base3D } from './Base3D';
import { CylinderOptions } from '.';
import { HighResBox } from '../models';

export class Cylinder3D extends Base3D {
    box: HighResBox;
    options: CylinderOptions;

    createMesh(scene: Scene) {
        if (this.mesh) return this.mesh;
        const diameter = (this.box.width + this.box.depth) / 2.0;
        this.mesh = MeshBuilder.CreateCylinder('Cylinder3D', { height: this.box.height, diameter: diameter, ...this.options }, scene);
        return this.mesh;
    }

    constructor(box: HighResBox = new HighResBox(), options: CylinderOptions = {}) {
        super();
        this.box = box;
        this.options = options;
    }
}
