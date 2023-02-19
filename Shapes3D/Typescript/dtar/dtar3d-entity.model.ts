import { DTAR3DWorld } from './dtar3d.world';

import { Subject } from 'rxjs';
import { Setter } from 'solid-js';
import { HighResBox, UDTO_Base, UDTO_Body, UDTO_Platform } from '../models';

import { DynamicTexture, MeshBuilder, Scene, StandardMaterial, Vector3 } from 'babylonjs';
import { PlatformWrapper, BodyWrapper } from '../models3D';
import { Constructable } from '../shared/types';
//import { Block_Platform } from '../testing/block-test';
import { Toast } from '../components/toast';
import { DT_Document } from '../models/DT_Document';
import { RestService } from '../services/rest-service';

export interface IDTAR3dEntity {
    platformTimestamp?: HTMLSpanElement;
    platforms: UDTO_Platform[];
}
export class DTAR3dEntity extends UDTO_Base {
    public platforms: UDTO_Platform[];
    public wrappers: PlatformWrapper[];

    private world: DTAR3DWorld;
    _platformLookup: { [key: string]: UDTO_Platform } = {};
    _wrapperLookup: { [key: string]: PlatformWrapper } = {};

    private done$ = new Subject<any>();

    private frameID: number;
    private drawingScale: number = 1.0;
    private _canvas: HTMLCanvasElement;

    private display: Setter<number>;

    constructor(properties?: IDTAR3dEntity) {
        super();
        super.override(properties);
    }

    private isRunning(): boolean {
        return true;
    }

    public setDrawingScale(scale: number) {
        this.drawingScale = scale;
        return this;
    }

    private doGlbSubTest(world: DTAR3DWorld): BodyWrapper {
        const sub = 'https://iobtdataadapter.blob.core.windows.net/blob-container-1/sub.glb';
        const shape = new BodyWrapper({ data: UDTO_Body.newGlbFile(sub) });
        shape.render3D(world.scene());
        shape.RotateBy(0, Math.PI / 2, 0);
        shape.MoveBy(0, -100, 0);

        var box = world.box();
        var direction = 1;
        shape.setPreRender((frameID) => {
            shape.MoveBy(0, 0, direction);
            const pos = shape.mesh().position;
            if (pos.z > box.depth || pos.z < -box.depth) {
                direction = -1 * direction;
                shape.RotateBy(0, Math.PI, 0);
            }
        });
        return shape;
    }

    private doGlbHydroTest(world: DTAR3DWorld): BodyWrapper {
        const sub = 'https://iobtdataadapter.blob.core.windows.net/blob-container-1/Hydro_All_Cleaned.obj';
        const shape = new BodyWrapper({ data: UDTO_Body.newGlbFile(sub) });
        shape.render3D(world.scene());
        //shape.RotateBy(0,Math.PI/2,0);
        //shape.MoveBy(0,100,0);

        return shape;
    }

    // private doRenderShape(world: DTAR3DWorld): BodyWrapper {
    //     const url = this.document?.url;
    //     let shape;
    //     if (url) {
    //         const shape = new BodyWrapper({ data: UDTO_Body.newGlbFile(url) });
    //         shape.render3D(world.scene());
    //     } else {
    //         Toast.error(`No URL available for ${this.document?.guid}`);
    //     }
    //     return shape;
    // }
    private doBoidTest(world: DTAR3DWorld): BodyWrapper {
        const shape = new BodyWrapper({ data: UDTO_Body.newBoid() });
        shape.render3D(world.scene());

        let ang = 0;
        let range = 200;
        let alt = 200;
        const turn = Math.PI / 180;
        shape.RotateBy(0, 0, Math.PI / 10);

        shape.setPreRender((frameID) => {
            ang += turn;
            const pitch = turn;
            const x = range * Math.sin(ang);
            const y = 50 * Math.sin(ang) + alt;
            const z = range * Math.cos(ang);
            shape.MoveTo(x, y, z);
            shape.RotateBy(0, turn, 0);
        });
        return shape;
    }

