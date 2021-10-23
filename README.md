# Pocket Palm Heroes

Native edition

## Cloning

This repo contains Git Submodules (with dependencies), so after cloning, you need to initialize them:

```shell script
git clone THIS_REPO_URL
git submodule init
git submodule update

# OR (in a single command instead of three):
git clone --recurse-submodules THIS_REPO_URL
```

## Building and running

This project can be compiled and run in Windows 10:

1. Install Visual Studio Community 2019 and the following components in its Installer:
    * Desktop development with C++
    * MSVC v142
    * Windows 10 SDK
    * C++ ATL for latest v142 build tools
    * C++ MFC for latest v142 build tools
2. Open `pheroes/HMM.sln` as a solution.
3. Select HMM or MapEditor in Solutuon Explorer and run it via Green Triangle.

## Credits

This work contains the following code:

* Pocket Heroes Game, Apache 2.0 License
* libxml2, MIT License

Also, there are the following dependencies in the `externals` dir:

* Windows Template Library, Microsoft Public License
* LZ👌 (lzokay), MIT License
* libpng and zlib, zlib License

## License

This project is [Apache 2.0](LICENSE)-licensed.
