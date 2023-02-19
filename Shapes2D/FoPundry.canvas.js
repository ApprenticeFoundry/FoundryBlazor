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
FoundryBlazor.cv = FoundryBlazor.canvas;

//stubs to it runs if CreateJs is not present
if (createjs) {
  FoundryBlazor.createjs = createjs;
  (FoundryBlazor.createjs.createShape = function () {
    return new createjs.Shape();
  }),
    (FoundryBlazor.createjs.createContainer = function () {
      return new createjs.Container();
    });
  FoundryBlazor.createjs.createText = function (t, f, c) {
    return new createjs.Text(t, f, c);
  };
  FoundryBlazor.createjs.createBitmap = function (u) {
    return new createjs.Bitmap(u);
  };
  FoundryBlazor.createjs.createStage = function (e, f) {
    return new createjs.Stage(e, f);
  };

  //extensions to text object to format text to fit max width;
  if (createjs.Text && createjs.Text.prototype) {
    var canonicalizeNewlines = function (str) {
      return str.replace(/(\r\n|\r|\n)/g, "\n");
    };

    createjs.Text.prototype.splitLine = function (st, n) {
      var b = "";
      var s = canonicalizeNewlines(st);
      while (s[0] == "\n") {
        s = s.substring(1);
      }

      while (s.length > n) {
        var c = s.substring(0, n);
        var d = c.lastIndexOf(" ");
        var e = c.lastIndexOf("\n");
        if (e != -1) d = e;
        if (d == -1) d = n;
        b += c.substring(0, d) + "\n";
        s = s.substring(d + 1);
      }
      return b + s;
    };

    createjs.Text.prototype.flowText = function (text, n, maxLines) {
      if (!text) return;
      var modifiedText = text ? this.splitLine(text, n) : text;
      var lines = modifiedText.split("\n");
      if (!maxLines || lines.length <= maxLines) {
        this.text = modifiedText;
        return modifiedText;
      }
      var total = maxLines - 1;
      var sublist = lines.splice(0, total - 1);
      var result = sublist.join("\n") + " " + lines[total] + " ...";
      this.text = result;
      return result;
    };
  }
} else {
  FoundryBlazor.createjs = {
    createShape: function () {
      return;
    },
    createContainer: function () {
      return;
    },
    createText: function () {
      return;
    },
    createBitmap: function () {
      return;
    },
    createStage: function () {
      return;
    },
    Tween: undefined,
    Touch: undefined,
    Ticker: undefined,
  };
}