    public createSim(canvas: HTMLCanvasElement, box: HighResBox, display: Setter<number>): PlatformWrapper[] {
        this.display = display;
        this._canvas = canvas;
        this.world = new DTAR3DWorld(this._canvas, box);

        const [wPlatform, wWrap] = this.renderWorldPlatform(this.world.scene(), box);
        this.panID = 'demo';

        this.wrappers = [];
        this.platforms.forEach((platform, idx) => {
            const data = this.establishPlatform(platform.platformName, platform);
            const pWrap = this.addPlatformWrapper(new PlatformWrapper({ data }));
            const parentNode = pWrap.render3D(this.world.scene());
            pWrap.wrapMembers().forEach((item) => {
                const childNode = item.render3D(this.world.scene());
                childNode.parent = parentNode;
            });
            parentNode.position = new Vector3(idx * 10, 0, idx * 20);
            this.wrappers.push(pWrap);
        });

        this.world.startRenderLoop((frameID: number) => {
            this.frameID = frameID;
            if (this.isRunning()) {
                this.platformWrappers.forEach((item) => item.getContainer3D(this.world.scene()));
                this.platformWrappers.forEach((item) => item.runStep(frameID));
            }
        });

        return this.wrappers;
    }

    private addPlatformWrapper(wrap: PlatformWrapper): PlatformWrapper {
        this._wrapperLookup[wrap.uniqueGuid] = wrap;
        return wrap;
    }

    get platformWrappers(): PlatformWrapper[] {
        return Object.values(this._wrapperLookup);
    }

    private renderWorldPlatform(scene: Scene, box: HighResBox) {
        const platform = this.establishPlatform(
            'world',
            new UDTO_Platform({
                boundingBox: box
            })
        );

        const worldWrap = this.addPlatformWrapper(
            new PlatformWrapper({
                data: platform,
                showFloor: true,
                showAxis: false
            })
        );
        worldWrap.render3D(scene);
        //this.testLabelRender(scene);

        return [platform, worldWrap];
    }

    private testLabelRender(scene: Scene) {
        //Set font type
        var font_type = 'Arial';

        //Set width an height for plane
        var planeWidth = 10;
        var planeHeight = 3;

        //Create plane
        var plane = MeshBuilder.CreatePlane('plane', { width: planeWidth, height: planeHeight }, scene);

        //Set width and height for dynamic texture using same multiplier
        var DTWidth = planeWidth * 60;
        var DTHeight = planeHeight * 60;

        //Set text
        var text = 'Some words to fit';

        //Create dynamic texture
        var dynamicTexture = new DynamicTexture('DynamicTexture', { width: DTWidth, height: DTHeight }, scene);

        //Check width of text for given font type at any size of font
        var ctx = dynamicTexture.getContext();
        var size = 12; //any value will work
        ctx.font = size + 'px ' + font_type;
        var textWidth = ctx.measureText(text).width;

        //Calculate ratio of text width to size of font used
        var ratio = textWidth / size;

        //set font to be actually used to write text on dynamic texture
        var font_size = Math.floor(DTWidth / (ratio * 1)); //size of multiplier (1) can be adjusted, increase for smaller text
        var font = font_size + 'px ' + font_type;

        //Draw text
        dynamicTexture.drawText(text, null, null, font, '#000000', '#ffffff', true);

        //create material
        var mat = new StandardMaterial('mat', scene);
        mat.diffuseTexture = dynamicTexture;

        //apply material
        plane.material = mat;
        plane.position.set(10, 20, 30);

        return scene;
    }

    // renderSample<T extends UDTO_Platform>(type: Constructable<T>, scene: Scene): [UDTO_Platform, PlatformWrapper] {
    //     const platform = new type();
    //     const place = this.establishPlatform(platform.platformName, platform);
    //     const pWrap = this.addPlatformWrapper(
    //         new PlatformWrapper({
    //             data: place,
    //             showVolume: false
    //         })
    //     );
    //     const parentNode = pWrap.render3D(scene);
    //     pWrap.wrapMembers().forEach((item) => {
    //         const childNode = item.render3D(scene);
    //         childNode.parent = parentNode;
    //     });
    //     return [platform, pWrap];
    // }

    get allPlatforms(): UDTO_Platform[] {
        return Object.values(this._platformLookup);
    }

    public establishPlatform(name: string, platform?: UDTO_Platform): UDTO_Platform {
        let found = this._platformLookup[name];
        if (found && platform) {
            found.override({
                boundingBox: platform.boundingBox,
                position: platform.position
            });
        } else if (!found && platform) {
            this._platformLookup[name] = platform;
            found = platform;
        } else if (!found && !platform) {
            found = new UDTO_Platform({
                platformName: name
            });
            this._platformLookup[name] = found;
        }
        return found;
    }

    public associateBodiesWithPlatform() {}

    public toggleInspector(state: boolean) {
        if (state) {
            this.world.inspectorOn();
        } else {
            this.world.inspectorOff();
        }
    }

    public destroy() {
        this.done$.next('DONE');
        this.done$.complete();
        this.world.stopRenderLoop();
    }
}
