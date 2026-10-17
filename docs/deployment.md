# Deploying HELP-Board on PiTony

## Release boundary

This packages the current hello-world Blazor WebAssembly shell and ASP.NET Core
API together, not the completed reporting application. Google Workspace
OpenID Connect sign-in and authenticated API access are implemented, but must
be configured before sign-in is available. Authentik fallback and fine-grained
permissions are not implemented. Until real Google credentials and an approved
Workspace domain are configured and verified, do not connect this milestone to
real board data. Until Google sign-in has been configured and verified, keep a
restrictive Nginx Proxy Manager (NPM) Access List on the **entire** proxy host
and use a dedicated empty preview database on PiDB. After verification, the
welcome page at `/` is public; `/dashboard` and all application APIs require an
authenticated Workspace member.

The sole production target is PiTony running 64-bit Linux and Docker
(`linux/arm64`), with PostgreSQL remaining on PiDB and HTTPS terminated by NPM at
`https://help.becauseimclever.com`. No database or identity provider is included
in the application container. Like the existing BudgetExperiment deployment,
NPM forwards to PiTony's LAN IP and a published host port. HELP-Board uses
**2026:8080** instead of BudgetExperiment's **5099:8080**, avoiding a collision.
The API hosts the frontend in one running container, not separate UI/API
services. No shared NPM Docker network or changes to BudgetExperiment are needed.

## 1. Build and publish through GitHub Actions

The registry is **GitHub Container Registry (GHCR)**, not Docker Hub. The image
repository is `ghcr.io/fortinbra/help-board`.

1. Merge the container files and workflows into `main`, including the intended
   SDK and central dependency versions. The Dockerfile's SDK tag must match
   `global.json` when the pinned SDK changes.
2. Under repository **Settings > Actions > General**, allow these workflows and
   their pinned GitHub/Docker actions. The publishing job requests
   `contents: read` and `packages: write`; it uses the ephemeral `GITHUB_TOKEN`.
   No Docker Hub credentials or long-lived publishing PAT are needed.
3. Open **Actions > Publish container > Run workflow**, selecting the intended
   branch, or push a version tag such as `v0.1.0` at the intended commit.
   Creating a draft release does not trigger publishing: pushing its `v*` tag
   does. The manual-dispatch workflow must first exist on the default branch.
4. Wait for build/test validation, native AMD64 and ARM64 image builds, and
   container smoke checks to pass. Each platform publishes the same image it
   smoke-tested; a final job combines their digests into one multi-architecture
   tag, following the reference BudgetExperiment release pattern.
5. The final publishing job verifies the package visibility is **Private**
   before creating release tags. New GHCR packages default to private even when
   their source repository is public. If a package already exists, its existing
   visibility is not changed by the workflow; make it private before publishing
   sensitive artifacts. Keep it private and grant only intended readers.
   The source label links the package to this repository. If an existing package
   rejects publishing, grant this repository Actions write access in the
   package settings.
6. Use `ghcr.io/fortinbra/help-board:latest` for updates without editing the
   deployment configuration. Manual builds from the default branch (`main`)
   and stable `vMAJOR.MINOR.PATCH` tags update `latest`; feature-branch builds
   and prereleases do not. Record the multi-architecture
   `Digest: sha256:...` from the run summary for rollback or pinned deployments.
   Every run publishes `sha-<7-character-SHA>` and `sha-<full-commit-SHA>`;
   tag-triggered runs additionally publish the exact tag, for example `v0.1.0`.
   The per-platform images also retain `sha-<full-commit-SHA>-amd64` and
   `sha-<full-commit-SHA>-arm64` tags. Tags can move; use
   `ghcr.io/fortinbra/help-board@sha256:...` when pinning a deployment.

For example, a commit starting `f8475f4` can be pulled with:

```sh
docker pull ghcr.io/fortinbra/help-board:sha-f8475f4
```

Docker selects ARM64 on PiTony and AMD64 on a compatible development host. This
does not split the frontend and API or introduce another production target.

