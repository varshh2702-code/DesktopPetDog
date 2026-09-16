# Replacing or extending animation artwork

The shipped main dog is `src/DesktopPetDog/Assets/golden-retriever-puppy.png`, embedded as a WPF Resource. The vector dog drawn in `DogView.cs` is the legal, always-available fallback.

For production artwork, use original PNG or WebP frame sequences with transparent backgrounds. Recommended folders are:

```text
Assets/Animations/standing/
Assets/Animations/walking/
Assets/Animations/running/
Assets/Animations/sitting/
Assets/Animations/sleeping/
Assets/Animations/waking/
Assets/Animations/stretching/
Assets/Animations/yawning/
Assets/Animations/looking/
Assets/Animations/playing/
Assets/Animations/jumping/
Assets/Animations/petted/
Assets/Accessories/
```

Use equal canvas dimensions for every frame, descriptive numeric names such as `walking_001.png`, and 96–192 DPI. Mark assets as `Resource` in the project file so they are embedded in the single executable. Decode frames once and cache frozen `BitmapSource` objects; do not read files on every animation tick. If a sequence is missing or invalid, call the existing vector `DogView.Animate` fallback instead of throwing an exception.

Keep artwork original or properly licensed. Do not copy frames from the reference video unless you own them or have permission.
