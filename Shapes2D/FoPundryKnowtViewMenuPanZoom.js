(function (app, fo, undefined) {

    fo.defineType(app.defaultNS('menuPanZoom'), {
        //code to manage the scale of the drawing
        drawing: fo.fromParent,
        rootPage: fo.fromParent,
        doZoom1To1: function () {
            //var drawing = this.drawing;
            //drawing && drawing.doZoom1To1;

            var page = this.rootPage;
            if (page) page.zoom1To1(page.updatePIP);
        },
        doZoomToFit: function () {
            //var drawing = this.drawing;
            //drawing && drawing.doZoomToFit;

            var page = this.rootPage;
            if (page) page.zoomToFit(page.updatePIP);
        },
        //doZoomOut: function () {
        //    var drawing = this.drawing;
        //    drawing && drawing.doZoomOut;
        //},
        //doZoomIn: function () {
        //    var drawing = this.drawing;
        //    drawing && drawing.doZoomIn;
        //},
        doTogglePanZoomWindow: function () {
           // this.doRepaint;
            var drawing = this.drawing;
            drawing && drawing.doTogglePanZoomWindow;
            this.smashProperty('isPanZoomWindowOpen');
        },
        isPanZoomWindowOpen: function () {
            var drawing = this.drawing;
            return drawing ? drawing.isPanZoomWindowOpen : false;
        },
        zoomText: function () {
            var open = this.isPanZoomWindowOpen;
            return 'Zoom ' + (open ? 'off' : 'on');

            //return 'Zoom ' + (open ? 'close' : 'open');
        },
    },
    function (properties, subcomponents, parent) {

        var result = fo.makeAdaptor(properties);
        //subscribe to any do* of goto* messages...
        result.subscribeToCommands();



        return result;
    });

}(knowtApp, Foundry));