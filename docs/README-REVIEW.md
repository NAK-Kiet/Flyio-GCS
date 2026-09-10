# Fly.io GCS documentation review

## Drafts

- [Full GitHub README](../README.md): proposed public/product presentation.
- [Internal GitLab README](../README.gitlab.md): standalone draft intended to replace the root README only in the internal GitLab presentation after approval. It has no hero or screenshot gallery.
- [Animated header](assets/flyio-drone-header.gif).
- [Decoded header preview](assets/flyio-drone-header-preview.png).

No remote README has been changed. Keeping two local drafts does not automatically give GitHub and GitLab different root READMEs. Publication strategy and any branch changes remain outside this review.

## Verified checkout

The requested `C:\Development\Flyio-GCS` path was absent. Work used the available `C:\Flyio-GCS` checkout.

- Branch: `main`.
- HEAD: `c3dfd7159eb4d87b782232ac58305975a321674d`.
- Remotes: `origin` is the requested NAK-Kiet/Flyio-GCS GitHub repository; `gitlab` is the user-provided private server; `upstream` is ArduPilot/MissionPlanner.
- Initial tracked diff: empty.
- Existing untracked files: `flyio-start.err`, `flyio-start.out`; left untouched.
- No root or relevant nested AGENTS.md was found.
- No root CHANGELOG.md or docs directory existed before this work.
- All directories in the proposed repository tree were verified.
- MissionPlanner.csproj confirms net472, Fly.io-GCS, and the historical Release/net461 output path.
- Application build/run and real vehicle validation were not performed for this documentation task.

## Original README assessment

The previous README included application pages/features, user/developer startup, Windows packaging, vehicle connection, PX4/QGroundControl context, architecture, project structure, troubleshooting, release validation, DRDC, repository links, licensing, and a screenshot placeholder. It repeated packaging instructions, had a broken update-command fence, lacked the explicit NuGet config argument, and contained no visual header or approved screenshot gallery.

## Existing asset inventory

| Category | Existing paths | Tracking / decision |
| --- | --- | --- |
| Primary Fly.io logo | `Resources/Fly.io icon.png` | Tracked; selected for top branding |
| Fly.io variants | `Resources/Fly.io icon v2.png`, `Resources/Fly.io taskbar icon.png`, `Resources/Flyio.ico` | Tracked; not selected for top branding |
| Racing wordmark | `Resources/Flyio-Racing-Drone-Warrior-Logo.png` | Tracked; available, not a standalone drone illustration |
| HELP Fly.io partner logo | `Resources/CommunityPartners/Flyio.png` | Tracked; selected in community table |
| DRDC | `Resources/CommunityPartners/Drdc.png`, `Resources/Drdc icon.png` | Tracked; CommunityPartners variant selected for branding and integration |
| Fly.io navigation art | `Assets/FlyioNav/{data,plan,setup,config,simulation,help,drdc}.png` | All tracked; navigation illustrations, not interface screenshots |
| Header drone source | `Assets/FlyioNav/simulation.png` | Tracked; crop (480,64)-(606,186) isolates the existing white drone |
| Alternative drone | `Resources/quad2.png` | Tracked; low-resolution inherited icon, not selected |
| Other inherited vehicle imagery | `Resources/quadframes*`, `Resources/quadhover*`, `Resources/plane2.png`, `Resources/rover.png` | Available; not selected for public branding |
| Interface captures | `artifacts/visual-review/` | Ignored, untracked review artifacts; none selected |

HELP's `BuildCommunityPartnersSection` uses these ten existing tracked files, all reused without recreating logos:

- `Resources/CommunityPartners/Aerobyte.png`
- `Resources/CommunityPartners/Aerospace.png`
- `Resources/CommunityPartners/Dev.png`
- `Resources/CommunityPartners/Drdc.png`
- `Resources/CommunityPartners/Electronic.png`
- `Resources/CommunityPartners/Factoryio.png`
- `Resources/CommunityPartners/FactoryX.png`
- `Resources/CommunityPartners/Flyio.png`
- `Resources/CommunityPartners/Maker.png`
- `Resources/CommunityPartners/Racingdrone.png`

The presentation distinguishes the HELP community from open-source acknowledgements and does not assert direct contribution by every displayed organization.

## Interface capture decision

