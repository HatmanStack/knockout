# Git LFS Setup

## Overview

This repository uses Git Large File Storage (LFS) to efficiently manage binary assets such as 3D models, audio files, and textures.

## Installation Status

✅ **Git LFS Installed**: Version 3.3.0 (Debian package)
✅ **Repository Initialized**: `git lfs install` completed
✅ **Tracking Configured**: `.gitattributes` contains all necessary patterns

## Tracked File Types

The following file types are automatically tracked by Git LFS:

### 3D Assets
- `*.fbx` - 3D models and animations
- `*.blend` - Blender source files

### Audio
- `*.wav` - Uncompressed audio
- `*.mp3` - Compressed audio

### Images
- `*.png` - PNG images
- `*.jpg` - JPEG images
- `*.psd` - Photoshop files
- `*.tga` - Targa images
- `*.tif` - TIFF images
- `*.hdr` - HDR images
- `*.exr` - OpenEXR images
- `*.cubemap` - Cubemap textures

### Video
- `*.mp4` - Video files

### Archives & Binaries
- `*.zip` - Compressed archives
- `*.rar` - RAR archives
- `*.unitypackage` - Unity packages
- `*.dll` - Dynamic libraries
- `*.exe` - Executables
- `*.dylib` - macOS libraries
- `*.apk` - Android packages
- `*.a` - Static libraries

## Current Asset Inventory

Large asset directories in this repository:
- `Assets/AAAnimators_Boxing_basic1.1/` - 168MB (boxing animations)
- `Assets/Asset Unlock - 3D Prototyping/` - 24MB (character animations)

**Total Binary Assets**: ~192MB

## Setup for New Contributors

If you're cloning this repository for the first time:

### 1. Install Git LFS

**Debian/Ubuntu:**
```bash
sudo apt-get install git-lfs
```

**macOS:**
```bash
brew install git-lfs
```

**Windows:**
Download from [git-lfs.github.com](https://git-lfs.github.com/)

### 2. Initialize LFS
```bash
git lfs install
```

### 3. Clone Repository
```bash
git clone https://github.com/HatmanStack/knockout.git
cd knockout
```

Git LFS will automatically download the binary assets during clone.

## Verifying LFS is Working

Check which files are tracked by LFS:
```bash
git lfs ls-files
```

Check LFS status:
```bash
git lfs status
```

## Adding New Binary Assets

When adding new binary files that match the patterns in `.gitattributes`, they will automatically be tracked by LFS. Just use normal git commands:

```bash
git add Assets/NewModel.fbx
git commit -m "feat(assets): add new character model"
```

Git LFS will handle the rest automatically.

## Troubleshooting

### Assets Not Using LFS

If you added files before LFS was set up, they may be stored as regular git objects. To migrate them:

```bash
git lfs migrate import --include="*.fbx,*.wav,*.mp3" --everything
```

**Warning**: This rewrites git history and requires force push if the branch has been published.

### Missing LFS Objects

If you see pointer files instead of actual content:

```bash
git lfs pull
```

### Checking File Storage

To verify if a file is stored in LFS:

```bash
git lfs ls-files | grep filename
```

## Performance Notes

- **Clone Time**: First clone will download all LFS objects (~192MB)
- **Subsequent Clones**: Use `git lfs clone` for faster cloning with deferred downloads
- **Storage**: LFS objects are stored separately from git history, keeping repository lean

## Migration History

- **2025-11-19**: Git LFS installed and configured
- **Pre-LFS**: Binary assets (192MB) exist in git history from before LFS setup
- **Going Forward**: All new binary assets automatically use LFS

## References

- [Git LFS Official Documentation](https://git-lfs.github.com/)
- [Unity + Git LFS Best Practices](https://unity.com/solutions/version-control)
- [GitHub LFS Billing](https://docs.github.com/en/billing/managing-billing-for-git-large-file-storage)

## Notes for PR Reviewers

This repository was converted to use Git LFS on 2025-11-19. Historical commits contain binary data directly, but all future commits will use LFS automatically. The `.gitattributes` file is properly configured for all Unity asset types.