Validation runs on GitHub-hosted runners. Pull requests get no package-write
permission and no access to the private network. Publishing uses a native
`ubuntu-24.04-arm` runner for ARM64 and `ubuntu-latest` for AMD64; no self-hosted runner, SSH deployment secret, or access
to PiTony is required. This milestone intentionally leaves the production pull
and restart as an operator action. Production approvals, internal-agent
deployment, and automated rollback remain future Feature 006 work.

Configure branch protection on `main` to require the CI **Build and test** check,
an up-to-date branch, at least one review, and stale-review dismissal. The files
do not configure repository protection or package visibility automatically.

## 2. Prepare PiTony

Install a supported Docker Engine and Docker Compose v2. Confirm `docker info`
reports Linux/ARM64 (`aarch64` is the ARM64 architecture name on some installs).
Use a GitHub **personal access token (classic)** with only `read:packages` and
an account that has read access to the private package. Authorize organization
SSO if applicable. Do not use an Actions token as a permanent server credential.

On PiTony's Linux shell:

```sh
docker login ghcr.io --username YOUR_GITHUB_USERNAME
```

Enter the token at the password prompt, not in a command line or committed file.
Use a Docker credential helper where available; protect the Docker account's
configuration, which otherwise stores registry credentials in encoded form.

Copy [compose.yml](../deploy/compose.yml) and
[.env.example](../deploy/.env.example) to a restricted directory on PiTony,
for example `/opt/help-board`. These Linux commands run **on PiTony**, not in
Windows PowerShell:

```sh
cd /opt/help-board
umask 077
cp .env.example .env
chmod 600 .env
```

Edit `.env` locally on the server:

| Variable | Value |
|----------|-------|
| `HELPBOARD_IMAGE` | `ghcr.io/fortinbra/help-board:latest`, or a specific tag/digest for a pinned deployment. |
| `HELPBOARD_BIND_IP` | PiTony's LAN IPv4 address; the host port binds only to this interface. |
| `HELPBOARD_APP_PORT` | `2026`, the approved HELP-Board host port. |
| `HELPBOARD_PROXY_IP` | NPM's stable source IP as seen by the application after Docker forwarding/NAT. |
| `DB_CONNECTION_STRING` | PiDB connection string for a least-privilege, dedicated preview database user. Uses the same environment-file variable name as BudgetExperiment, mapped to HELP-Board's own configuration key. |
| `GOOGLE_OAUTH_CLIENT_ID` | OAuth 2.0 Web application client ID from the organization's Google Cloud project. |
| `GOOGLE_OAUTH_CLIENT_SECRET` | The matching client secret. Keep only in the restricted, ignored `.env` file; never commit or include it in an image. |
| `GOOGLE_WORKSPACE_DOMAIN` | The single Workspace domain whose verified members may sign in. |
| `GOOGLE_ADMIN_EMAILS` | Optional comma-separated Workspace email addresses assigned the initial Admin role. Other eligible users receive Viewer. |

The client ID is public OAuth configuration. Add it as `GOOGLE_OAUTH_CLIENT_ID`
in the ignored `.env`; store the matching client secret only as
`GOOGLE_OAUTH_CLIENT_SECRET` in that same protected file.

### Google Workspace OAuth setup

Create an OAuth 2.0 client of type **Web application** in the organization's
Google Cloud project. Configure this exact authorized redirect URI:

```text
https://help.becauseimclever.com/signin-google
```

For local HTTPS testing, add the exact localhost callback URI used by the
development profile as a separate redirect URI. Configure the OAuth consent
screen for the Workspace audience and have the Workspace administrator allow
the application if organizational OAuth controls require approval. HELP-Board
requests only the `openid`, `email`, and `profile` identity scopes; no Drive or
Gmail API access is needed.

