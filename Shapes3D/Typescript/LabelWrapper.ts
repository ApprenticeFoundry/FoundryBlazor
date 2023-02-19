import { Color3, DynamicTexture, Mesh, MeshBuilder, Scene, StandardMaterial, TransformNode, Vector3 } from 'babylonjs';
import { EditableTreeNode } from '../models/UDTO_3D';
import { Label3D } from '../primitives3D';
import { LabelModelBase } from './LabelModelBase';
import { PubSub, Topic, TopicById } from '../shared/K2PubSub';
import { takeUntil } from 'rxjs';
import { UDTO_Label } from '../models/UDTO_Label';

export class LabelWrapper extends LabelModelBase {
    private labelEntity: Label3D;
    private scene: Scene;

    private get material() {
        const mat = new StandardMaterial('labelMat');
        mat.diffuseColor = Color3.Green();
        return mat;
    }

    private repositionLabel(label: Mesh) {
        label.parent = this._container3D;
        if (Boolean(this.data.position)) {
            const { xLoc, yLoc, zLoc } = this.data.position;
            this.MoveTo(xLoc, yLoc, zLoc);
        }
    }

    private updateLabel(label: UDTO_Label) {
        const labelMesh = this.labelEntity.updateText(label.text, this.scene);
        this.repositionLabel(labelMesh);
    }

    private onLabelChange() {
        const id = this.data.uniqueGuid;
        PubSub.pubSubById$({ id, topic: Topic.WORLD_3D_LABEL_UPDATE })
            .pipe(takeUntil(this.unsubscribe$))
            .subscribe(([topic, data]: [TopicById, UDTO_Label]) => {
                this.updateLabel(data);
            });
    }

    public create3DGeometry(scene: Scene): TransformNode {
        this.scene = scene;
        const node = super.create3DGeometry(scene);
        node.position = new Vector3(this.pos.xLoc, this.pos.yLoc, this.pos.zLoc);

        this.labelEntity = new Label3D(this.text, 16);
        const label = this.labelEntity.createMesh(scene);
        label.parent = node;

        return node;
    }

    public setLocation() {
        return (treeText: string) => {
            const pos = this.data.position;
            const newText = this.treeNodeSetterLoc(treeText, pos);
            this.MoveTo(pos.xLoc, pos.yLoc, pos.zLoc);
            return newText;
        };
    }

    public TreeChildren(): EditableTreeNode[] {
        const data = this.data;
        const position = data.position;
        const pos = position.toPos();

        const typeInfo = new EditableTreeNode({ getter: () => data.type, setter: this.treeNodeSetterReadOnly(), self: this });
        const textInfo = new EditableTreeNode({ getter: () => data.text, setter: this.treeNodeSetterReadOnly(), self: this });
        const posInfo = new EditableTreeNode({ getter: () => pos, setter: this.setLocation(), self: this });

        return [typeInfo, textInfo, posInfo];
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
        this.onLabelChange();
    }
}
