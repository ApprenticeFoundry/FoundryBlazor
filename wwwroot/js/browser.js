class Browser {
    getWindowDimensions() {
        return {
            innerWidth: window.innerWidth,
            innerHeight: window.innerHeight,
        };
    }
}
window.Browser = new Browser();
