window.imposterVideo = {
    connection: null,
    localStream: null,
    peers: {},
    names: {},
    localVideoId: null,
    videoGridId: null,

    start: async function (roomCode, playerId, playerName, localVideoId, videoGridId) {
        this.localVideoId = localVideoId;
        this.videoGridId = videoGridId;

        if (!window.signalR) {
            throw new Error("SignalR JavaScript client was not loaded.");
        }

        const localVideo = document.getElementById(localVideoId);
        if (!localVideo) {
            throw new Error("Local video element not found.");
        }

        this.localStream = await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: true
        });

        localVideo.srcObject = this.localStream;

        const status = document.getElementById("localMediaStatus");
        if (status) status.innerText = "Camera + mic on";

        if (!this.connection) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/videohub")
                .withAutomaticReconnect()
                .build();

            this.connection.on("ExistingUsers", async (users) => {
                for (const user of users) {
                    await this.createPeer(user.connectionId, user.playerName, true);
                }
            });

            this.connection.on("UserJoined", async (user) => {
                this.names[user.connectionId] = user.playerName;
            });

            this.connection.on("ReceiveOffer", async (fromConnectionId, fromPlayerName, offer) => {
                const pc = await this.createPeer(fromConnectionId, fromPlayerName, false);
                await pc.setRemoteDescription(new RTCSessionDescription(offer));
                const answer = await pc.createAnswer();
                await pc.setLocalDescription(answer);
                await this.connection.invoke("SendAnswer", fromConnectionId, answer);
            });

            this.connection.on("ReceiveAnswer", async (fromConnectionId, answer) => {
                const pc = this.peers[fromConnectionId];
                if (pc) {
                    await pc.setRemoteDescription(new RTCSessionDescription(answer));
                }
            });

            this.connection.on("ReceiveIceCandidate", async (fromConnectionId, candidate) => {
                const pc = this.peers[fromConnectionId];
                if (pc && candidate) {
                    await pc.addIceCandidate(new RTCIceCandidate(candidate));
                }
            });

            this.connection.on("UserLeft", (connectionId) => {
                this.removePeer(connectionId);
            });

            await this.connection.start();
        }

        await this.connection.invoke("JoinVideoRoom", roomCode, playerId, playerName);
    },

    createPeer: async function (connectionId, playerName, makeOffer) {
        if (this.peers[connectionId]) {
            return this.peers[connectionId];
        }

        this.names[connectionId] = playerName || "Player";

        const pc = new RTCPeerConnection({
            iceServers: [
                { urls: "stun:stun.l.google.com:19302" },
                { urls: "stun:stun1.l.google.com:19302" }
            ]
        });

        this.peers[connectionId] = pc;

        if (this.localStream) {
            for (const track of this.localStream.getTracks()) {
                pc.addTrack(track, this.localStream);
            }
        }

        pc.onicecandidate = async (event) => {
            if (event.candidate && this.connection) {
                await this.connection.invoke("SendIceCandidate", connectionId, event.candidate);
            }
        };

        pc.ontrack = (event) => {
            this.addRemoteVideo(connectionId, this.names[connectionId], event.streams[0]);
        };

        pc.onconnectionstatechange = () => {
            if (["failed", "closed", "disconnected"].includes(pc.connectionState)) {
                this.removePeer(connectionId);
            }
        };

        if (makeOffer) {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await this.connection.invoke("SendOffer", connectionId, offer);
        }

        return pc;
    },

    addRemoteVideo: function (connectionId, playerName, stream) {
        const grid = document.getElementById(this.videoGridId);
        if (!grid) return;

        let tile = document.getElementById("tile-" + connectionId);

        if (!tile) {
            tile = document.createElement("div");
            tile.id = "tile-" + connectionId;
            tile.className = "remote-tile";

            const video = document.createElement("video");
            video.id = "video-" + connectionId;
            video.autoplay = true;
            video.playsInline = true;
            video.className = "video-box";

            const footer = document.createElement("div");
            footer.className = "tile-footer";
            footer.innerHTML = `<b>${this.escapeHtml(playerName || "Player")}</b><span>📹 🎤</span>`;

            tile.appendChild(video);
            tile.appendChild(footer);
            grid.appendChild(tile);
        }

        const videoElement = document.getElementById("video-" + connectionId);
        if (videoElement && videoElement.srcObject !== stream) {
            videoElement.srcObject = stream;
        }
    },

    removePeer: function (connectionId) {
        const pc = this.peers[connectionId];
        if (pc) {
            pc.close();
            delete this.peers[connectionId];
        }

        const tile = document.getElementById("tile-" + connectionId);
        if (tile) tile.remove();
    },

    stop: async function () {
        for (const connectionId of Object.keys(this.peers)) {
            this.removePeer(connectionId);
        }

        if (this.localStream) {
            this.localStream.getTracks().forEach(track => track.stop());
            this.localStream = null;
        }

        const localVideo = document.getElementById(this.localVideoId);
        if (localVideo) localVideo.srcObject = null;

        const status = document.getElementById("localMediaStatus");
        if (status) status.innerText = "Off";

        if (this.connection) {
            try {
                await this.connection.stop();
            } catch {
            }
            this.connection = null;
        }
    },

    escapeHtml: function (value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }
};
