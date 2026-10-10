# Profile picture uploads on deployed environments

## Why local can work while deployment fails

1. `VirusScan:Enabled` defaults to `true`. Production must be able to reach ClamAV at `VirusScan:ClamAvHost:VirusScan:ClamAvPort`. If the deployed server/container has no ClamAV listener, uploads are rejected when `VirusScan:FailClosed=true` (the secure default).
2. A container/app filesystem may be temporary or read-only. Uploads saved only under the app's `wwwroot` may disappear on redeploy or fail due to permissions.

## Required deployment settings

Configure environment variables for the deployed backend (environment variables override appsettings):

- `VirusScan__Enabled=true`
- `VirusScan__FailClosed=true`
- `VirusScan__ClamAvHost=<reachable ClamAV hostname or service name>`
- `VirusScan__ClamAvPort=3310`
- `FileStorage__PublicRoot=<absolute path on persistent writable storage>`

Example Linux mount/path: `/var/lib/lts/public`. Mount a persistent volume there and ensure the API process has read/write permission. The application will store profile pictures under `<path>/uploads/profile_pictures` and serve them under `/uploads/profile_pictures/...`. Do not use an ephemeral container layer if images must survive restarts/redeployments.

If the backend and ClamAV run in separate Docker containers, do not use `localhost` for ClamAV; use the ClamAV service/container DNS name. If hosted directly on a VM, verify the ClamAV daemon is running and listening on the configured host/port.

Do **not** set `VirusScan__Enabled=false` as a production workaround unless you have deliberately accepted the security risk and replaced scanning with an equivalent control.

## Verification

1. Deploy with the environment variables above and a persistent writable mount.
2. Check backend logs during upload for `Could not reach ClamAV daemon`, `Failed to save file`, or `New profile image saved`.
3. After upload, confirm the response stores a path like `/uploads/profile_pictures/<guid>.png`.
4. Open that URL on the backend domain and verify it returns the image.
5. Restart/redeploy and confirm the same URL still works.

The source changes use the configured persistent root for both saving/deleting images and serving `/uploads`. Existing local development continues to use `wwwroot` when `FileStorage:PublicRoot` is empty.
