import { Color3, DynamicTexture, Mesh, MeshBuilder, Scene, SceneLoader, StandardMaterial, Vector3 } from 'babylonjs';
import { HighResBox, UDTO_Body } from '../models';
import 'babylonjs-loaders';

import { Base3D } from './Base3D';
type DynamicTextureDimensions = { width: number; height: number };

// https://iobtdataadapter.blob.core.windows.net/blob-container-1/sub.glb
// https://iobtdataadapter.blob.core.windows.net/blob-container-1/box-truck-white.glb
// https://iobtdataadapter.blob.core.windows.net/blob-container-1/flir-a70.glb

export class GlbFile3D extends Base3D {
    body: UDTO_Body;

    getFilename(body: UDTO_Body) {
        const symbol = body.symbol.split('/');

        const url = symbol.slice(0, -1).join('/') + '/';
        url.replace(':/', '://');
        const fileName = symbol[symbol.length - 1];
        return fileName;
    }

    getUrl(body: UDTO_Body) {
        const symbol = body.symbol.split('/');
        const url = symbol.slice(0, -1).join('/') + '/';
        url.replace(':/', '://');
        return url;
    }

    loadFile(scene: Scene, func: (mesh: Mesh) => void) {
        const url = this.getUrl(this.body);
        const fileName = this.getFilename(this.body);
        console.log('loading ', url, fileName);

        SceneLoader.ImportMeshAsync('', `${url}`, fileName, scene)
            .then((value) => {
                const { meshes } = value;
                const root = meshes[0] as Mesh;
                // root.getChildMeshes().forEach((item) => {
                //     if (item.name.indexOf('Sail') >= 0) {
                //         item.visibility = 0.25;
                //     }
                // });

                func(root);
            })
            .catch((rejected: any) => {
                console.log('Problem loading model=', rejected, url, fileName);
            });
    }

    private setPlaneDimensions(filename: string): DynamicTextureDimensions {
        const fontSize = 84;
        const height = (fontSize * 3) / 48;

        //Set height for dynamic texture
        const dynamicTextureHeight = 1.5 * fontSize;
        const ratio = height / dynamicTextureHeight;

        const temp = new DynamicTexture('DynamicTexture', 64);
        const tempContext = temp.getContext();
        tempContext.font = `bold ${fontSize}px Arial`;
        const text = `${filename} Loading...`;
        const dynamicTextureWidth = tempContext.measureText(text).width + 18;
        //this.width = dynamicTextureWidth * ratio;

        return { width: dynamicTextureWidth, height: dynamicTextureHeight };
    }
    private setMaterial(filename: string, dimensions: DynamicTextureDimensions): StandardMaterial {
        const fontSize = 84;
        const texture = new DynamicTexture('text3d', { ...dimensions });
        const material = new StandardMaterial('mat1');
        material.diffuseTexture = texture;
        const font = `bold ${fontSize}px Arial`;
        const text = ` Loading ${filename}...`;
        texture.drawText(text, 0, 0, font, '#000000', '#ffffff');
        return material;
    }

    createMesh(scene: Scene): Mesh {
        if (this.mesh) return this.mesh;

        //https://forum.babylonjs.com/t/left-and-right-handed-shenanagins/17049/4
        //https://doc.babylonjs.com/divingDeeper/mesh/displayBoundingBoxes#drawing-a-bounding-box-around-a-single-object

        this.loadFile(scene, (realMesh) => {
            const parent = this.mesh.parent;
            this.mesh.dispose(true); //this forces place holder from the scene

            //realMesh.showBoundingBox = false;
            this.mesh.material = null;
            this.mesh = realMesh;
            this.mesh.parent = parent;
            console.log('the mesh is loaded');
        });

        //this.mesh = MeshBuilder.CreateBox('Placeholder', { ...this.body.boundingBox }, scene);
        //this.mesh.material =new StandardMaterial('GLB');
        //this.mesh.material.alpha = 0.0;

        const fileName = this.getFilename(this.body);
        const dtDimensions = this.setPlaneDimensions(fileName);
        const material = this.setMaterial(fileName, dtDimensions);

        const box = new HighResBox({
            width: 5,
            height: 1,
            depth: 1
        });
        this.mesh = MeshBuilder.CreatePlane('Placeholder', { ...box }, scene);
        this.mesh.material = material;

        return this.mesh;
    }

    constructor(body: UDTO_Body) {
        super();
        this.body = body;
    }
}
