window.imposterMedia = {
    stream: null,

    startLocalVideo: async function (videoId) {
        const video = document.getElementById(videoId);

        if (!video) {
            console.error("Video element not found:", videoId);
            return;
        }

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("Camera/mic is not supported in this browser.");
            return;
        }

        this.stream = await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: true
        });

        video.srcObject = this.stream;
    },

    stopLocalVideo: function (videoId) {
        const video = document.getElementById(videoId);

        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        if (video) {
            video.srcObject = null;
        }
    }
};
