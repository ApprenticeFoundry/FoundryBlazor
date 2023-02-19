import { Toast } from '../components/toast';
import { Accessor, createSignal, onCleanup, Setter } from 'solid-js';
import { K2ComponentModel } from '../shared/component-model';
import { DTAR3dEntity } from './dtar3d-entity.model';

import { K2InputEvent } from '../shared/types';
import { HighResBox } from '../models';
import { ViewProps } from './dtar3d.component';

export class DTAR3dModel extends K2ComponentModel {
    public props: ViewProps;

    public scale: Accessor<number>;
    private setScale: Setter<number>;

    public triggerChange: Accessor<number>;
    private setTriggerChange: Setter<number>;

    private defaultScale!: number; //Meters
    private model: DTAR3dEntity;

    public canvas: HTMLCanvasElement;
    public platformTimestamp: HTMLSpanElement;
    public isShowMiniMap = false;

    constructor() {
        super();
        this.init();

        onCleanup(() => {
            this.destroy();
        });
    }

    private init() {
        this.defaultScale = 10; //meters per grid line

        [this.scale, this.setScale] = createSignal<number>(this.defaultScale);
        [this.triggerChange, this.setTriggerChange] = createSignal<number>(0);
    }

    private destroy() {
        this.model && this.model.destroy();
    }

    public onCanvasReady() {
        this.model = this.props.DTAR3dEntity;

        // Need canvas element before starting flock world
        if (this.canvas) {
            const box = new HighResBox({
                width: 130,
                height: 130,
                depth: 130
            });
            this.model.createSim(this.canvas, box, this.setTriggerChange);
            this.setTriggerChange(1);
        } else {
            Toast.error('Cannot start; no canvas available');
        }
    }

    public inputScale(evt: K2InputEvent) {
        this.setScale(Number(evt.currentTarget.value));
        this.model.setDrawingScale(this.scale());
    }

    public pause() {
        //this.model.stopAnimation();
    }

    public resume() {
        //this.model.startAnimation();
    }

    public clear() {
        //this.model.clear();
    }

    public setPositionOfMiniMapIframe(): string {
        // Mini-Map size is 90vh and is centered on screen; need to adjust for its margin
        const mapWidth = window.innerHeight * 0.9; // 90vh
        const scale = 0.35;
        const mapMargin = (window.innerWidth - mapWidth) * 0.5;
        const miniWindowWidth = window.innerWidth * scale;
        const fudgeLeft = 20;
        const moveLeft = -1 * (((window.innerWidth - miniWindowWidth) * 0.5) / scale + mapMargin) + fudgeLeft;

        const miniWindowHeight = window.innerHeight * scale;
        const fudgeTop = 80;
        const moveUp = -1 * (((window.innerHeight - miniWindowHeight) * 0.5) / scale) + fudgeTop;

        return `scale(${scale}) translate(${moveLeft}px, ${moveUp}px)`;
    }
}
