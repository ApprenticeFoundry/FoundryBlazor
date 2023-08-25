class Browser {
    getWindowDimensions() {
        return {
            innerWidth: window.innerWidth,
            innerHeight: window.innerHeight,
        };
    }
    canvasPNGBase64(id = 'canvasHolder') {
        const containerNode = document.getElementById(id);
        let canvasNode = null;
        if (Boolean(containerNode)) canvasNode = containerNode.getElementsByTagName('canvas').item(0);

        if (Boolean(canvasNode)) {
            console.log('canvasNode=', canvasNode);
            return canvasNode.toDataURL();
        }
    }
}
window.Browser = new Browser();