//FoundryShape
(function (ns, fo, create, undefined) {
  var tween = create.Tween;
  var utils = fo.utils;

  ns.updateShapeForLayout = function (shape) {};

  var FoundryShape = function (properties, subcomponents, parent) {
    var shapeSpec = {
      isVisible: true,
      context: function () {
        //should be overriden by properties.. if not try to find in model
        if (!this.contextType) return;
        var model = fo.newInstance(this.contextType);
        return model;
      },
      geom: function () {
        return create.createShape();
      },
      update: function () {
        if (!this.geom) return false;
        this.geom.graphics.clear();
        return false;
      },
    };

    this.base = fo.Component;
    this.base(utils.union(shapeSpec, properties), subcomponents, parent);
    this.myType = (properties && properties.myType) || "FoundryShape";

    return this;
  };

  FoundryShape.prototype = (function () {
    var anonymous = function () {
      this.constructor = FoundryShape;
    };
    anonymous.prototype = fo.Component.prototype;
    return new anonymous();
  })();

  ns.FoundryShape = FoundryShape;
  utils.isaFoundryShape = function (obj) {
    return obj instanceof FoundryShape ? true : false;
  };

  FoundryShape.prototype.render = function (stage, context) {
    var property = this.getProperty("geom");
    if (property && this.update) {
      var geom = property.getValue();
      stage && stage.addChild && stage.addChild(geom);

      this.Subcomponents.forEach(function (subshape) {
        subshape.render(geom || stage, context);
      });
    }
  };

  FoundryShape.prototype.updateStage = function () {
    if (this.myParent) {
      this.render(this.myParent.geom);
    }
    return this;
  };

  FoundryShape.prototype.isPage = function () {
    return this == this.rootPage();
  };

  FoundryShape.prototype.outlineRef = function () {
    return "";
  };

  FoundryShape.prototype.isInGroup = function () {
    return this.myParent !== this.rootPage();
  };

  //http://blog.toggl.com/2013/05/6-performance-tips-for-html-canvas-and-createjs/

  FoundryShape.prototype.isShapeHit = function (gX, gY, w, h, skipSelected) {
    var geom = this.geom;
    if (!geom) return geom;

    var pt1 = geom.localToGlobal(0, 0);
    if (gX < pt1.x) return false;

    var pt2 = geom.localToGlobal(w, h);
    if (gX > pt2.x) return false;

    if (gY >= pt1.y && gY <= pt2.y) {
      return this;
    }

    //keep looking through children..
    //this could be faster if we skipped over the items known to be outside of gX & gY
    var elements = this.Subcomponents.elements;
    for (var i = 0; i < elements.length; i++) {
      var subShape = elements[i];
      if (skipSelected && subShape.isSelected) continue;

      var found = subShape.isShapeHit(
        gX,
        gY,
        subShape.width,
        subShape.height,
        skipSelected
      );
      if (found) return found;
    }
  };

  FoundryShape.prototype.subcomponentHitTest = function (gX, gY, skipSelected) {
    var elements = this.Subcomponents.elements;
    //for(var i=0; i<elements.length; i++ ){

    var start = elements.length - 1;
    for (var i = start; i >= 0; i--) {
      var subShape = elements[i];
      if (skipSelected && subShape.isSelected) continue;

      var found = subShape.isShapeHit(
        gX,
        gY,
        subShape.width,
        subShape.height,
        skipSelected
      );
      if (found) return found;
    }
  };

  FoundryShape.prototype.glueRemoveFromModel = function () {
    if (!this.Connections) return;

    this.Connections.map(function (item) {
      var shape = item.target;
      shape.removeFromModel();
    });
  };

  FoundryShape.prototype.glueTo = function (target) {
    var list = this.establishCollection("Connections", []); // you need to make this observable and dynamic

    var found = list.firstWhere(function (item) {
      return item.target == target;
    });

    if (!found) {
      found = {
        source: this,
        target: target,
      };
      list.push(found);
      //var str = target.stringify();
      //found.name = "{0} ({1}_{2})".format(list.count, this.name, target.name);
    }

    return found;
  };

  FoundryShape.prototype.unglueTo = function (target, deep) {
    var found;
    if (deep) {
      this.Subcomponents.map(function (shape) {
        found = shape.unglueTo(target, deep);
      });
    }

    var connections = this.Connections;

    if (!connections) return found;

    found = connections.firstWhere(function (item) {
      return item.target == target;
    });
    if (found) {
      connections.remove(found);
    }

    return found;
  };

  FoundryShape.prototype.glueShapeMoved = function (target, x, y, w, h, deep) {
    if (this.Connections) {
      this.Connections.map(function (item) {
        var shape = item.target;
        shape.glueShapeMoved(target, x, y, w, h);
      });
    }

    if (!deep) return;

    this.Subcomponents.map(function (shape) {
      shape.glueShapeMoved(shape, x, y + h, shape.width, shape.height, deep);
    });
  };
})(FoundryBlazor.canvas, Foundry, FoundryBlazor.createjs);

