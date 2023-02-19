import { Color3, MeshBuilder, Scene, StandardMaterial, TransformNode, Axis, Space, Vector3, AxesViewer } from 'babylonjs';
import { EditableTreeNode, UDTO_3D } from '../models/UDTO_3D';
import { UDTO_Label } from '../models/UDTO_Label';
import { PubSub, Topic } from '../shared/K2PubSub';
import { BodyWrapper } from './BodyWrapper';
import { LabelWrapper } from './LabelWrapper';
import { IModelBase3D, ModelBase3D } from './ModelBase3D';

import { PlatformModelBase } from './PlatformModelBase';

export class PlatformWrapper extends PlatformModelBase {
    public showAxis: boolean;

    _bodyLookup: { [key: string]: ModelBase3D<UDTO_3D> } = {};
    _labelLookup: { [key: string]: ModelBase3D<UDTO_3D> } = {};
    _children: ModelBase3D<UDTO_3D>[] = null;

    private addBodyWrapper(wrap: ModelBase3D<UDTO_3D>): ModelBase3D<UDTO_3D> {
        const id = wrap.uniqueGuid;
        if (!this._bodyLookup[id]) {
            this._children = null;
            this._bodyLookup[id] = wrap;
        }
        return wrap;
    }

    private addLabelWrapper(wrap: ModelBase3D<UDTO_3D>): ModelBase3D<UDTO_3D> {
        const id = wrap.uniqueGuid;
        if (!this._labelLookup[id]) {
            this._children = null;
            this._labelLookup[id] = wrap;
        }
        return wrap;
    }

    public Children3D(): ModelBase3D<UDTO_3D>[] {
        if (!Boolean(this._children)) {
            this._children = [...Object.values(this._bodyLookup), ...Object.values(this._labelLookup)];
        }
        return this._children;
    }

    public wrapMembers(): ModelBase3D<UDTO_3D>[] {
        const platform = this.data;

        platform.Bodies().forEach((body: UDTO_3D) => this.addBodyWrapper(new BodyWrapper({ data: body })));

        platform.Labels().forEach((label: UDTO_3D) => this.addLabelWrapper(new LabelWrapper({ data: label })));

        return this.Children3D();
    }

    public applyAnimation(): IModelBase3D {
        this.Children3D().forEach((item) => item.applyAnimation());
        return this;
    }

    public create3DGeometry(scene: Scene): TransformNode {
        const node = super.create3DGeometry(scene);

        const d = 0.05;
        const dd = 30 * d;
        const extra = 10;
        const ang = Math.PI / 2;

        const axis = new AxesViewer(scene, 1);

        if (!this.showAxis) return node;

        const xAxisMaterial = new StandardMaterial('xaxis', scene);
        xAxisMaterial.diffuseColor = new Color3(1, 0, 0);
        const axisX = MeshBuilder.CreateCylinder('axisX', { diameter: d, height: this.box.width }, scene);
        axisX.rotate(Axis.Z, ang, Space.LOCAL);
        axisX.material = xAxisMaterial;
        axisX.parent = node;

        const xEnd = MeshBuilder.CreateSphere('endX', { diameter: dd }, scene);
        xEnd.material = xAxisMaterial;
        xEnd.position = new Vector3(extra + this.box.width / 2.0, 0, 0);
        xEnd.parent = node;

        const yAxisMaterial = new StandardMaterial('yaxis', scene);
        yAxisMaterial.diffuseColor = new Color3(0, 1, 0);
        const axisY = MeshBuilder.CreateCylinder('axisY', { diameter: d, height: this.box.height }, scene);
        axisY.rotate(Axis.Y, 0, Space.LOCAL);
        axisY.material = yAxisMaterial;
        axisY.parent = node;

        const yEnd = MeshBuilder.CreateSphere('endY', { diameter: dd }, scene);
        yEnd.material = yAxisMaterial;
        yEnd.position = new Vector3(0, extra + this.box.height / 2.0, 0);
        yEnd.parent = node;

        const zAxisMaterial = new StandardMaterial('zaxis', scene);
        zAxisMaterial.diffuseColor = new Color3(0, 0, 1);
        const axisZ = MeshBuilder.CreateCylinder('axisZ', { diameter: d, height: this.box.depth }, scene);
        axisZ.rotate(Axis.X, ang, Space.LOCAL);
        axisZ.material = zAxisMaterial;
        axisZ.parent = node;

        const zEnd = MeshBuilder.CreateSphere('endZ', { diameter: dd }, scene);
        zEnd.material = zAxisMaterial;
        zEnd.position = new Vector3(0, 0, extra + this.box.depth / 2.0);
        zEnd.parent = node;
        return node;
    }

    public updateLabel(label: UDTO_Label) {
        const id = label.uniqueGuid;
        const found = this._labelLookup[id];
        if (Boolean(found)) {
            found.data = label;
        }
    }

    public testLabelTimer() {
        setInterval(() => {
            const label = Object.values(this._labelLookup)[0];
            const testLabel = new UDTO_Label(label.data);
            testLabel.text = `${new Date().getTime()}`;
            const id = testLabel.uniqueGuid;
            PubSub.broadcastById({ id, topic: Topic.WORLD_3D_LABEL_UPDATE }, testLabel);
        }, 1000);
    }

    public runStep(frameID: number): IModelBase3D {
        super.runStep(frameID);
        this.Children3D().forEach((item) => {
            item.runStep(frameID);
        });

        return this;
    }

    constructor(properties?: any) {
        super(properties);
        this.override(properties);
    }
}
