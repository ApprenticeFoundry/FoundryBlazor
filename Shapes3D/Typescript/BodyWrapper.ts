import { BoundingInfo, Mesh, Scene, TransformNode, Vector3, AxesViewer } from 'babylonjs';
import { takeUntil } from 'rxjs';
import { HighResBox } from '../models/HighResBox';
import { EditableTreeNode } from '../models/UDTO_3D';
import { UDTO_Body } from '../models/UDTO_Body';
import { UDTO_Label } from '../models/UDTO_Label';
import { Cylinder3D, Box3D, Sphere3D, Boundary3D, GlbFile3D } from '../primitives3D';
import { Boid3D } from '../primitives3D/Boid3D';
import { Stub3D } from '../primitives3D/Stub3D';
import { PubSub, Topic, TopicById } from '../shared/K2PubSub';

import { BodyModelBase } from './BodyModelBase';

export class BodyWrapper extends BodyModelBase {
    protected _shapeMesh: Mesh;

    private get factory() {
        const key = this.data?.type || 'Stub';
        var found = this.bodyMeshDict[key];
        if (!found) {
            found = this.makeStub3D.bind(this);
        }
        return found;
    }

    private bodyMeshDict: Record<string, Function> = {
        Glb: this.makeGlbFile3D.bind(this),
        Stub: this.makeStub3D.bind(this), //this should be a simple transform node
        Box: this.makeBox3D.bind(this),
        Sphere: this.makeSphere3D.bind(this),
        Boid: this.makeBoid3D.bind(this),
        Boundry: this.makeBoundry3D.bind(this),
        Cylinder: this.makeCylinder3D.bind(this)
    };

    primitives(): string[] {
        return Object.keys(this.bodyMeshDict);
    }

    private makeGlbFile3D(scene: Scene) {
        return new GlbFile3D(this.data).createMesh(scene);
    }

    private makeStub3D(scene: Scene) {
        return new Stub3D().createMesh(scene);
    }

    private makeBox3D(scene: Scene) {
        return new Box3D(this.box).createMesh(scene);
    }
    private makeSphere3D(scene: Scene) {
        return new Sphere3D(this.box).createMesh(scene);
    }

    private makeCylinder3D(scene: Scene) {
        return new Cylinder3D(this.box).createMesh(scene);
    }

    private makeBoid3D(scene: Scene) {
        return new Boid3D(this.box).createMesh(scene);
    }

    private makeBoundry3D(scene: Scene) {
        return new Boundary3D(this.box).createMesh(scene);
    }

    private updateBody(body: UDTO_Body) {
        const { xAng, yAng, zAng } = body.position;
        this.RotateTo(xAng, yAng, zAng);
    }

    private onBodyChange() {
        const id = this.data.uniqueGuid;
        PubSub.pubSubById$({ id, topic: Topic.WORLD_3D_BODY_UPDATE })
            .pipe(takeUntil(this.unsubscribe$))
            .subscribe(([topic, data]: [TopicById, UDTO_Body]) => {
                this.updateBody(data);
            });
    }

    public create3DGeometry(scene: Scene): TransformNode {
        const node = super.create3DGeometry(scene);
        node.position = this.localPosition();
        node.rotation = this.localRotation();

        this._shapeMesh = this.factory(scene);
        this._shapeMesh.parent = node;

        this.captureBoundingBox();

        // const axis = new AxesViewer(scene,1);
        // axis.xAxis.parent = node;
        // axis.yAxis.parent = node;
        // axis.zAxis.parent = node;

        return node;
    }
    public captureBoundingBox() {
        const mesh = this._shapeMesh;
        if (!mesh) return;

        const box = mesh.getHierarchyBoundingVectors();

        this.data.boundingBox = new HighResBox({
            width: box.max.x - box.min.x,
            height: box.max.y - box.min.y,
            depth: box.max.z - box.min.z
        });
    }

    //https://playground.babylonjs.com/#4F33I3#6
    public findBoundingBox(parent: Mesh) {
        let childMeshes = parent.getChildMeshes();
        let box = childMeshes[0].getBoundingInfo().boundingBox;

        let min: Vector3 = box.minimumWorld;
        let max: Vector3 = box.maximumWorld;

        for (let i = 0; i < childMeshes.length; i++) {
            let box = childMeshes[i].getBoundingInfo().boundingBox;
            let meshMin = box.minimumWorld;
            let meshMax = box.maximumWorld;

            min = Minimize(min, meshMin);
            max = Maximize(max, meshMax);
        }

        parent.setBoundingInfo(new BoundingInfo(min, max));

        parent.showBoundingBox = true;
    }

    public setLocation() {
        return (treeText: string) => {
            const pos = this.data.position;
            const newText = this.treeNodeSetterLoc(treeText, pos);
            this.MoveTo(pos.xLoc, pos.yLoc, pos.zLoc);
            return newText;
        };
    }

    public setAngle() {
        return (treeText: string) => {
            const pos = this.data.position;
            const newText = this.treeNodeSetterAng(treeText, pos);
            this.RotateTo(pos.xAng, pos.yAng, pos.zAng);
            return newText;
        };
    }

    public setPin() {
        return (treeText: string) => {
            const bBox = this.data.boundingBox;
            const newText = this.treeNodeSetterPin(treeText, bBox);
            this.PinTo(bBox.pinX, bBox.pinY, bBox.pinZ);
            return newText;
        };
    }

    public setSize() {
        return (treeText: string) => {
            const bBox = this.data.boundingBox;
            const newText = this.treeNodeSetterSize(treeText, bBox);
            this.PinTo(bBox.pinX, bBox.pinY, bBox.pinZ);
            return newText;
        };
    }

    public TreeChildren(): EditableTreeNode[] {
        const data = this.data;
        const position = data.position;
        const boundingBox = data.boundingBox;

        const pos = position.toPos();
        const rotation = position.toRotationDegrees();
        const pin = boundingBox.toPin();
        const size = boundingBox.toSize();

        let symbol = 'NO SYMBOL';
        if (data.symbol) {
            symbol = data.symbol.split('/').reverse()[0];
        }

        const typeInfo = new EditableTreeNode({ getter: () => data.type, setter: this.treeNodeSetterReadOnly(), self: this });
        const symbolInfo = new EditableTreeNode({ getter: () => symbol, setter: this.treeNodeSetterReadOnly(), self: this });
        const posInfo = new EditableTreeNode({ getter: () => pos, setter: this.setLocation(), self: this });
        const rotationInfo = new EditableTreeNode({ getter: () => rotation, setter: this.setAngle(), self: this });
        const pinInfo = new EditableTreeNode({ getter: () => pin, setter: this.setPin(), self: this });
        const sizeInfo = new EditableTreeNode({ getter: () => size, setter: this.setSize(), self: this });

        const result = [typeInfo, symbolInfo, posInfo, rotationInfo, pinInfo];
        if (data.type != 'Glb' || true) {
            result.push(sizeInfo);
        }
        return result;
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
        this.onBodyChange();
    }
}
function Minimize(min: Vector3, meshMin: Vector3): Vector3 {
    throw new Error('Function not implemented.');
}

function Maximize(max: Vector3, meshMax: Vector3): Vector3 {
    throw new Error('Function not implemented.');
}
