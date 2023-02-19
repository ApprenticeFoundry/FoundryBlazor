class VideoManager {
    id = 'DEFAULT_ID';

    _showAlert() {
        const msg = `No video node with id=${this.id} is available.`;
        window.alert(msg);
    }

    play(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.play();
        } else {
            this._showAlert();
        }
    }

    pause(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.pause();
        } else {
            this._showAlert();
        }
    }

    restart(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.pause();
            node.currentTime = 0;
            this.play(id);
        } else {
            this._showAlert();
        }
    }
}
window.VideoManager = new VideoManager();
