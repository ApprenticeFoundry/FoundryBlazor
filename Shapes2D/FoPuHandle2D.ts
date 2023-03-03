import { cPoint2D } from './foGeometry2D';
import { Matrix2D } from './foMatrix2D';

import { iPoint2D } from '../foInterface';

import { foObject } from '../foObject.model';

import { foGlyph2D } from './foGlyph2D.model';
import { Lifecycle } from '../foLifecycle';
import { BroadcastChange } from '../foChange';

import { foHandle } from '../foHandle';

export class foHandle2D extends foHandle {
  //get size(): number { return this._size || 10.0; }
  //set size(value: number) { this._size = value; }

  protected _x: number;
  protected _y: number;
  protected _angle: number;

  get x(): number {
    return this._x || 0.0;
  }
  set x(value: number) {
    this.smash();
    this._x = value;
  }
  get y(): number {
    return this._y || 0.0;
  }
  set y(value: number) {
    this.smash();
    this._y = value;
  }

  get angle(): number {
    return this._angle || 0.0;
  }
  set angle(value: number) {
    this.smash();
    this._angle = value;
  }
  public rotation = (): number => {
    return this.angle;
  }

  public drawHover: (ctx: CanvasRenderingContext2D) => void;
  public preDraw: (ctx: CanvasRenderingContext2D) => void;
  public postDraw: (ctx: CanvasRenderingContext2D) => void;

  protected _matrix: Matrix2D;
  protected _invMatrix: Matrix2D;
  smash() {
    //console.log('smash matrix')
    this._matrix = undefined;
    this._invMatrix = undefined;
  }

  constructor(
    properties?: any,
    subcomponents?: Array<foHandle2D>,
    parent?: foObject
  ) {
    super(properties, subcomponents, parent);
  }

  public dropAt(
    x: number = Number.NaN,
    y: number = Number.NaN,
    angle: number = Number.NaN
  ) {
    if (!Number.isNaN(x)) this.x = x;
    if (!Number.isNaN(y)) this.y = y;
    if (!Number.isNaN(angle)) this.angle = angle;
    return this;
  }

  public moveTo(loc: iPoint2D, offset?: iPoint2D) {
    //let x = loc.x + (offset ? offset.x : 0);
    //let y = loc.y + (offset ? offset.y : 0);

    this.doMoveProxy && this.doMoveProxy(loc);
    BroadcastChange.moved(this, loc);
    Lifecycle.handle(this, loc);
    return this;
  }

  updateContext(ctx: CanvasRenderingContext2D) {
    const mtx = this.getMatrix();
    ctx.transform(mtx.a, mtx.b, mtx.c, mtx.d, mtx.tx, mtx.ty);
    ctx.globalAlpha *= this.opacity;
  }

  getGlobalMatrix() {
    const mtx = new Matrix2D(this.getMatrix());
    const parent = <foGlyph2D>this.myParent();
    if (parent) {
      mtx.prependMatrix(parent.getGlobalMatrix());
    }
    return mtx;
  }

  getMatrix() {
    if (this._matrix === undefined) {
      this._matrix = new Matrix2D();
      const delta = this.size / 2;
      this._matrix.appendTransform(
        this.x,
        this.y,
        1,
        1,
        this.rotation(),
        0,
        0,
        delta,
        delta
      );
    }
    return this._matrix;
  }

  getInvMatrix() {
    if (this._invMatrix === undefined) {
      this._invMatrix = this.getMatrix().invertCopy();
    }
    return this._invMatrix;
  }

  localToGlobal(x: number, y: number, pt?: cPoint2D) {
    const mtx = this.getGlobalMatrix();
    return mtx.transformPoint(x, y, pt);
  }

  globalToLocal(x: number, y: number, pt?: cPoint2D) {
    const inv = this.getGlobalMatrix().invertCopy();
    return inv.transformPoint(x, y, pt);
  }

  localToGlobalPoint(pt: cPoint2D): cPoint2D {
    const mtx = this.getGlobalMatrix();
    return mtx.transformPoint(pt.x, pt.y, pt);
  }

  globalCenter(): cPoint2D {
    const { x, y } = this.pinLocation();
    const mtx = this.getGlobalMatrix();
    return mtx.transformPoint(x, y);
  }

  public getOffset = (loc: iPoint2D): iPoint2D => {
    const x = this.x;
    const y = this.y;
    return new cPoint2D(x - loc.x, y - loc.y);
  }

  protected localHitTest = (hit: any): boolean => {
    const { x, y } = hit as iPoint2D;
    const loc = this.globalToLocal(x, y);

    if (loc.x < 0) return false;
    if (loc.x > this.size) return false;

    if (loc.y < 0) return false;
    if (loc.y > this.size) return false;
    //foObject.beep();
    return true;
  }

  public hitTest = (hit: any): boolean => {
    return this.isHitable && this.isVisible && this.localHitTest(hit);
  }

  public render(ctx: CanvasRenderingContext2D, deep: boolean = true) {
    if (this.isInvisible) return;
    ctx.save();

    this.updateContext(ctx);

    this.preDraw && this.preDraw(ctx);
    this.draw(ctx);
    this.drawHover && this.drawHover(ctx);
    this.postDraw && this.postDraw(ctx);

    ctx.restore();
  }

  public draw = (ctx: CanvasRenderingContext2D): void => {
    ctx.fillStyle = this.color;
    ctx.lineWidth = 1;
    ctx.globalAlpha = this.opacity;

    ctx.fillRect(0, 0, this.size, this.size);
  }
}
