# FatSorter

Reorder directory entries by moving files and folders into a temporary directory and back in case-insensitive alphabetical order, which is useful for FAT-based media used by embedded devices.

## Quickstart

1. Open the GitHub [Releases](https://github.com/paulczy/FatSorter/releases) page.
2. Download the zip file for your platform:
   `win-x86`, `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, or `osx-arm64`.
3. Extract the archive.
4. Run `FatSorter` against the mounted FAT volume you want to reorder.

Examples:

```text
FatSorter <directory> [--verbose] [--log-file <path>] [--yes]
```

`--yes` skips the confirmation prompt. Omit it if you want the tool to pause before making changes.

```powershell
# Windows
.\FatSorter.exe E:\ --yes
.\FatSorter.exe E:\ --verbose --log-file .\fat-sorter.log
```

```bash
# macOS
chmod +x ./FatSorter
./FatSorter /Volumes/SDCARD --yes
./FatSorter /Volumes/SDCARD --verbose --log-file fat-sorter.log

# Linux
chmod +x ./FatSorter
./FatSorter /mnt/sdcard --yes
./FatSorter /media/$USER/SDCARD --verbose --log-file fat-sorter.log
```

> **macOS Gatekeeper:** The first time you run FatSorter, macOS may block it because it is not signed. Go to **System Settings > Privacy & Security**, scroll down to the security section, and click **Allow Anyway** next to the FatSorter message. Then run the command again.

## Local Publish

Self-contained single-file builds are supported for:

- `win-x86`
- `win-x64`
- `win-arm64`
- `linux-x64`
- `linux-arm64`
- `osx-x64`
- `osx-arm64`

Run:

```powershell
.\build\publish.ps1
```

Artifacts are written to `.\publish\<rid>\`.

## Notes

- Run the tool against the actual FAT volume if you want the on-device file ordering to change.
- Dot-prefixed entries are skipped intentionally.