Set the client ID, client secret, Workspace domain, and optional administrator
addresses in the private `.env` on the host, then recreate the container. The
domain check requires both Google's verified `hd` claim and the email address
to match the configured domain. A Google login outside that Workspace is
rejected. The client secret is injected only into the server-side API process.
The repository ignores `.env` files; do not add the populated file to version
control or share its contents.

The initial authorization model is intentionally small: all eligible members
are Viewers, with Admin assigned only to addresses in `GOOGLE_ADMIN_EMAILS`.
Roles are carried in the protected authentication cookie rather than stored in
an application user database. Sign out and sign in again after changing the
administrator list; sessions expire after eight hours. Authentik fallback and
database-backed provisioning/revocation remain future work.

Compose creates its own bridge network, like the existing application. Match
the current NPM LAN routing, but confirm the source address actually seen by the
application; Docker NAT may make it different from the proxy's configured
address. Do not use a wildcard trusted proxy or trust all forwarded headers.
Allow access to TCP 2026 only from NPM and approved operators using firewall
rules that cover Docker-published traffic (for example the `DOCKER-USER` chain);
ordinary UFW rules alone may be bypassed by Docker forwarding. Do not forward
2026 from the Internet. Direct port access otherwise bypasses the NPM Access List.

The application accepts `X-Forwarded-For` and `X-Forwarded-Proto` from the
configured proxy IP (and framework-default loopback proxies), with the default
one-hop limit. It retains the original Host header rather than trusting
`X-Forwarded-Host`. Configure NPM to preserve the canonical Host and send
`X-Forwarded-Proto: https`. A changed proxy IP must be updated in `.env` before
recreating the application.

Use PiDB's actual DNS address and firewall rules, not `localhost` (which means
the application container). Configure TLS according to PiDB's certificate setup;
prefer `SSL Mode=VerifyFull` with a trusted server certificate. Do not disable
certificate validation as a workaround. The example pool/timeout settings cap
the pool at 10 connections, start with zero connections, and bound waits.
Validate them against PiDB capacity. Environment variables are server-side but
visible to Docker administrators; restrict host/Docker access and keep `.env`
out of source control and backups without appropriate encryption.

## 3. Configure Nginx Proxy Manager

Create or update the proxy host:

