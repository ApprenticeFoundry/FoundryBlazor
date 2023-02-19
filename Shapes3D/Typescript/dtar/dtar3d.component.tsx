import { Context, useContext as getSharedProps } from 'solid-js';
import { DTAR3dModel as Model } from './dtar3d.model';
import css from './dtar3d.module.css';
import { onMount } from 'solid-js';
import { K2ComponentContext } from '../shared/component-model';
// import { DT_AssetReference } from '../models/DT_AssetReference';
import { DT_Document } from '../models/DT_Document';
import { UDTO_Platform } from '../models';
import { DTAR3dEntity } from './dtar3d-entity.model';

export interface ViewProps {
    DTAR3dEntity: DTAR3dEntity;
}
class SharedProps {
    M: Model;
    This: Component;
}
class Component extends K2ComponentContext<SharedProps, Component> {
    constructor() {
        super();
    }

    public Root(This: Component, props: ViewProps) {
        const M = new Model();
        M.props = props;

        SharedContext = This.setSharedProps({ M, This });

        onMount(() => M.onCanvasReady());

        return (
            <SharedContext.Provider value={SharedContext.defaultValue}>
                <div class={css['world-container']}>
                    {/* <This.ActionPalette /> */}
                    <This.World />
                    {/* <This.DebugInfo /> */}
                </div>
            </SharedContext.Provider>
        );
    }

    private ActionPalette() {
        const { M } = getSharedProps(SharedContext);
        return (
            <div class={css['action-palette-container']}>
                <div class="row align-items-center mb-2">
                    <span class="col-sm-2 fw-bolder">
                        <label for="boid-count-input">1 Grid Square = {M.scale()} Meters</label>
                        <input type="text" placeholder="enter a number" class="form-control" id="boid-count-input" onInput={M.inputScale} value={M.scale()}></input>
                    </span>
                </div>
            </div>
        );
    }

    private DebugInfo() {
        const { M } = getSharedProps(SharedContext);
        return (
            <div class={css['debug-container']}>
                Platform Timestamp from server:<span ref={(el) => (M.platformTimestamp = el)}></span>
            </div>
        );
    }

    private World() {
        const { M } = getSharedProps(SharedContext);
        return (
            <>
                <canvas ref={(el) => (M.canvas = el)} class={css['world-element']} />
            </>
        );
    }
}

let SharedContext: Context<SharedProps>;
export const Dtar3D = Component.display(Component);

export default Dtar3D;
