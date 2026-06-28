# Imposter Game

A multiplayer web-based social deduction game built with **.NET 9**, **Blazor Server**, **SignalR**, and **WebRTC**.

Players can create or join a room, receive hidden roles, chat in real time, vote for the suspected imposter, and use camera/microphone features during the game.

## Features

- Create and join rooms using a room code
- Random imposter selection
- Secret word shown only to non-imposter players
- Host-only controls
  - Start game
  - Start voting
  - Reveal result
  - Play again
- Player voting system
- Voted indicator next to players
- Real-time chat
- Press Enter to send chat messages
- Auto-scroll chat to latest message
- Camera and microphone integration
- Separate camera and microphone controls
- WebRTC-based video/audio prototype
- SignalR-based real-time communication/signaling
- Responsive UI
- Docker support
- Render deployment support

## Tech Stack

- C#
- .NET 9
- Blazor Server
- Razor Components
- SignalR
- WebRTC
- JavaScript Interop
- HTML/CSS
- Docker
- GitHub
- Render

## How It Works

1. A user creates a room and becomes the host.
2. Other players join using the room code.
3. The host starts the game.
4. One player is randomly selected as the imposter.
5. Non-imposter players see the secret word.
6. The imposter does not see the secret word.
7. Players discuss, chat, and use camera/mic if enabled.
8. The host starts voting.
9. Players vote for who they think is the imposter.
10. The host reveals the result.

## WebRTC Usage

WebRTC is used for browser-based camera and microphone communication.

The app uses:

- `navigator.mediaDevices.getUserMedia()` to request camera and microphone access
- `RTCPeerConnection` to establish browser-to-browser media connections
- SignalR as the signaling layer to exchange:
  - Offers
  - Answers
  - ICE candidates

SignalR is used only to help browsers establish the WebRTC connection. Once the connection is established, audio and video are handled by WebRTC.

## Run Locally

Make sure you have the .NET 9 SDK installed.

```bash
dotnet restore
dotnet run
