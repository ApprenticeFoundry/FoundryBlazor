import { ModelBase } from '../shared/model-base';
import { Pin3D } from '../primitives3D';
import { AxesViewer, Axis, Mesh, Scene, Space, TransformNode, Vector3 } from 'babylonjs';

import { HighResBox, HighResPosition, UDTO_3D, EditableTreeNode } from '../models';
import { Tools } from '../shared/foTools';
import { Subject } from 'rxjs';

export interface IModelBase3D {
    runStep(frameID: number): IModelBase3D;
    get uniqueGuid(): string;
    localPosition(): Vector3;
    localRotation(): Vector3;
    getContainer3D(scene: Scene): TransformNode;
    markDirty(): void;
    MoveBy(x: number, y: number, z: number): IModelBase3D;
    RotateBy(x: number, y: number, z: number): IModelBase3D;
    applyAnimation(): IModelBase3D;
    render3D(scene: Scene): TransformNode;
}

export class ModelBase3D<T extends UDTO_3D> extends ModelBase implements IModelBase3D {
    public showPin: boolean = false;

    public scale: number;
    public data: T;
    protected _container3D: TransformNode;
    protected _pin: Pin3D;
    protected readonly unsubscribe$ = new Subject<void>();

    public preRenderFunc: (frameID: number) => void;

    public runStep(frameID: number): IModelBase3D {
        this.data?.runStep(frameID);
        this.preRenderFunc && this.preRenderFunc(frameID);
        return this;
    }

    public applyAnimation(): IModelBase3D {
        this.data.update3D(this.mesh());
        return this;
    }

    public setPreRender(func: (frameID: number) => void) {
        this.preRenderFunc = func;
    }

    public get uniqueGuid(): string {
        return this.data.uniqueGuid;
    }

    protected get box(): HighResBox {
        let result = this.data.hasOwnProperty('boundingBox');
        if (!result) {
            Object.defineProperty(this.data, 'boundingBox', {
                enumerable: true,
                configurable: true,
                value: new HighResBox()
            });
        }
        return this.data.getPropertyValue(this.data, 'boundingBox');
    }

    protected get pos(): HighResPosition {
        let result = this.data.hasOwnProperty('position');
        if (!result) {
            Object.defineProperty(this.data, 'position', {
                enumerable: true,
                configurable: true,
                value: new HighResPosition()
            });
        }
        return this.data.getPropertyValue(this.data, 'position');
    }

    public localPosition(): Vector3 {
        const pos = this.pos;
        const box = this.box;
        return new Vector3(pos.xLoc - box.pinX, pos.yLoc - box.pinY, pos.zLoc - box.pinZ);
    }
    public localRotation(): Vector3 {
        const pos = this.pos;
        return new Vector3(pos.xAng, pos.yAng, pos.zAng);
    }

    public create3DPin(scene: Scene): Mesh {
        this._pin = new Pin3D();
        const mesh = this._pin.createMesh(scene);
        mesh.position = new Vector3(this.box.pinX, this.box.pinY, this.box.pinZ);
        mesh.parent = this._container3D;

        return mesh;
    }

    public mesh(): TransformNode {
        return this._container3D;
    }

    public create3DGeometry(scene: Scene): TransformNode {
        this._container3D = new TransformNode('container');

        if (this.showPin) {
            var mesh = this.create3DPin(scene);

            const axis = new AxesViewer(scene, 1);
            axis.xAxis.parent = mesh;
            axis.yAxis.parent = mesh;
            axis.zAxis.parent = mesh;
        }
        return this._container3D;
    }

    public getContainer3D(scene: Scene): TransformNode {
        if (!this._container3D) {
            this.create3DGeometry(scene);
        }
        return this._container3D;
    }

    public markDirty(): IModelBase3D {
        if (this._container3D) {
            this._container3D.parent.dispose();
            this._container3D.dispose();
        }
        this._container3D = null;
        return this;
    }

    public MoveTo(x: number, y: number, z: number): IModelBase3D {
        const node = this.mesh();
        if (node) {
            node.position.x = x;
            node.position.y = y;
            node.position.z = z;
        }
        return this;
    }

    public MoveBy(x: number, y: number, z: number): IModelBase3D {
        const node = this.mesh();
        if (node) {
            node.position.x += x;
            node.position.y += y;
            node.position.z += z;
        }
        return this;
    }

    public RotateBy(x: number, y: number, z: number): IModelBase3D {
        const node = this.mesh();
        if (node) {
            node.rotate(Axis.X, x, Space.LOCAL);
            node.rotate(Axis.Y, y, Space.LOCAL);
            node.rotate(Axis.Z, z, Space.LOCAL);
        }
        return this;
    }

    public RotateTo(x: number, y: number, z: number): IModelBase3D {
        const node = this.mesh();
        if (node) {
            node.rotation.set(x, y, z);
        }
        return this;
    }

    public PinTo(x: number, y: number, z: number): IModelBase3D {
        const node = this.mesh();
        if (node) {
            this.box.pinX = x;
            this.box.pinY = y;
            this.box.pinZ = z;
            node.position = this.localPosition();
        }
        return this;
    }

    public SizeTo(x: number, y: number, z: number): IModelBase3D {
        this.box.width = x;
        this.box.height = y;
        this.box.depth = z;

        if (Boolean(this._container3D)) {
            this._container3D.dispose();
            this._container3D = null;
        }
        return this;
    }

    public render3D(scene: Scene): TransformNode {
        return this.getContainer3D(scene);
    }

    protected parseTreeTextValues(treeText: string) {
        const valueList = treeText.split('=')[1].split(',');
        const values = valueList.map((item) => {
            return parseFloat(item);
        });
        return values;
    }

    protected treeNodeSetterReadOnly() {
        return (treeText: string) => {};
    }

    protected treeNodeSetterLoc(treeText: string, position: HighResPosition) {
        const dims = this.parseTreeTextValues(treeText);
        position.setPos(dims);
        return position.toPos();
    }

    protected treeNodeSetterAng(treeText: string, position: HighResPosition) {
        const dims = this.parseTreeTextValues(treeText);
        const useDims = dims.map((item) => {
            const val = Number(Number(Tools.degreesToRadians(item)).toFixed(2));
            return val;
        });
        position.setRotation(useDims);
        return position.toRotationDegrees();
    }

    protected treeNodeSetterPin(treeText: string, boundingBox: HighResBox) {
        const dims = this.parseTreeTextValues(treeText);
        boundingBox.setPin(dims);
        return boundingBox.toPin();
    }

    protected treeNodeSetterSize(treeText: string, boundingBox: HighResBox) {
        const dims = this.parseTreeTextValues(treeText);
        boundingBox.setSize(dims);
        return boundingBox.toSize();
    }

    public destroy() {
        this._destroy();
    }

    private _destroy() {
        this.unsubscribe$ && this.unsubscribe$.next();
        this.unsubscribe$ && this.unsubscribe$.complete();
    }

    public TreeChildren(): EditableTreeNode[] {
        const data = this.data;

        const typeInfo = new EditableTreeNode({ getter: () => 'Base Tree: Do Not Use', setter: this.treeNodeSetterReadOnly(), self: this });

        return [typeInfo];
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
    }
}
