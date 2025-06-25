# Required tools/files
- Install [wine](https://www.VTReplayPrefixhq.org/) with your package manager
- Install [winetricks](https://github.com/Winetricks/winetricks) (guides on their github page)
- This [tacview](https://github.com/Vyrtuoz/Tacview/tree/master) repository
- tacview and vtol vr on [steam](https://store.steampowered.com/)

*this guide assumes you use both on steam, but it should be possible to adapt it to other vtolvr/tacview installation methods*

# Wine setup
- Create a wine prefix

```bash
WINEPREFIX=~/.VTReplayPrefix winecfg
```

This should create a folder with a new wine drive_c with a few windows files.

Now whenever you want to run a command that's related to wine, you'll want to use this prefix like this

```bash
WINEPREFIX=~/.VTReplayPrefix %command%
```

# VTReplayConverter initial instalation
- Download the instaler on the releases page

- Run 

```bash
WINEPREFIX=~/.VTReplayPrefix wine start VTRC_installer.exe
```

This will install the VTReplayConverter to run with wine, but it probably won't work properly just yet
# Program installation folders
- Look for the wine appdata/Roaming/Bd folder steam created for vtolvr, for example:

`$HOME/.steam/steam/steamapps/compatdata/667970/pfx/drive_c/users/steamuser/AppData/Roaming/Boundless Dynamics, LLC`

*Assuming it's on the main drive*

*The tacview app id is 667970*

- Also look for the tacview appdata/Roaming wine folder, for example:

`$HOME/.steam/steam/steamapps/compatdata/1174860/pfx/drive_c/users/steamuser/AppData/Roaming/Tacview/`

*Assuming it's on the main drive*

*The tacview app id is 1174860*

- Also look for the tacview installation itself by clicking `Browse local files` on tacview on steam, it will be something like

`$HOME/.steam/steam/steamapps/common/Tacview/`


# Symbolic links to vtol and tacview

- Create a symbolic link between the drive_c AppData/Roaming of vtolvr and tacview(see last installation folders above) to the drive_c you created for VTReplayConverter. For example:

```bash
ln -s "$HOME/.steam/steam/steamapps/compatdata/667970/pfx/drive_c/users/steamuser/AppData/Roaming/Boundless Dynamics, LLC" "$HOME/.VTReplayPrefix/drive_c/users/$USER/AppData/Roaming/Boundless Dynamics, LLC"
```

```bash
ln -s "$HOME/.steam/steam/steamapps/compatdata/1174860/pfx/drive_c/users/steamuser/AppData/Roaming/Tacview" "$HOME/.VTReplayPrefix/drive_c/users/$USER/AppData/Roaming/Tacview"
```

- Create a symbolic link between the tacview instalation and the ProgramData folder on the converter wine folder. For example

```bash
ln -s $HOME/.steam/steam/steamapps/common/Tacview $HOME/.VTReplayPrefix/drive_c/ProgramData/Tacview
```

# Tacview mesh files
Following the guide to the previous step might solve it if your tacview installation already contains the mesh folder with the correct files
However ,if you get an error like `tacview mesh path not installed` when starting the converter, you need to

- Create the following folders on your tacview wine c drive, for example:
```bash
mkdir -p $HOME/.steam/steam/steamapps/common/Tacview/Data/Terrain/Custom
mkdir -p $HOME/.steam/steam/steamapps/common/Tacview/Data/Meshes
```
- Populate the folder `$HOME/.steam/steam/steamapps/common/Tacview/Data/Meshes` with data from [tacview](https://github.com/Vyrtuoz/Tacview/tree/master/3D%20Models/Data/Meshes)

# Wine Dotnet installation
You should probably still get an error like `Could not load file or assembly 'UnityEngine.PhysicsModule...'`, if you created a clean wine prefix for the converter

To solve this, simply run

```bash
WINEPREFIX=~/.VTReplayPrefix winetricks -f -q dotnet472
```
*-f is 'Don't check whether packages were already installed'*
*-q is 'Don't ask any questions, just install automatically'*

# End of the guide so far
I wrote this guide so i could document how i got the converter to run on linux

But if you're still having trouble using it on linux, feel free to contact [me](https://github.com/RodolphoVSantoro). I'll try to help and update this guide, if i can find the time to.