| Page | Available local examples | Selected |
| --- | --- | --- |
| DATA | `DATA.png`, `package-DATA.png`, `final-data-no-px4.png` | None; approved current capture needed |
| PLAN | `PLAN.png`, `package-PLAN.png`, `final-plan.png` | None; approved current capture needed |
| SETUP | `SETUP.png`, `package-SETUP.png`, `final-SETUP.png` | None; approved current capture needed |
| CONFIG | `CONFIG.png`, `package-CONFIG.png`, `final-CONFIG.png` | None; approved current capture needed |
| SIMULATION | `SIMULATION.png`, `package-SIMULATION.png`, `final-simulation.png` | None; approved current capture needed |
| HELP | `HELP.png`, `package-HELP.png`, `final-help.png` | None; approved current capture needed |
| DRDC | `DRDC.png`, `package-DRDC.png`, `final-drdc.png` | None; approved current capture needed |

All paths above are under `artifacts/visual-review/`. The pre-existing README explicitly withheld these from product documentation pending approval. A visual spot-check of `package-DATA.png` showed the old PX4 test panel and unavailable telemetry; `portable-final2-help-small.png` showed a terminal instead of HELP. Filenames alone are not evidence of approval or correct captures. The new public README lists the missing approved screenshots rather than using poor or unapproved images.

## New documentation assets

| Path | Purpose |
| --- | --- |
| `docs/assets/flyio-drone-header.gif` | GitHub-only animated header; 531,460 bytes (519 KiB), 960 Ã— 240, 219 optimized encoded frames, 12 seconds, infinite loop |
| `docs/assets/flyio-drone-header-preview.png` | Decoded GIF frame for visual review |
| `docs/assets/build-header.py` | Reproducible documentation asset generator using Pillow and existing artwork |

No AI-generated artwork was used. Animation moves left to right at approximately 95 pixels/second, with gentle vertical motion. Off-screen identical frames are combined by GIF optimization. The background and branding stay still; there is no flashing. The GIF can be regenerated with Python/Pillow; the generator is not part of the application build.

## Selected badges

Static Shields.io badges, with no implied live CI, release, or hardware test status:

- Top row: Windows native/recommended, target net472, product Fly.io GCS.
- Platforms: Windows 10/11 native; Linux and macOS source workflows only.
- Core: C#, .NET Framework 4.7.2, WinForms, MAVLink.
- Tools: Visual Studio, VS Code editor, PowerShell, Git.
- Collaboration: GitHub, internal GitLab.
- Ecosystem: ArduPilot, PX4 development context, QGroundControl separate GCS.

The technology table is collapsed to limit visual clutter. No unrelated technology badges were added.

## Included links

Public README external destinations:

- `https://github.com/NAK-Kiet/Flyio-GCS.git` â€” clone command.
- `https://github.com/NAK-Kiet/Flyio-GCS/issues` â€” non-sensitive reports.
- `https://drdc-dev-web.drsys.asia/` â€” HELP's DRDC website.
- `https://drdt-dev-gcs-frontend.drsys.asia/en/login` â€” dashboard sign-in confirmed in both HELP and DRDCWebView source; remote availability unverified.
- `https://github.com/ArduPilot/MissionPlanner` â€” upstream attribution.
- `https://github.com/ArduPilot/MissionPlanner/graphs/contributors` â€” upstream contributors.
- `https://ardupilot.org/ardupilot/` â€” ecosystem acknowledgement.
- `https://img.shields.io/badge/...` â€” descriptive badge images.

The web tool could not open the two DRDC destinations, so neither is represented as independently availability-tested. No authenticated access was attempted.

Local links include all selected image paths, `COPYING.txt`, `NOTICE-Fly.io-GCS.txt`, and `run-scripts/README.md`, plus README section anchors. Internal clone instructions appear only in the GitLab draft, with network, repository permission, and SSH public-key prerequisites. No secrets are included.

## Verification and change scope

- README local links: all resolve.
- Code fences: balanced in both drafts.
- GIF: decoded successfully; infinite loop metadata and total 12-second duration verified; decoded preview visually inspected.
- `git diff --check`: passed.
- `git diff --stat`: README.md, 278 insertions and 256 deletions. Untracked new files are not included by this command.
- `COPYING.txt` and `NOTICE-Fly.io-GCS.txt`: unchanged.

Final expected short status:

```text
 M README.md
?? README.gitlab.md
?? docs/
?? flyio-start.err
?? flyio-start.out
```

Application source changes: NONE
UI changes: NONE
PX4 changes: NONE
MAVLink/backend changes: NONE
Build configuration changes: NONE
Commit: NONE
GitHub push: NONE
GitLab push: NONE

GitHub README prepared for visual/product review.
GitLab README prepared for internal engineering review.
No documentation published yet.