//Shape
(function (ns, fo, create, undefined) {
  var tween = create.Tween;
  var utils = fo.utils;

  var Shape = function (properties, subcomponents, parent) {
    var shapeSpec = {
      pinX: 0.0,
      pinY: 0.0,
      angle: 0.0,
      isSelected: false,
      isEditing: false,
      isActiveTarget: false,
      canGroupItems: function () {
        return true;
      },
      canBeGrouped: function () {
        return true;
      },
      showSubcomponents: true,
      showDetails: true,
    };

    this.base = ns.FoundryShape;
    this.base(utils.union(shapeSpec, properties), subcomponents, parent);
    this.myType = (properties && properties.myType) || "Shape";

    return this;
  };

  Shape.prototype = (function () {
    var anonymous = function () {
      this.constructor = Shape;
    };
    anonymous.prototype = ns.FoundryShape.prototype;
    return new anonymous();
  })();

  ns.Shape = Shape;
  utils.isaShape = function (obj) {
    return obj instanceof Shape ? true : false;
  };

  ns.makeShape = function (properties, subcomponents, parent) {
    var shape = new Shape(properties, subcomponents, parent);
    return shape;
  };

  Shape.prototype.setVisualState = function (state) {};

  Shape.prototype.angleInRads = function () {
    return (Math.PI / 180) * this.angle;
  };

  Shape.prototype.rootPage = function () {
    if (!this.myParent) {
      return;
    }
    return this.myParent.rootPage && this.myParent.rootPage();
  };

  Shape.prototype.outlineRef = function () {
    var depth = this.componentDepth();
    if (depth == 0 || !this.myParent) return "";
    var index = (this.myIndex() + 1).toString();
    if (this.myParent.componentDepth() == 0) return index;
    var root = this.myParent.outlineRef();

    var result = root ? root + "." + index : index;
    return result;
  };

  Shape.prototype.defaultPinX = function () {
    var page = this.rootPage();
    return page.defaultPinX();
  };

  Shape.prototype.defaultPinY = function () {
    var page = this.rootPage();
    return page.defaultPinY();
  };

  Shape.prototype.setDefaultDropLocation = function () {
    if (this.isInGroup() == false) {
      this.pinX = this.defaultPinX();
      this.pinY = this.defaultPinY();
    }
  };

  Shape.prototype.hasGroupMembers = function () {
    return this.Subcomponents.isNotEmpty();
  };

  Shape.prototype.hasDetails = function () {
    return this.context && this.context.isTextDifferent;
  };

  Shape.prototype.currentToggleSubcomponentsState = function () {
    if (!this.hasGroupMembers()) return "none";
    return this.showSubcomponents ? "minus" : "plus";
  };

  Shape.prototype.toggleShowSubcomponents = function () {
    this.showSubcomponents = !this.showSubcomponents;
    return this.showSubcomponents;
  };

  Shape.prototype.currentToggleDetailsState = function () {
    if (!this.hasDetails()) return "none";
    return this.showDetails ? "close" : "open";
  };

  Shape.prototype.toggleShowDetails = function () {
    this.showDetails = !this.showDetails;
    return this.showDetails;
  };

  //500, createjs.Ease.sineInOut);
  Shape.prototype.MorphGeomTo = function (rule, time, ease, onComplete) {
    var geom = this.geom;
    return this.MorphTo(geom, rule, time, ease, onComplete);
  };

  //500, createjs.Ease.sineInOut);
  Shape.prototype.MorphTo = function (geom, rule, time, ease, onComplete) {
    var page = this.rootPage();
    page
      ? page.MorphTo(geom, rule, time, ease, onComplete)
      : onComplete && onComplete();
  };

  Shape.prototype.repositionTo = function (pinX, pinY, angle, onComplete) {
    var self = this;
    //this.canTrace() && ns.trace.writeLog("repositioning", self.name, pinX, pinY, angle);
    this.MorphGeomTo(
      { x: pinX, y: pinY },
      500,
      create.Ease.sineInOut,
      function () {
        self.pinX = pinX;
        self.pinY = pinY;
        self.angle = angle ? angle : 0.0;
        self.glueShapeMoved(self, pinX, pinY, self.width, self.height, true);

        if (onComplete) onComplete();
        //ns.trace.writeLog("reposition Complete", self.name, self.pinX, self.pinY, self.angle);
      }
    );
  };
})(FoundryBlazor.canvas, Foundry, FoundryBlazor.createjs);

(function (ns, cv, undefined) {
  if (!ns.establishType) return;

  ns.establishType("Shape", {}, cv.makeShape);
})(Foundry, FoundryBlazor.canvas);
