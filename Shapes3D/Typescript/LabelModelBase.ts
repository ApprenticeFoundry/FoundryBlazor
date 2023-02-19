
import { HighResPosition, UDTO_Label } from '../models';
import { IModelBase3D, ModelBase3D } from './ModelBase3D';

export class LabelModelBase extends ModelBase3D<UDTO_Label> {

    protected get text() {
        return this.data.text;
    }

    protected get pos(): HighResPosition {
        if (!this.data.position ) {
            this.data.position = new HighResPosition();
        }
        return this.data.position;
    }

    public applyAnimation():IModelBase3D 
    {
        this.data.update3D(this.mesh());
        return this;
    }

    constructor(properties?: any) {
        super();
        this.override(properties);
    }
}
