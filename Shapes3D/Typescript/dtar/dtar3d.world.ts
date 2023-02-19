import { ArcRotateCamera, Color3, Color4, Engine, HemisphericLight, InstancedMesh, Mesh, MeshBuilder, Scene, StandardMaterial, Vector3, Vector4 } from 'babylonjs';
import { max } from 'lodash';

import { HighResBox } from '../models';

export class DTAR3DWorld {
    private _engine: Engine;
    private _scene: Scene;
    private _camera: ArcRotateCamera;
    private _canvas: HTMLCanvasElement;
    private _box: HighResBox;
    private _size: number;

    private frameID: number = 0;

    constructor(canvas: HTMLCanvasElement, box: HighResBox) {
        this._canvas = canvas;
        this._box = box;
        this._size = max([box.width, box.height, box.depth]); //.sort((a, b) => a - b)[0];
        this._engine = new Engine(this._canvas, true);

        //https://forum.babylonjs.com/t/left-and-right-handed-shenanagins/17049/4
        // This creates a basic Babylon Scene object (non-mesh)
        this._scene = new Scene(this._engine);
        this._scene.useRightHandedSystem = true; //is this enough, yes it matches CAD

        this._scene.clearColor = new Color4(0, 0, 0, 0);
        const light = new HemisphericLight('light', new Vector3(0, 1, 0), this._scene);
        light.intensity = 0.75;

        // vis the volume helps debug things
        // this.createVolume(this._scene, this._box);

        this._camera = this.createCamera(this._scene, this._size, 2.0 * this._size);
        this._camera.attachControl(this._canvas, true);
        this.inspectorOn();
    }

    public createCamera(scene: Scene, size: number, radius: number): ArcRotateCamera {
        const camera = new ArcRotateCamera('camera', -Math.PI / 2, Math.PI / 2.5, size, Vector3.Zero(), scene);
        camera.layerMask = 0x0fffffff;
        //Increasing the number value makes the camera zoom slower, and lowering it speeds up the zoom.
        camera.wheelPrecision = 10;
        camera.speed = 0.25;
        camera.radius = radius / 3;
        camera.panningSensibility = 8;
        return camera;
    }

    public createVolume(scene: Scene, box: HighResBox): Mesh {
        const mat = new StandardMaterial('worldVolume', scene);
        mat.alpha = 0.2;
        mat.diffuseColor = Color3.Blue();
        const volume = MeshBuilder.CreateBox('worldVolume', box, scene);
        volume.material = mat;
        return volume;
    }

    public scene(): Scene {
        return this._scene;
    }
    public box(): HighResBox {
        return this._box;
    }

    public resize(box: HighResBox): DTAR3DWorld {
        this._box = box;
        this._size = max([box.width, box.height, box.depth]);
        this._camera = this.createCamera(this._scene, this._size, 2.0 * this._size);
        this._camera.attachControl(this._canvas, true);
        return this;
    }

    public startRenderLoop(func: (frameID: number) => void) {
        this._engine.runRenderLoop(() => {
            func(this.frameID++);
            this._scene.render();
        });
    }

    public inspectorOn() {
        this._scene.debugLayer.show({
            embedMode: true
        });
    }

    public inspectorOff() {
        this._scene.debugLayer.hide();
    }

    public stopRenderLoop() {
        this._engine.stopRenderLoop();
    }
}
