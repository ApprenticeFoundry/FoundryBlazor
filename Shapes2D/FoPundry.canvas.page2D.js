/*
    FoundryBlazor.undo.js part of the FoundryJS project
    Copyright (C) 2012 Steve Strong  http://foundryjs.azurewebsites.net/

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

var Foundry = Foundry || {};
FoundryBlazor.canvas = FoundryBlazor.canvas || {};

//Page2DCanvas
(function (ns, fo, create, undefined) {
  var tween = create.Tween;
  var utils = fo.utils;

  var Page2DCanvas = function (properties, subcomponents, parent) {
    var canvasElement =
      (properties && properties.canvasElement) ||
      document.getElementById("myCanvas") ||
      document.createElement("canvas");

    var page2DSpec = {
      canvas: function () {
        return canvasElement;
      },
      stage: function () {
        var stage = create.createStage(this.canvas, true);
        if (!stage) return;
        //stage.enableMouseOver(10);
        //stage.enableDOMEvents(true);
        create.Touch && create.Touch.enable(stage);

        if (stage) stage.mouseMoveOutside = false;
        return stage;
      },
      geom: function () {
        return this.stage;
      },
      update: function () {
        return this.geom ? true : false;
      },

      scale: 1.0,
      panX: 0.0,
      panY: 0.0,
      rotation: 0,
      headerText: function () {
        return this.myName;
      },
      title: function () {
        return this.headerText;
      },
      canvasWidth: function () {
        return this.canvas ? this.canvas.width : 0;
      },
      canvasHeight: function () {
        return this.canvas ? this.canvas.height : 0;
      },
      canvasWH: function () {
        var width = this.canvasWidth;
        var height = this.canvasHeight;
        return "W:{0}  H:{1}".format(width, height);
      },
      drawingWH: function () {
        var width = this.drawingWidth / this.pixelsPerInch;
        var height = this.drawingHeight / this.pixelsPerInch;
        return "sz[{0}x{1}]in".format(width, height);
      },
      pixelsPerInch: 150,
      drawingWidth: function () {
        return 16 * this.pixelsPerInch;
      }, //16 inches wide
      drawingHeight: function () {
        return 9 * this.pixelsPerInch;
      }, //9 inches wide
      drawingMargin: function () {
        return 0.25 * this.pixelsPerInch;
      }, //.25 inch margin all around
      surfaceWidth: function () {
        return this.drawingWidth + 2 * this.drawingMargin;
      },
      surfaceHeight: function () {
        return this.drawingHeight + 2 * this.drawingMargin;
      },
      showBackground: true,
      background: function () {
        var result = create.createShape();
        if (result) {
          var g = result.graphics;
          g.beginFill("gray")
            .drawRect(
              this.drawingMargin,
              this.drawingMargin,
              this.drawingWidth,
              this.drawingHeight
            )
            .endFill();
        }
        return result;
      },
      canDoWheelZoom: true,
      showGrid: false,
      gridX: function () {
        return 1 * this.pixelsPerInch;
      }, //1 inch
      gridY: function () {
        return 0.5 * this.pixelsPerInch;
      }, //.5 inch
      grid: function () {
        var result = create.createShape();
        result &&
          this.gridGeom(
            this.drawingWidth,
            this.drawingHeight,
            this.drawingMargin,
            this.gridX,
            this.gridY,
            result,
            true
          );
        return result;
      },
    };

    page2DSpec = utils.mixin(page2DSpec, {
      draggable: true,
      selectionSet: function () {
        return fo.makeCollection();
      },
      dropTargetSet: function () {
        return fo.makeCollection();
      },
    });

    this.base = ns.FoundryShape;
    this.base(utils.union(page2DSpec, properties), subcomponents, parent);
    this.myType = (properties && properties.myType) || "Page";

    if (this.canvas && this.stage && this.draggable) {
      this.setupDragAndDrop(true);
      this.setupScaleBindings();
    }

    this.doAnimations = true;
    // this.setAnimationsOn(this.doAnimations);

    this._defaultPinX = 100;
    this._defaultPinY = 100;
    this._defaultPinXDelta = 0;
    this._defaultPinYDelta = 0;
    this.resetDropDelta();

    this._pasteAreaPinX = 0;
    this._pasteAreaPinY = 0;
    this._pasteAreaGap = 10;

    this.resetPasteArea();

    return this;
  };

  Page2DCanvas.prototype = (function () {
    var anonymous = function () {
      this.constructor = Page2DCanvas;
    };
    anonymous.prototype = ns.FoundryShape.prototype;
    return new anonymous();
  })();

  ns.Page2DCanvas = Page2DCanvas;
  utils.isa2DCanvas = function (obj) {
    return obj instanceof Page2DCanvas ? true : false;
  };
  utils.isaPage = function (obj) {
    return obj instanceof Page2DCanvas ? true : false;
  };

  ns.makePage = function (properties, subcomponents, parent) {
    var page = new Page2DCanvas(properties, subcomponents, parent);
    if (!page.myName) page.myName = page.myType || "Page";
    return page;
  };

  ns.makePage2D = function (id, spec, parent) {
    var canvasElement = fo.utils.isString(id)
      ? document.getElementById(id)
      : id;
    var properties = spec || {};
    properties.canvasElement = canvasElement;
    var page = ns.makePage(properties, {}, parent);
    return page;
  };

  Page2DCanvas.prototype.clearStage = function () {
    this.stage.removeAllChildren();
  };

  Page2DCanvas.prototype.rePaint = function () {
    this.stage && this.stage.update();
  };

  Page2DCanvas.prototype.render = function (stage, context) {
    var rootStage = stage ? stage : this.stage;
    if (this.showBackground) {
      this.establishChild(this.background);
    }

    if (this.showGrid) {
      this.establishChild(this.grid);
    }

    this.Subcomponents.forEach(function (subshape) {
      subshape.render(rootStage || this.geom, context);
    });

    //this.updatePIP();
  };

  Page2DCanvas.prototype.updateStage = function (clear) {
    var self = this;

    var turnOnAnimation = animationsAreOn;
    if (animationsAreOn) {
      self.setAnimationsOn(false);
    }
    //force all updates to include stage

    if (clear) {
      self.clearStage();
      self.selectShape(undefined, true);
    }
    //same as stage but forces compute
    self.render(self.geom, self.context);

    this.rePaint();

    self.setAnimationsOn(turnOnAnimation);

    return self;
  };

  var animationsAreOn = false;
  Page2DCanvas.prototype.setAnimationsOn = function (val) {
    var self = this;
    var stage = self.geom; //same as stage but forces compute

    if (!create.Ticker) return;

    //the animation loop forces the stage update
    if (val) {
      create.Ticker.useRAF = true;
      create.Ticker.setFPS(30);
      create.Ticker.addEventListener("tick", stage);
    } else {
      create.Ticker.removeEventListener("tick", stage);
    }
    animationsAreOn = val;
  };

  Page2DCanvas.prototype.resetPasteArea = function (gap) {
    this._pasteAreaPinX = this.drawingMargin;
    this._pasteAreaPinY = this.drawingMargin;
    this._pasteAreaGap = gap ? gap : 10;
    return this._pasteAreaGap;
  };

  Page2DCanvas.prototype.showPasteArea = function (onComplete) {
    var page = this;
    var x1 = 0;
    var y1 = 0;
    var x2 = page.surfaceWidth;
    var y2 = page.surfaceHeight;

    var scale = Math.min(page.canvasWidth / x2, page.canvasHeight / y2);
    var panX = (page.canvasWidth - scale * x2) / 2.0;
    var panY = (page.canvasHeight - scale * y2) / 2.0;

    page.transformTo(scale, panX + 100, panY, 200, function () {
      onComplete && onComplete();
    });
    return this;
  };

  Page2DCanvas.prototype.moveToPasteArea = function (shape) {
    var page = this;

    var pinX = page._pasteAreaPinX - shape.width / 2 - page._pasteAreaGap;
    var pinY = page._pasteAreaPinY + page._pasteAreaGap;
    shape.setXY(pinX, pinY);

    var depth = 1; //shape.mySiblingsMaxValue('groupDepth', 1);
    page._pasteAreaPinY = pinY + shape.height * depth;

    fo.publish("ShapeMoved", [shape.myName, undefined, shape]);
    setTimeout(function () {
      page.introduceShape(shape);
    }, 300);
  };

  Page2DCanvas.prototype.resetDropDelta = function () {
    this._defaultPinXDelta = this._defaultPinYDelta = 0;
    this._defaultPinX = 1.5 * this.drawingMargin - this.panX / this.scale;
    this._defaultPinY = 1.5 * this.drawingMargin - this.panY / this.scale;

    this.resetPasteArea();
  };

  Page2DCanvas.prototype.overrideDefaultPinXPinY = function (pinX, pinY) {
    this._defaultPinX = pinX;
    this._defaultPinY = pinY;
    this._defaultPinXDelta = this._defaultPinYDelta = 0;
  };

  Page2DCanvas.prototype.defaultPinX = function () {
    var result = this._defaultPinX + this.defaultPinXDelta();
    return result;
  };

  Page2DCanvas.prototype.defaultPinY = function () {
    var result = this._defaultPinY + this.defaultPinYDelta();
    return result;
  };

  Page2DCanvas.prototype.defaultXY = function () {
    var result = [this.defaultPinX(), this.defaultPinY()];
    return result;
  };

  Page2DCanvas.prototype.defaultPinXDelta = function () {
    if (this._defaultPinXDelta > 200) this._defaultPinXDelta = 0;
    this._defaultPinXDelta = this._defaultPinXDelta + 20;
    return this._defaultPinXDelta;
  };

  Page2DCanvas.prototype.defaultPinYDelta = function () {
    if (this._defaultPinYDelta > 200) this._defaultPinYDelta = 0;
    this._defaultPinYDelta = this._defaultPinYDelta + 20;
    return this._defaultPinYDelta;
  };

  Page2DCanvas.prototype.rootPage = function () {
    return this;
  };

  Page2DCanvas.prototype.isPage = function () {
    return true;
  };

  Page2DCanvas.prototype.setCanvasWidth = function (width) {
    if (this.canvas && this.canvas.width != width) {
      this.canvas.width = width;
    }
    this.smashProperty("canvasWidth");
  };

  Page2DCanvas.prototype.setCanvasHeight = function (height) {
    if (this.canvas && this.canvas.height != height) {
      this.canvas.height = height;
    }
    this.smashProperty("canvasHeight");
  };

  Page2DCanvas.prototype.establishChild = function (child, index) {
    var stage = this.stage;
    if (stage.getChildIndex(child) == -1) {
      stage.addChild(child);
      if (index != undefined) stage.setChildIndex(child, index);
    }
    return child;
  };

  Page2DCanvas.prototype.setupScaleBindings = function () {
    var self = this;
    self.addBinding("scale", function (prop, obj) {
      var scale = prop.getValue();
      var stage = obj.stage;
      stage.scaleX = stage.scaleY = scale;
    });

    self.addBinding("panX", function (prop, obj) {
      var value = prop.getValue();
      obj.resetDropDelta();
      var stage = obj.stage;
      stage.x = value;
    });

    self.addBinding("panY", function (prop, obj) {
      var value = prop.getValue();
      obj.resetDropDelta();
      var stage = obj.stage;
      stage.y = value;
    });
  };

  //define as noop first then replace
  Page2DCanvas.prototype.updatePIP = function () {};
  Page2DCanvas.prototype.setPIP = function (pip) {
    var page = this;
    if (page.pictureInPicture) {
      throw new Error("cannot reset pip in this page");
    }
    page.pictureInPicture = pip;
    pip.myParent = page;
    page.updatePIP = function () {
      fo.publish("updatePanZoom", [page.pictureInPicture, page]);
    };
    return page.pictureInPicture;
  };

  Page2DCanvas.prototype.getPIP = function () {
    return this.pictureInPicture;
  };

  Page2DCanvas.prototype.setPIPSize = function (width, height, element) {
    var page = this;
    if (this.canvas && page.getPIP()) {
      fo.publish("sizePanZoom", [page.getPIP(), page, width, height, element]);
    }
  };

  Page2DCanvas.prototype.setPIPPosition = function (width, height, element) {
    var page = this;
    if (this.canvas && page.getPIP()) {
      fo.publish("positionPanZoom", [
        page.getPIP(),
        page,
        width,
        height,
        element,
      ]);
    }
  };

  Page2DCanvas.prototype.introduceShape = function (shape, callback) {
    var geom = shape.geom;

    callback && callback();
    geom.scaleX = geom.scaleY = 0;
    geom.rotation = -45;

    var page = this;
    geom.visible = true;

    shape.MorphTo(
      geom,
      { scaleY: 1.0, scaleX: 1.0, rotation: 0 },
      200,
      create.Ease.linear,
      function () {
        //backInOut
        geom.scaleY = geom.scaleX = 1.0;
        geom.rotation = 0;
        page.updatePIP();
      }
    );
  };

  Page2DCanvas.prototype.farewellShape = function (shape, callback) {
    this.selectShape(undefined, true);

    var page = this;
    var geom = shape.geom;

    shape.MorphTo(
      geom,
      { scaleY: 0, scaleX: 0, rotation: 45 },
      200,
      create.Ease.linear,
      function () {
        //backInOut
        geom.visible = false;
        callback && callback();
        page.updatePIP();
      }
    );
  };

  Page2DCanvas.prototype.MorphTo = function (
    geom,
    rule,
    time,
    ease,
    onComplete
  ) {
    if (this.doAnimations && geom) {
      var tw = tween.get(geom).to(rule, time, ease);
      tw.call(function () {
        onComplete && onComplete();
      });
    } else {
      onComplete && onComplete();
    }
  };

  Page2DCanvas.prototype.DrawDot = function (x, y, f, r) {
    var shape = create.createShape();
    if (!shape) return;
    var fill = f ? f : "#F00";
    var radius = r ? r : 6;

    shape
      .setTransform(x, y)
      .graphics.beginFill(fill)
      .drawCircle(0, 0, radius)
      .endFill();

    this.stage.addChild(shape);
  };

  Page2DCanvas.prototype.gridGeom = function (
    width,
    height,
    margin,
    deltaX,
    deltaY,
    shape,
    clear
  ) {
    var g = shape.graphics;

    //draw the body of the grid.
    if (clear) g.clear();
    g.beginStroke("#000");

    //first draw Verticle lines
    var left = margin;
    var top = margin;
    var right = margin + width;
    var bottom = margin + height;
    g.drawRect(left, top, width, height);

    var x = left;
    while (x < right) {
      g.moveTo(x, top);
      g.lineTo(x, bottom);
      x += deltaX;
    }
    var y = top;
    while (y < bottom) {
      g.moveTo(left, y);
      g.lineTo(right, y);
      y += deltaY;
    }

    return g;
  };
})(FoundryBlazor.canvas, Foundry, FoundryBlazor.createjs);

//Page2DCanvas  mouse interaction
(function (ns, fo, create, undefined) {
  var tween = create.Tween;
  var utils = fo.utils;

  Page2DCanvas = ns.Page2DCanvas;

  var CTRLKEY = false;
  var ALTKEY = false;
  var SHIFTKEY = false;

  function doKeyDown(evt) {
    CTRLKEY = evt.ctrlKey;
    ALTKEY = evt.altKey;
    SHIFTKEY = evt.shiftKey;
  }

  function doKeyUp(evt) {
    CTRLKEY = false;
    ALTKEY = false;
    SHIFTKEY = false;
  }

  //http://jsfiddle.net/SVArR/
  window.addEventListener("keydown", doKeyDown, true);
  window.addEventListener("keyup", doKeyUp, true);

  Page2DCanvas.prototype.zoomBy = function (value) {
    var page = this;
    page.scale *= value;
    return this;
  };

  Page2DCanvas.prototype.setZoomTo = function (targetValue) {
    var page = this;
    var newFactor = targetValue / page.scale;
    page.zoomBy(newFactor);
    return this;
  };

  Page2DCanvas.prototype.setPanTo = function (xTargetValue, yTargetValue) {
    var page = this;
    page.panX = xTargetValue;
    page.panY = yTargetValue;
    return this;
  };

  Page2DCanvas.prototype.panBy = function (deltaX, deltaY) {
    var page = this;
    page.panX += deltaX;
    page.panY += deltaY;
    return this;
  };

  Page2DCanvas.prototype.transformTo = function (
    scale,
    panX,
    panY,
    delay,
    onComplete
  ) {
    var page = this;
    var stage = page.stage;

    var time = delay ? delay : 0;
    var tw = tween
      .get(stage)
      .to(
        { x: panX, y: panY, scaleX: scale, scaleY: scale },
        time,
        create.Ease.linear
      );

    tw.call(function () {
      //fo.digestLock(page, function () {
      page.setPanTo(panX, panY);
      page.setZoomTo(scale);
      //}, );
      onComplete && onComplete();
    });

    return this;
  };

  Page2DCanvas.prototype.zoomToFit = function (onComplete) {
    var page = this;
    var x1 = 0;
    var y1 = 0;
    var x2 = page.surfaceWidth;
    var y2 = page.surfaceHeight;
    //determine the full size of the content
    page.Subcomponents.forEach(function (item) {
      x1 = Math.min(x1, item.pinX);
      y1 = Math.min(y1, item.pinY);
      var w = item.width ? item.width : 0;
      x2 = Math.max(x2, item.pinX + w);
      var h = item.height ? item.height : 0;
      y2 = Math.max(y2, item.pinY + h);
    });

    var scale = Math.min(page.canvasWidth / x2, page.canvasHeight / y2);
    var panX = (page.canvasWidth - scale * x2) / 2.0;
    var panY = (page.canvasHeight - scale * y2) / 2.0;

    page.transformTo(scale, panX, panY, 500, function () {
      page.updatePIP();
      onComplete && onComplete();
    });
    return this;
  };

  Page2DCanvas.prototype.zoom1To1 = function (onComplete) {
    var page = this;
    page.transformTo(1, 0, 0, 500, function () {
      page.updatePIP();
      onComplete && onComplete();
    });
    return this;
  };

  Page2DCanvas.prototype.selectShapeHitTest = function (gX, gY) {
    var elements = this.selectionSet.elements;
    for (var i = 0; i < elements.length; i++) {
      var subShape = elements[i];
      if (subShape.myParent != this) continue;

      var found = subShape.isShapeHit(gX, gY, subShape.width, subShape.height);
      if (found == subShape) return subShape;

      //testing the geom will include the subshapes so you will get a hit if any geom
      //is found for now this is not what we want
      //var subGeom = subShape.geom;
      //var subPt = subGeom.globalToLocal(gX, gY);
      //if (subGeom.hitTest(subPt.x, subPt.y)) {
      //    return subShape;
      //}
    }
  };

  Page2DCanvas.prototype.clear = function () {
    this.selectShape(undefined, true);
    this.removeAllSubcomponents();
  };

  Page2DCanvas.prototype.selectShape = function (shape, clear) {
    var selections = this.selectionSet;

    if (shape && shape.isSelected) {
      fo.publish("ShapeSelected", [this, shape, selections]);
      return;
    }

    if (!shape && selections.count == 0) {
      fo.publish("ShapeSelected", [this, shape, selections]);
      return;
    }

    var previousShape = selections.first();
    if (clear) {
      selections.forEach(function (item) {
        item.isSelected = item.isEditing = false;
        item.setVisualState("isSelected");
      });
      selections.clear();
      this.selectDropTarget(undefined, true);
    }

    if (shape) {
      previousShape = undefined;
      shape.isSelected = true;
      if (selections.count == 0) selections.push(shape);
      shape.setVisualState("isSelected");

      //force selection to top
      var stage = this.geom;
      var index = stage.children.length - 1;
      stage.setChildIndex(shape.geom, index);
    }

    fo.publish("ShapeSelected", [
      this,
      shape ? shape : previousShape,
      selections,
    ]);
  };

  Page2DCanvas.prototype.selectedShape = function () {
    var selections = this.selectionSet;
    var shape = selections.count > 0 ? selections.last() : undefined;
    return shape;
  };

  Page2DCanvas.prototype.selectedContext = function () {
    var shape = this.selectedShape();
    return shape ? shape.context : undefined;
  };

  Page2DCanvas.prototype.selectDropTarget = function (shape, clear) {
    var dropTargets = this.dropTargetSet;

    if (shape && shape.isActiveTarget) return;

    if (clear) {
      dropTargets.forEach(function (item) {
        item.isActiveTarget = false;
        item.setVisualState("isActiveTarget");
      });
      dropTargets.clear();
    }

    if (shape) {
      shape.isActiveTarget = true;
      shape.setVisualState("isActiveTarget");
      if (dropTargets.count == 0) dropTargets.push(shape);
    }

    //ns.publish('DropTargetSelected', [this, shape, dropTargets]);
  };

  Page2DCanvas.prototype.selectedDropTarget = function () {
    var dropTargets = this.dropTargetSet;
    var shape = dropTargets.count > 0 ? dropTargets.last() : undefined;
    return shape;
  };

  Page2DCanvas.prototype.shapeDropped = function (shape, offset, ev) {
    var x = ev.stageX;
    var y = ev.stageY;

    //var pinX = (x - this.panX) / this.scale - offset.x;
    //var pinY = (y - this.panY) / this.scale - offset.y;

    var page = this;
    var loc = { pinX: shape.pinX, pinY: shape.pinY, index: shape.myIndex() };

    var found = page.subcomponentHitTest(x, y, true);
    if (found) {
      //found.isActiveTarget = true; //.setVisualState('canDropOnGroup');
      //capturing should be enough to trigger recompute
      //of properties that depend on shapeDepth
      var oldParent = found.capture(shape);

      fo.publish("ShapeReparented", [shape, oldParent, shape.myParent, loc]);
    }
  };

  Page2DCanvas.prototype.shapePulled = function (shape, group, offset, ev) {
    var x = ev ? ev.stageX : 0;
    var y = ev ? ev.stageY : 0;

    var page = this;
    var pinX = (x - page.panX) / page.scale - offset.x;
    var pinY = (y - page.panY) / page.scale - offset.y;

    //var loc = { pinX: shape.pinX, pinY: shape.pinY, index: shape.myIndex() }
    var loc = { pinX: pinX, pinY: pinY, index: shape.myIndex() };

    var oldParent = page.capture(shape);
    fo.publish("ShapeReparented", [shape, oldParent, shape.myParent, loc]);

    shape.setXY(pinX, pinY);
    fo.publish("ShapeMoved", [shape.myName, undefined, shape]);

    shape.doUpdate();
  };

  Page2DCanvas.prototype.setupDragAndDrop = function (state) {
    var page = this;
    var stage = page.geom; //makes sure the stage / geom property exist

    var canvas = page.canvas;
    var selections = page.selectionSet;
    var offset = { x: 0, y: 0 };
    var isMoving = false;
    var ignoreMoving = false;

    var isMouseDown = false;
    var isBoxing = false;
    var boxingStart = { x: 0, y: 0 };
    var boxingFinish = { x: 0, y: 0 };

    var pullingOffset = { x: 0, y: 0 };
    var pullingOnShape = undefined;
    var pullingOnParent = undefined;
    var isPullingCount = 0;
    var isPullingStart = 10;
    var isPullingMAX = 18;
    var panOffset = { x: 0, y: 0 };

    page.allowSingleTouchPan = false;
    page.publishTouchBoxingEvents = false;

    if (stage && stage.addEventListener) {
      if (!canvas.canDoMouse) {
        stage.addEventListener("stagemousedown", onMouseDownState, false);
        stage.addEventListener("stagemousemove", onMouseMoveState, false);
        stage.addEventListener("stagemouseup", onMouseUpState, false);

        stage.addEventListener("dblclick", onStageDoubleClick, false);
        canvas.canDoMouse = true;
      }
    }

    if (canvas && canvas.addEventListener && this.canDoWheelZoom) {
      if (!canvas.canDoWheelZoom) {
        canvas.addEventListener("mousewheel", mouseWheelHandler, false);
        canvas.addEventListener("DOMMouseScroll", mouseWheelHandler, false);
        canvas.canDoWheelZoom = true;
      }
    }

    function makeBox(s, f) {
      var box = {
        x1: Math.min(s.x, f.x),
        x2: Math.max(s.x, f.x),
        y1: Math.min(s.y, f.y),
        y2: Math.max(s.y, f.y),
      };
      box.width = box.x2 - box.x1;
      box.height = box.y2 - box.y1;
      box.area = box.width * box.height;
      return box;
    }

    var boxingRect = create.createShape();
    boxingRect.alpha = 0.1;

    function renderBox(box) {
      var g = boxingRect.graphics;
      g.clear();
      if (box) {
        g.beginFill("#ff0000").drawRect(box.x1, box.y1, box.width, box.height);
        stage.addChild(boxingRect);
      }
      stage.update();
    }

    function panStage(ev) {
      var x = ev.stageX;
      var y = ev.stageY;

      var panX = x - panOffset.x;
      var panY = y - panOffset.y;

      //fo.digestLock(page,function () {
      page.setPanTo(panX, panY);
      page.updatePIP();
      //});
    }

    function zoomStageToCenter(gX, gY, zoom, ev) {
      //page.DrawDot(0, 0, 'yellow', 50);

      //page.DrawDot(ev.x, ev.y, 'blue', 18);
      //page.DrawDot(ev.clientX, ev.clientY, 'red', 15);
      //page.DrawDot(ev.pageX, ev.pageY, 'yellow', 12);
      //page.DrawDot(ev.layerX, ev.layerY, 'black', 9);
      //page.DrawDot(ev.offsetX, ev.offsetY, 'green', 6);
      //page.DrawDot(ev.screenX, ev.screenY, 'white', 3);
      //Layer, offest, Page, Screen
      //page.DrawDot(ev.x, ev.y, 'blue', 7);
      //page.DrawDot(ev.x, ev.y, 'blue', 7);

      //you need to track this position in global space
      //so you can return it to the same location on the screen
      var pt1 = page.geom.globalToLocal(gX, gY);
      //fo.digestLock(page,function () {
      page.zoomBy(zoom);
      page.updatePIP();

      //});

      //once the zoom is applied, measure where the global point has moved to
      //then pan back so it is in the center...
      var pt2 = page.geom.localToGlobal(pt1.x, pt1.y);
      // fo.digestLock(page, function () {
      page.panBy(gX - pt2.x, gY - pt2.y);
      page.updatePIP();
      //});
    }

    function touchZoomStageToCenter(gX, gY, newZoom, ev) {
      //page.DrawDot(0, 0, 'yellow', 50);

      //page.DrawDot(ev.x, ev.y, 'blue', 18);
      //page.DrawDot(ev.clientX, ev.clientY, 'red', 15);
      //page.DrawDot(ev.pageX, ev.pageY, 'yellow', 12);
      //page.DrawDot(ev.layerX, ev.layerY, 'black', 9);
      //page.DrawDot(ev.offsetX, ev.offsetY, 'green', 6);
      //page.DrawDot(ev.screenX, ev.screenY, 'white', 3);
      //Layer, offest, Page, Screen
      //page.DrawDot(ev.x, ev.y, 'blue', 7);
      //page.DrawDot(ev.x, ev.y, 'blue', 7);

      //you need to track this position in global space
      //so you can return it to the same location on the screen
      var pt1 = page.geom.globalToLocal(gX, gY);
      // fo.digestLock(page, function () {
      page.setZoomTo(newZoom);
      page.updatePIP();
      //});

      //once the zoom is applied, measure where the global point has moved to
      //then pan back so it is in the center...
      var pt2 = page.geom.localToGlobal(pt1.x, pt1.y);
      //fo.digestLock(page, function () {
      page.panBy(gX - pt2.x, gY - pt2.y);
      page.updatePIP();
      //});
    }

    fo.subscribe("touchZoomStageToCenter", function (gX, gY, zoom, ev) {
      touchZoomStageToCenter(gx, gy, zoom, ev);
    });

    function cancelBubble(e) {
      if (!e) e = window.event;

      //IE9 & Other Browsers
      if (e.stopPropagation) {
        e.stopPropagation();
      } else {
        //IE8 and Lower
        e.cancelBubble = true;
      }
    }

    function onMouseDownState(ev) {
      cancelBubble(ev);
      var x = ev.stageX;
      var y = ev.stageY;

      //if (stage.mouseInBounds)
      //    ns.publish("mouse down", [ev, stage]);

      if (CTRLKEY) {
        panOffset = { x: x - stage.x, y: y - stage.y };
        stage.addEventListener("stagemousemove", panStage);
        stage.addEventListener("stagemouseup", function (evt) {
          stage.removeEventListener("stagemousemove", panStage);
        });
        return;
      }

      offset = { x: 0, y: 0 };
      pullingOnShape = pullingOnParent = undefined;

      var shape = page.selectShapeHitTest(x, y);
      if (shape) {
        var pt = shape.geom.globalToLocal(x, y);
        offset = { x: pt.x, y: pt.y };
      }

      var newShape = shape ? undefined : page.subcomponentHitTest(x, y);
      if (newShape && !shape) {
        shape = newShape;
        var pt = shape.geom.globalToLocal(x, y);
        offset = { x: pt.x, y: pt.y };
        if (shape.myParent != page) {
          shape.setVisualState("canPullFromGroup");
          pullingOnShape = shape;
          pullingOnParent = shape.myParent; //item
          isPullingCount = isPullingMAX;

          var geom = pullingOnParent.geom;
          var pt = geom.globalToLocal(x, y);

          pullingOffset = { x: pt.x, y: pt.y };
        }
      }

      //fo.digestLock(page, function () {
      page.selectShape(shape, true);
      page.selectDropTarget(undefined, true);
      // });

      isMouseDown = true;
      isMoving = shape !== undefined;

      isBoxing = !isMoving && page.publishTouchBoxingEvents;
      if (isBoxing) {
        //start and finish are in drawing coords
        boxingStart = page.geom.globalToLocal(x, y);
        renderBox();
        var box = makeBox(boxingStart, boxingStart);
        fo.publish("BoxingStarted", [page, box, ev]);
      }

      //start panning the screen is nothing selected
      if (!shape && page.allowSingleTouchPan) {
        panOffset = { x: x - stage.x, y: y - stage.y };
        stage.addEventListener("stagemousemove", panStage);
        stage.addEventListener("stagemouseup", function (evt) {
          stage.removeEventListener("stagemousemove", panStage);
        });
      }
    }

    function onMouseMoveState(ev) {
      if (!isMouseDown || ignoreMoving) return;
      cancelBubble(ev);

      var x = ev.stageX;
      var y = ev.stageY;

      //if (stage.mouseInBounds)
      //    ns.publish("mouse move", [ev, stage]);

      if (pullingOnShape && isPullingCount >= 0) {
        var geom = pullingOnShape.geom;
        var parentGeom = pullingOnParent.geom;

        isPullingCount -= 1;
        if (pullingOnParent && isPullingCount > isPullingStart) {
          return;
        }
        if (pullingOnParent && isPullingCount > 0) {
          var delta = isPullingStart - isPullingCount - 3;

          var skew = 2 * delta;
          pullingOnShape.setVisualState(
            "canBePullFromGroup",
            { skewY: skew, skewX: skew },
            200,
            function () {
              geom && (geom.skewX = geom.skewY = skew);
            }
          );

          var scale = 1.01 + delta / 10;
          pullingOnParent.setVisualState(
            "canHaveItemPulled",
            { scaleX: scale },
            200,
            function () {
              parentGeom && (parentGeom.scaleX = scale);
            }
          );
          return;
        }

        //this will snap it back to normal
        if (isPullingCount == 0) {
          isPullingCount = -1;
          parentGeom.removeChild(geom);

          //ignoreMoving = true;
          var pt = page.geom.globalToLocal(x, y);
          geom.x = pt.x - offset.x;
          geom.y = pt.y - offset.y;

          stage.addChild(geom);

          page.selectShape(pullingOnShape, true);
          page.selectDropTarget(undefined, true);

          //maybe too aggressize for this version
          //wait until drop,  but it does heal up nice
          //page.captureSubcomponent(pullingOnShape, undefined, true);

          pullingOnShape.setVisualState(
            "wasPulledFromGroup",
            { skewY: 0, skewX: 0 },
            200,
            function () {
              //backInOut
              geom.skewY = 0;
              geom.skewX = 0;
              //ignoreMoving = false;

              //var scale = 1;
              if (pullingOnParent) {
                pullingOnParent.setVisualState(
                  "canHaveItemPulled",
                  { scaleX: 1 },
                  300,
                  function () {
                    parentGeom && (parentGeom.scaleX = 1);
                  }
                );
              }
            }
          );
        }

        return;
      }

      var shape = page.selectedShape();
      //not needed... if (shape) page.resetDropDelta();

      if (shape && isMoving) {
        var geom = shape.geom;
        var pt = page.geom.globalToLocal(x, y);
        geom.x = pt.x - offset.x;
        geom.y = pt.y - offset.y;
        geom.skewX = geom.skewY = 0;
        shape.glueShapeMoved(
          shape,
          geom.x,
          geom.y,
          shape.width,
          shape.height,
          true
        );

        //used to redraw the current drawing on pan/zoom window
        //not necessary until shape reached final position
        fo.publish("ShapeMoving", [page, shape, ev]);
        //SRS publish ShapeMoving  means that page does not need to update             page.updatePIP();
      }

      if (isBoxing && isMouseDown) {
        //start and finish are in drawing coords
        boxingFinish = page.geom.globalToLocal(x, y);
        var box = makeBox(boxingStart, boxingFinish);
        renderBox(box);
        fo.publish("BoxingSizing", [page, box, ev]);
      }

      if (!isMoving || !shape || pullingOnShape) return;
      if (shape && !shape.canBeGrouped) return;

      //var dropTarget = page.selectedDropTarget();

      //ns.trace && ns.trace.clr();
      //this section find targets for dropping the shape into groups
      var found = page.subcomponentHitTest(x, y, true);
      found = found && found.canGroupItems ? found : undefined;
      //if (dropTarget == found) return;
      if (found) {
        page.selectDropTarget(found, true);
        return;
      }
      page.selectDropTarget(found, true);
    }

    function onMouseUpState(ev) {
      cancelBubble(ev);
      var x = ev.stageX;
      var y = ev.stageY;

      isMouseDown = false;
      //if (stage.mouseInBounds)
      //    ns.publish("mouse up", [ev, stage]);

      if (isBoxing) {
        boxingFinish = page.geom.globalToLocal(x, y);
        var box = makeBox(boxingStart, boxingFinish);
        //locate shapes based on Start and Finished range...
        var group = undefined;
        page.Subcomponents.forEach(function (item) {
          var geom = item.geom;
          if (geom.x < box.x1) return;
          if (geom.x > box.x2) return;
          if (geom.y < box.y1) return;
          if (geom.y > box.y2) return;

          group = group ? group : [];
          group.push(item);
        });
        renderBox();
        fo.publish("BoxingFinished", [page, box, group, ev]);
      }

      //user decided to not complete removing this subshape so clean up geometry
      if (pullingOnShape && isPullingCount > 0) {
        var geom = pullingOnShape.geom;
        var parentGeom = pullingOnParent.geom;

        var skew = 0;
        pullingOnShape.setVisualState(
          "canBePullFromGroup",
          { skewY: skew, skewX: skew },
          300,
          function () {
            geom && (geom.skewX = geom.skewY = skew);
          }
        );

        var scale = 1;
        pullingOnParent.setVisualState(
          "canHaveItemPulled",
          { scaleX: scale },
          300,
          function () {
            parentGeom && (parentGeom.scaleX = scale);
          }
        );
        isMoving = false;
      }

      var shape = page.selectedShape();
      if (shape && isMoving) {
        isMoving = false;
        var geom = shape.geom;
        var pt = page.geom.globalToLocal(x, y);
        geom.x = pt.x - offset.x;
        geom.y = pt.y - offset.y;

        fo.publish("ShapeMoving", [page, shape, ev]);

        //did we land on something?
        var target = page.subcomponentHitTest(x, y, true);
        page.selectDropTarget(target, true);

        if (pullingOnShape && isPullingCount == -1) {
          page.shapePulled(pullingOnShape, pullingOnParent, offset, ev);
        } else if (pullingOnParent && isPullingCount >= 0) {
          pullingOnParent.setVisualState("canNotDropOnGroup");
        } else if (target && target.canGroupItems) {
          //did we
          page.shapeDropped(shape, offset, ev);
        } else {
          shape.pinX = geom.x;
          shape.pinY = geom.y;
          shape.angle = 0;

          if (!shape.isInGroup()) {
            var context = shape.context;
            fo.publish("ShapeMoved", [shape.myName, context, shape]);
          }
        }
      }

      page.selectDropTarget(undefined, true);

      //clean up any shapes that are still targets
      page.applyToChildren(function (item) {
        if (item.isActiveTarget) {
          item.isActiveTarget = false;
        }
        item.setVisualState && item.setVisualState("isActiveTarget");
      }, true);

      pullingOnShape = undefined;
      pullingOnParent = undefined;
    }

    //only send event if a shape is hit
    function onStageDoubleClick(ev) {
      cancelBubble(ev);

      var x = ev.stageX;
      var y = ev.stageY;

      //only send event if a shape is hit
      var shape = page.selectedShape();
      if (shape) {
        //&& page.subcomponentHitTest(x, y, true)) {
        shape.setVisualState("SelectedForEdit"); //one day if you can edit the shape directly
        var action = CTRLKEY ? "OpenNav" : "OpenEdit";
        fo.publish("doubleClick", [shape, shape.context, action]);
      }
    }

    //http://stackoverflow.com/questions/5189968/zoom-canvas-to-mouse-cursor

    function mouseWheelHandler(ev) {
      cancelBubble(ev);
      if (!page.canDoWheelZoom) {
        return;
      }

      var scale = 1.1;
      var zoom =
        Math.max(-1, Math.min(1, ev.wheelDelta || -ev.detail)) > 0
          ? scale
          : 1 / scale;

      var x = ev.offsetX;
      var y = ev.offsetY;

      zoomStageToCenter(x, y, zoom, ev);
    }

    //....................................................................
    //code for touch pinch zoom and pan..
    var pinchCenter;
    var baseDistance = 1;
    var baseZoom = 1;
    var pinchZoom = {};
    var pinchZoomMove = undefined;
    var touchPan = {};
    var touchPanMove = undefined;

    function distanceBetweenPoints(obj) {
      var list = fo.utils.forEachValue(obj, function (key, val) {
        return { x: val.clientX, y: val.clientX };
      });

      var x = Math.pow(list[1].x - list[0].x, 2);
      var y = Math.pow(list[1].y - list[0].y, 2);
      var result = Math.ceil(Math.sqrt(x + y));
      return result;
    }

    function averageBetweenPoints(obj) {
      var list = fo.utils.forEachValue(obj, function (key, val) {
        return { x: val.clientX, y: val.clientX };
      });

      return { x: (list[1].x + list[0].x) / 2, y: (list[1].y + list[0].y) / 2 };
    }

    var currentScale = 1;

    var onPointerMove = function (ev) {
      //cancelBubble(ev);

      var id = ev.pointerId;
      if (ev.pointerType == "touch") {
        if (pinchZoomMove == undefined) {
          pinchZoom[id] = ev; //keep track of current location..
        } else {
          pinchZoomMove[id] = ev; //keep track of current location..
        }
        touchPanMove = {};
        if (touchPan[id]) {
          touchPanMove[id] = ev;
        }
      }

      if (pinchZoomMove && pinchCenter) {
        //if this exist it only has 2 keys..
        var distance = distanceBetweenPoints(pinchZoomMove); //make the scale less senstive
        var newScale = distance / baseDistance;

        var delta = Math.abs(newScale - currentScale);
        if (delta < 0.1 || delta > 0.8) return;
        currentScale = newScale;

        //if ( ns.trace ) {
        //    ns.trace.clr();
        //    ns.trace.info('baseDistance');
        //    ns.trace.dir(baseDistance);
        //    ns.trace.info('distance');
        //    ns.trace.dir(distance);
        //    ns.trace.info('new scale');
        //    ns.trace.dir(newScale);
        //    ns.trace.info('pinchCenter');
        //    ns.trace.dir(pinchCenter);
        //}

        var gX = pinchCenter.x;
        var gY = pinchCenter.y;
        touchZoomStageToCenter(gX, gY, currentScale, ev);
      }

      if (touchPanMove && touchPanMove[id]) {
        var panX = touchPanMove[id].clientX - panOffset.x;
        var panY = touchPanMove[id].clientY - panOffset.y;
        //fo.digestLock(page, function () {
        page.setPanTo(panX, panY);
        page.updatePIP();
        // });
      }
    };

    var onPointerUp = function (ev) {
      //cancelBubble(ev);
      var id = ev.pointerId;
      if (ev.pointerType == "touch") {
        pinchZoomMove = undefined;
        touchPanMove = undefined;

        if (pinchZoom[id]) {
          delete pinchZoom[id];
        }
        if (touchPan[id]) {
          delete touchPan[id];
        }
      }
    };

    var onPointerDown = function (ev) {
      //cancelBubble(ev);
      var id = ev.pointerId;
      if (ev.pointerType == "touch") {
        touchPan = {}; //only track the last touch
        if (page.selectedShape() == undefined) {
          touchPan[id] = ev;
          panOffset = { x: ev.clientX - stage.x, y: ev.clientY - stage.y };
        }

        pinchZoom[id] = ev;
        var len = Object.keys(pinchZoom).length;
        if (len == 2) {
          //capture current state to do zoom math
          pinchZoomMove = {};
          for (var attr in pinchZoom) {
            pinchZoomMove[attr] = pinchZoom[attr];
          }
          //now compute the distance between used to scale...
          pinchCenter = averageBetweenPoints(pinchZoomMove);
          baseDistance = distanceBetweenPoints(pinchZoomMove);
          baseZoom = page.scale;
        }
      }
    };

    if (canvas && canvas.addEventListener) {
      canvas.addEventListener("pointerdown", onPointerDown, false);
      canvas.addEventListener("pointermove", onPointerMove, false);
      canvas.addEventListener("pointerup", onPointerUp, false);
    }

    //....................................................................
  };
})(FoundryBlazor.canvas, Foundry, FoundryBlazor.createjs);

(function (ns, cv, undefined) {
  if (!ns.establishType) return;

  ns.establishType("Page", {}, cv.makePage);
})(Foundry, FoundryBlazor.canvas);
