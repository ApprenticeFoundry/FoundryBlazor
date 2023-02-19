
import { HighResBox, HighResPosition } from '../models';
import { UDTO_Body } from '../models';
import { IModelBase3D, ModelBase3D } from './ModelBase3D';

export class BodyModelBase extends ModelBase3D<UDTO_Body> {
    

    protected get box(): HighResBox {
        if (!this.data.boundingBox ) {
            this.data.boundingBox = new HighResBox();
        }
        return this.data.boundingBox;
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
