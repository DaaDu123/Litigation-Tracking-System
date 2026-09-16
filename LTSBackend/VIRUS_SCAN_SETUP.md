# Virus Scan Setup (ClamAV)

Every file upload (profile pictures + case documents) is now scanned for
malware before being written to disk — see
`Services/VirusScan/ClamAvVirusScanService.cs`. This talks to a **clamd**
daemon (the ClamAV scanning engine) over a plain TCP socket. No NuGet
package is required, but **clamd itself must be running and reachable**
from the app server, or every upload will fail (`VirusScan:FailClosed`
defaults to `true` — uploads are rejected, not silently allowed through,
when the scanner can't be reached).

## Configuration (`appsettings.json`)

```json
"VirusScan": {
  "Enabled": true,
  "FailClosed": true,
  "ClamAvHost": "localhost",
  "ClamAvPort": 3310,
  "TimeoutSeconds": 30
}
```

| Setting | Meaning |
|---|---|
| `Enabled` | `false` disables scanning entirely (uploads go through unscanned). Only ever set this for local dev without clamd installed — **never in production.** |
| `FailClosed` | `true` (default, recommended) = reject the upload if the scanner can't be reached. `false` = allow the upload through anyway, just log a warning. |
| `ClamAvHost` / `ClamAvPort` | Where clamd is listening. `3310` is ClamAV's default. |
| `TimeoutSeconds` | Max time to wait for a scan before treating it as failed. |

## Installing clamd

**Option A — Docker (simplest, works on any host OS):**
```bash
docker run -d --name clamav -p 3310:3310 clamav/clamav
```
Set `ClamAvHost` to the Docker host's address (`localhost` if the app
runs on the same machine, or the container's network alias if both run
in Docker Compose).

**Option B — Linux (Ubuntu/Debian), same machine as the app:**
```bash
sudo apt-get update
sudo apt-get install -y clamav-daemon
sudo freshclam                      # download the latest virus definitions
sudo systemctl enable --now clamav-daemon
```
Confirm it's listening:
```bash
sudo ss -tlnp | grep 3310
```
If clamd is configured for a Unix socket instead of TCP by default,
edit `/etc/clamav/clamd.conf` and make sure this line is present (and
`LocalSocket` is commented out):
```
TCPSocket 3310
TCPAddr 127.0.0.1
```
Then `sudo systemctl restart clamav-daemon`.

**Option C — Windows:** clamd is not officially supported on Windows.
Recommended: run clamd via Docker Desktop (Option A), or on a small
Linux VM/container reachable from the app server, and point
`ClamAvHost`/`ClamAvPort` at it. Don't try to run clamd natively on the
same Windows box as the app in production.

## Keeping virus definitions current

ClamAV is only as good as its signature database. `freshclam` should run
on a schedule (it does automatically via `clamav-freshclam` on Linux
package installs, and the official Docker image updates on container
start — for a long-running container, also schedule a periodic
`docker exec clamav freshclam` or restart policy).

## Testing it actually works

Use the EICAR test file — an industry-standard, harmless string that
every AV engine (including ClamAV) is designed to flag as a "virus" for
exactly this purpose. Save this as `eicar.txt` and try uploading it
through the app; it should be rejected with a "malware ... FOUND"
message in the API response and in the backend logs:

```
X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*
```

If a clean PDF/DOCX/image uploads successfully and the EICAR string is
rejected, the integration is working end-to-end.

## Sizing note

ClamAV's default `StreamMaxLength` in `clamd.conf` is 25MB, while this
app's `UploadDocumentValidator` allows files up to 50MB. If you expect
uploads over 25MB, raise `StreamMaxLength` in `clamd.conf` to match
(e.g. `StreamMaxLength 50M`) and restart clamd — otherwise larger
uploads will fail the scan with an "INSTREAM size limit exceeded" error
even for clean files.
