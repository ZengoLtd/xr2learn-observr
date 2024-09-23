- Extract [observr_assets.zip](https://drive.zengo.eu/s/HdeqZptzKptQb6K) into the project
- Open the project and press ignore for the safemode
- Install Newtonsoft package with package manager. Click "Add package from git URL" and use this URL: com.unity.nuget.newtonsoft-json@3.2
- Add SCRCPY and ADB to the project. Download from here: https://scrcpy.org/
- Copy the directory's content into your project's plugin folder

If you want to use tests
- Install NuGet with package manager. Click "Add package from git URL" and use this URL:  https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
- Install Moq from NuGet.
- Install TestPackage from asset folder
- Install [Auto-ASMDEF](https://assetstore.unity.com/packages/tools/utilities/auto-asmdef-156502)
- Reopen Project
- Delete any AutoASMDEF folder
- Alt+Q open Auto-ASMDEF plugin and update assembly definitions