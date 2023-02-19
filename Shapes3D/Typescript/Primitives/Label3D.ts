import { Color3, MeshBuilder, StandardMaterial, DynamicTexture, Mesh, Scene, Vector3 } from 'babylonjs';
import { Base3D } from './Base3D';
import { Tools } from '../shared/foTools';
import { AdvancedDynamicTexture, Rectangle, TextBlock } from 'babylonjs-gui';

type DynamicTextureDimensions = { width: number; height: number };
export class Label3D extends Base3D {
    public text: string;
    public fontSize: number;
    public height: number;
    public width: number;

    private font: string;
    private material: StandardMaterial;

    private setMaterial(dimensions: DynamicTextureDimensions) {
        const texture = new DynamicTexture('text3d', { ...dimensions });
        this.material = new StandardMaterial('textMaterial');
        this.material.diffuseTexture = texture;
        texture.drawText(this.text, null, null, this.font, '#000000', '#ffffff');
    }

    private setPlaneDimensions(): DynamicTextureDimensions {
        this.height = (this.fontSize * 3) / 48;

        //Set height for dynamic texture
        const dynamicTextureHeight = 1.5 * this.fontSize;
        const ratio = this.height / dynamicTextureHeight;

        const temp = new DynamicTexture('DynamicTexture', 64);
        const tempContext = temp.getContext();
        tempContext.font = this.font;
        const dynamicTextureWidth = tempContext.measureText(this.text).width + 8;

        this.width = dynamicTextureWidth * ratio;

        return { width: dynamicTextureWidth, height: dynamicTextureHeight };
    }

    private followCamera() {
        // Assume we are following first scene camera.
        const camera = this.mesh.getScene().cameras[0];
        this.mesh.rotationQuaternion = camera.absoluteRotation;
        if (Boolean(camera)) {
            camera.onViewMatrixChangedObservable.add(() => {
                this.mesh.rotationQuaternion = camera.absoluteRotation;
            });
            // this.mesh.onBeforeDrawObservable.add((mesh) => {
            //     mesh.rotationQuaternion = camera.absoluteRotation;
            // });
        }
    }

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;

        const dtDimensions = this.setPlaneDimensions();
        this.setMaterial(dtDimensions);

        const plane = MeshBuilder.CreatePlane('plane', { width: this.width, height: this.height, sideOrientation: Mesh.BACKSIDE }, scene);
        plane.material = this.material;

        // const backside = plane.clone();
        // backside.rotation.y = Tools.toRadians(180);
        // backside.material = plane.material;

        // this.mesh = Mesh.MergeMeshes([plane, backside]);
        this.mesh = Mesh.MergeMeshes([plane]);
        this.followCamera();
        return this.mesh;
    }

    public updateText(text: string, scene: Scene) {
        this.text = text;
        this.mesh.dispose(false, true);
        this.mesh = null;

        this.mesh = this.createMesh(scene);
        return this.mesh;
    }

    constructor(text: string = 'No Text', fontSize = 84) {
        super();
        this.text = text;
        this.fontSize = fontSize;
        this.font = `bold ${this.fontSize}px Arial`;
    }
}
