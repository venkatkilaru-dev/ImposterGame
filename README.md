# ImposterGameFinal

Features:
- Real multi-user video/audio using WebRTC
- SignalR signaling hub
- Enter key sends chat
- Host-only start voting, reveal result, and play again
- Camera and mic turn on together
- Dockerfile included for Render

Run:

```bat
cd /d D:\dotnet\ImposterGameFinal
dotnet restore
dotnet run
```

Open:

```text
http://localhost:5252
```

Testing:
- Use 3 tabs or 3 devices.
- Every player must click **Turn on camera + mic**.
- On localhost, camera works in modern browsers.
- On Render, HTTPS is required and Render provides HTTPS.