- Domain: `help.becauseimclever.com`.
- Forward scheme: `http`.
- Forward hostname/IP: PiTony's LAN IP (same target host as BudgetExperiment).
- Forward port: `2026` (not BudgetExperiment's `5099`).
- Preserve the original Host header and HTTPS forwarding information.
- Obtain/configure a valid TLS certificate and enable **Force SSL**.
- Apply an Access List limiting the entire site to approved operators during
  this unauthenticated milestone. Confirm it covers `/api/` as well as `/`.

DNS must resolve to the existing NPM ingress. Publish only the LAN-bound host
port above; do not add a second TLS endpoint to the application container. Docker liveness
probes use HTTP internally before HTTPS redirection; normal proxied requests
use the trusted forwarded scheme.

## 4. Start and verify

```sh
cd /opt/help-board
docker compose config --quiet
docker compose pull
docker compose up -d --wait --wait-timeout 120
docker compose ps
docker compose logs --tail 100 app
docker stats --no-stream
```

From an approved operator host, check the LAN-bound probe while preserving an
allowed Host header:

```sh
curl --fail -H 'Host: help.becauseimclever.com' http://PITONY_LAN_IP:2026/healthz
```

Direct application-page requests are not the supported browsing path: host
filtering and HTTPS redirection expect the public hostname and trusted proxy
headers. Browse through NPM for the normal application flow.

`docker compose config --quiet` validates without printing interpolated
credentials. Avoid sharing the output of `docker inspect` or plain
`docker compose config`, which can include secrets.

The application runs as the .NET image's non-root user, with a read-only root
filesystem, a 64 MiB temporary filesystem, dropped capabilities, bounded logs,
one CPU, a 512 MiB memory ceiling, and a 128-process limit. These are initial
operational limits, **not measured production capacity guarantees**. Measure
startup, interactive WASM load, idle use, and representative load on PiTony
alongside its other services before accepting production operation.

Acceptance checks:

1. Compose reports `healthy` within 120 seconds. `/healthz` returns HTTP 200
   `Healthy`. This is **liveness only**: it does not connect to PostgreSQL,
   validate migrations, check authentication, or prove external availability.
2. From an approved client, open `https://help.becauseimclever.com` and confirm
   the shell finishes loading, the logo/theme controls render, and the browser
   has no missing WASM/framework/CSS/JS assets or redirect loops.
3. Open a non-file deep link, such as `/deployment/deep-link`, directly and
   confirm the Blazor shell loads (the client may show its Not Found page).
4. Confirm the NPM Access List denies unapproved clients access to both the
   shell and `/api/tickets`. Private registry visibility does not protect a
   running website. Also verify firewall rules deny those clients direct access
   to PiTony's published TCP 2026 port.
5. Validate DNS/TLS and confirm HTTP ingress redirects to the canonical HTTPS
   host. Check application logs for unknown-proxy warnings.
6. Separately confirm PiDB reachability and least-privilege access from the
   container network with approved database tooling. Do not treat a green
   liveness probe as a database check or log database credentials.
7. Record the commit, image digest, operator, deployment time, limits, and
   validation results. Recheck limits with `docker stats --no-stream`.

Authentication redirect/authorization checks are a release blocker for a
board-data release, not a passing check for this initial shell.

### Database migrations and persistent data

`FeatureManagement__DatabaseMigrationsApplyAtStartup` is explicitly `false`.
The pipeline and container never apply production migrations. No application
data is stored in the container, so replacing it does not move or delete PiDB
data. Review migration history, take and verify a PiDB backup, and approve/apply
schema changes separately using the repository's
[persistence guidance](features/feature-009-postgres-efcore-integration.md).
The current shell and liveness probe do not require database tables.

## 5. Update, restart, and roll back

Before an update, record the previous known-good digest and its compatible
schema/backup. With `HELPBOARD_IMAGE=ghcr.io/fortinbra/help-board:latest`,
leave the configuration unchanged and run:

```sh
docker compose pull
docker compose up -d --wait --wait-timeout 120
docker compose logs --tail 100 app
```

For a pinned deployment, edit `HELPBOARD_IMAGE` to the approved tag or digest
before these commands. Publishing `latest` does not automatically restart
PiTony's container; the pull/up commands apply the update.

Repeat the acceptance checks. This is a single-container deployment and can
have a short interruption. `unless-stopped` restarts the container after a
process/host restart; Docker's `unhealthy` state alone does **not** trigger an
automatic restart or rollback. Investigate with logs and resource measurements;
use `docker compose restart app` for an intentional process restart.

If startup fails, health stays unhealthy after 120 seconds, assets fail to
load, or routing/access checks fail, stop promotion and manually restore
`HELPBOARD_IMAGE` to the prior digest. Run the same pull/up commands and
acceptance checks. Retain previous images/packages until rollback is no longer
needed. Never assume an image rollback reverses a schema migration or restores
data; use the separately reviewed database recovery plan if required. Do not
run destructive schema rollback just to make an older image start.

## Local build verification

From the repository root in Windows PowerShell:

```powershell
dotnet test HelpBoard.slnx --configuration Release
node --test tests\HelpBoard.Web.Tests\theme.test.cjs
docker buildx build --platform linux/arm64 --load --tag help-board:local .
```

Running the ARM64 image locally requires ARM64 execution/emulation support.
GitHub's publishing workflow runs the smoke test natively on ARM64.

The Dockerfile publishes the API and also publishes the WebAssembly client
directly, then overlays the client's `wwwroot` into the host image. This ensures
the client's standalone HTML placeholder/import-map processing is retained;
the nested API publish alone leaves the source HTML placeholder unresolved with
the current SDK. Both outputs remain one application image. The release smoke
test rejects unresolved placeholders and checks an actual published WASM
response, not just an HTTP 200 that could be an HTML fallback.
