import { Color3, Mesh, MeshBuilder, Scene, StandardMaterial, TransformNode, Vector3 } from 'babylonjs';

import { HighResBox, HighResPosition, UDTO_Position } from '../models';
import { UDTO_Platform } from '../models';
import { IModelBase3D, ModelBase3D } from './ModelBase3D';
import { GridMaterial } from 'babylonjs-materials';

export class PlatformModelBase extends ModelBase3D<UDTO_Platform> {
    public showFloor: boolean;
    public showVolume: boolean;
    public pos3D: HighResPosition;

    protected get box(): HighResBox {
        if (!this.data.boundingBox) {
            this.data.boundingBox = new HighResBox({ width: 0.001, height: 0.001, depth: 0.001 });
        }
        return this.data.boundingBox;
    }

    protected get pos(): HighResPosition {
        if (!this.pos3D) {
            this.pos3D = new HighResPosition();
        }
        return this.pos3D;
    }

    protected get position(): UDTO_Position {
        return this.data.position;
    }

    protected GroundMat(scene: Scene) {
        //https://doc.babylonjs.com/toolsAndResources/assetLibraries/materialsLibrary/gridMat
        const mat = new GridMaterial('ground', scene);
        // mat.majorUnitFrequency = 10;
        // mat.majorUnitFrequency = 1
        //mat.minorUnitVisibility = .5;

        mat.gridRatio = 1;
        mat.backFaceCulling = false;
        mat.mainColor = new Color3(1, 1, 1);
        mat.lineColor = new Color3(0, 0, 0);
        mat.opacity = 0.5;
        return mat;
    }

    public createVolume(scene: Scene): Mesh {
        const mat = new StandardMaterial('platform', scene);
        mat.alpha = 0.2;
        mat.diffuseColor = Color3.Green();
        const volume = MeshBuilder.CreateBox('platform', this.box, scene);
        volume.material = mat;
        return volume;
    }

    public createSomethingToSee(scene: Scene): Mesh {
        const mat = new StandardMaterial('platform', scene);
        mat.alpha = 0.2;
        mat.diffuseColor = Color3.Red();
        const volume = MeshBuilder.CreateBox('platform', { width: 1, height: 20, depth: 5 }, scene);
        volume.material = mat;
        return volume;
    }

    public create3DGeometry(scene: Scene): TransformNode {
        const node = super.create3DGeometry(scene);

        if (this.showFloor) {
            const ground = MeshBuilder.CreateGround('ground', { width: this.box.width, height: this.box.depth }, scene);
            ground.material = this.GroundMat(scene);
            ground.parent = node;
        }

        if (this.showVolume) {
            const volume = this.createVolume(scene);
            volume.parent = node;
        }
        return node;
    }

    public runStep(frameID: number): IModelBase3D {
        super.runStep(frameID);
        return this;
    }

    public applyAnimation(): IModelBase3D {
        return this;
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
    }
}
