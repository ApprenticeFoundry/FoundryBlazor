/*
    FoundryBlazor.canvas.panzoom.js part of the FoundryJS project
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
FoundryBlazor.createjs = this.createjs || {};

(function (ns, fo, createjs, undefined) {
  var utils = fo.utils;

  var panning = false;

  var PanAndZoomWindow = function (properties, subcomponents, parent) {
    //define a hit testable object to render and represent the true drawing size
    var viewWindowSpec = {
      geom: function () {
        var geom = new createjs.Shape();
        geom.alpha = 0.2;
        return geom;
      },
    };

    var panAndZoomSpec = {
      title: "pan zoom",
      canvasWH: function () {
        var width = this.canvasWidth;
        var height = this.canvasHeight;
        return "w:{0}  h:{1}".format(width, height);
      },
      percentMargin: -0.02,
      percentSize: 0.25,
      draggable: true,
      canDoWheelZoom: false,
      parentScale: function () {
        return this.myParent ? this.myParent.scale : 1.0;
      },
      scaleFactor: function () {
        return this.scale / this.parentScale;
      },
      drawingGeom: function () {
        var result = new createjs.Shape();
        return result;
      },
      viewWindowShape: function () {
        var result = ns.makeShape(viewWindowSpec, {}, this);
        this.addSubcomponent(result);
        return result;
      },
      viewWindowGeom: function () {
        var geom = this.viewWindowShape.geom;
        return geom;
      },
    };

    this.base = ns.Page2DCanvas;
    this.base(utils.union(panAndZoomSpec, properties), subcomponents, parent);
    this.myType = "PanAndZoomWindow";
    return this;
  };

  PanAndZoomWindow.prototype = (function () {
    var anonymous = function () {
      this.constructor = PanAndZoomWindow;
    };
    anonymous.prototype = ns.Page2DCanvas.prototype;
    return new anonymous();
  })();

  ns.PanAndZoomWindow = PanAndZoomWindow;

  ns.makePanZoomWindow2D = function (id, spec, parent) {
    var element = fo.utils.isString(id) ? document.getElementById(id) : id;
    var canvasElement =
      (spec && spec.canvasElement) ||
      element ||
      document.createElement("canvas");

    var properties = spec || {};
    properties.canvasElement = canvasElement;
    var pzSelf = new PanAndZoomWindow(properties, {}, parent);
    var pzSelfParent = pzSelf.myParent;

    fo.subscribe("updatePanZoom", function (self, selfParent) {
      if (self && self != pzSelf) return;
      if (selfParent && selfParent != pzSelfParent) return;

      pzSelf.draw(pzSelfParent, "black");
      fo.publish("pip", ["update"]);
    });

    fo.subscribe(
      "ShapeReparented",
      function (child, oldParent, newParent, loc) {
        //fo.publish('info', ['ShapeReparented']);
        pzSelf.draw(pzSelfParent, "black");
        fo.publish("pip", ["reparent"]);
      }
    );

    fo.subscribe(
      "sizePanZoom",
      function (self, selfParent, width, height, element) {
        if (self && self != pzSelf) return;
        if (selfParent && selfParent != pzSelfParent) return;

        //fo.publish('info', ['sizePanZoom']);
        pzSelf.setCanvasWidth(width * pzSelf.percentSize);
        pzSelf.setCanvasHeight(height * pzSelf.percentSize);
        pzSelf.zoomToFit(function () {
          pzSelf.draw(pzSelfParent);
        });
        fo.publish("pip", ["resize"]);
      }
    );

    fo.subscribe(
      "positionPanZoom",
      function (self, selfParent, width, height, element) {
        if (self && self != pzSelf) return;
        if (selfParent && selfParent != pzSelfParent) return;

        // fo.publish('info', ['positionPanZoom']);
        //element.style.left = width - ((width * pzSelf.percentMargin) + pzSelf.canvasWidth) + 'px';
        //element.style.top = height + 30 - ((height * pzSelf.percentMargin) + pzSelf.canvasHeight) + 'px';
        element.style.width = 10 + width * pzSelf.percentSize + "px";
        element.style.height = 10 + height * pzSelf.percentSize + "px";
        fo.publish("pip", ["repositioned"]);
      }
    );

    fo.subscribe("ShapeMoved", function (uniqueID, model, shape) {
      //if model is undefined then you are panning
      //so just repaint in black
      panning = false;

      //fo.publish('info', ['ShapeMoved']);
      model && pzSelf.draw(pzSelfParent, "black");
      fo.publish("pip", ["moved"]);
    });

    //this is used to move the small redish window that pans the drawing surface
    fo.subscribe("ShapeMoving", function (page, shape, ev) {
      if (!shape || !page) return;

      //adjust the pan on the page
      // fo.publish('info', ['ShapeMoving']);

      if (page == pzSelf && shape == pzSelf.viewWindowShape) {
        var viewWindowGeom = pzSelf.viewWindowGeom;
        var scale = pzSelfParent.scale;
        var panX = viewWindowGeom.x * scale;
        var panY = viewWindowGeom.y * scale;
        pzSelfParent.setPanTo(-panX, -panY);
        pzSelf.draw(pzSelfParent, "black");
        panning = true;
      } else if (!panning) {
        pzSelf.draw(pzSelfParent, "black");
      }
      fo.publish("pip", ["moving"]);
    });

    return pzSelf;
  };

  utils.isaPanAndZoomWindow = function (obj) {
    return obj instanceof PanAndZoomWindow ? true : false;
  };

  PanAndZoomWindow.prototype.draw = function (page, color) {
    function renderPageOutline(g, obj, x, y) {
      obj.Subcomponents.forEach(function (item) {
        var geom = item.geom;
        var locX = x + geom.x;
        var locY = y + geom.y;
        g.drawRect(locX, locY, item.width, item.height);

        if (item.Subcomponents.count) {
          renderPageOutline(g, item, locX, locY);
        }
      });
    }

    var pzSelf = this;
    var stage = pzSelf.stage;

    var drawingGeom = pzSelf.drawingGeom;
    pzSelf.establishChild(drawingGeom);

    var psScale = pzSelf.scale;
    var viewWindowShape = pzSelf.viewWindowShape;
    var viewWindowGeom = pzSelf.viewWindowGeom;

    var g = drawingGeom.graphics;
    g.clear();

    //do all the to draw the gray page outline
    //SRS mod draw the page size and location
    var x = pzSelf.drawingMargin;
    var y = pzSelf.drawingMargin;
    var w = pzSelf.drawingWidth;
    var h = pzSelf.drawingHeight;
    g.beginFill("gray").drawRect(x, y, w, h).endFill();

    if (page.Subcomponents.count) {
      g.beginFill(color ? color : "black");
      renderPageOutline(g, page, 0, 0);
      g.endFill();
    }

    //do all the to draw the redish window
    //maybe manage this geometry the same way as a 2D Shape
    pzSelf.establishChild(viewWindowGeom);

    //this is an anti scale pattern
    var scale = page.scale;
    var pinX = page.panX / scale;
    var pinY = page.panY / scale;
    var width = page.canvasWidth / scale;
    var height = page.canvasHeight / scale;

    viewWindowShape.width = width;
    viewWindowShape.height = height;

    viewWindowGeom.x = -pinX;
    viewWindowGeom.y = -pinY;

    var g = viewWindowGeom.graphics;
    g.clear();
    g.beginFill("red").drawRect(0, 0, width, height).endFill();

    stage.update();
  };

  PanAndZoomWindow.prototype.computeViewPortOffset = function (x, y) {
    var viewPort = this.selected;
    var offset = viewPort.globalToLocal(x, y);
    return offset;
  };

  PanAndZoomWindow.prototype.adjustPanUsingView = function (x, y, offset) {
    var viewPort = this.selected;
    var scale = (-1.0 * this.scale) / this.sizeFactor;

    var pt = viewPort.globalToLocal(x, y);

    viewPort.x += pt.x - offset.x;
    viewPort.y += pt.y - offset.y;
    return viewPort;
  };

  PanAndZoomWindow.prototype.computePanUsingView = function () {
    var viewPort = this.selected;
    var scale = (-1.0 * this.scale) / this.sizeFactor;

    return { x: viewPort.x * scale, y: viewPort.y * scale };
  };

  PanAndZoomWindow.prototype.isViewPortHit = function (gX, gY) {
    var viewPort = this.selected;

    var obj = this;
    var parent = obj.myParent;
    var sizeFactor = obj.sizeFactor;
    var scale = obj.scale;

    var viewPortLeft = (obj.panX * sizeFactor) / scale;
    var viewPortTop = (obj.panY * sizeFactor) / scale;
    var viewPortWidth = (parent.canvasWidth * sizeFactor) / scale;
    var viewPortHeight = (parent.canvasHeight * sizeFactor) / scale;

    var pt1 = viewPort.localToGlobal(0, 0);
    if (gX < pt1.x) return false;
    if (gY < pt1.y) return false;

    var pt2 = viewPort.localToGlobal(viewPortWidth, viewPortHeight);
    if (gX > pt2.x) return false;
    if (gY > pt2.y) return false;

    if (gY >= pt1.y && gY <= pt2.y) {
      return true;
    }
  };
})(FoundryBlazor.canvas, Foundry, FoundryBlazor.createjs);
