# T8NTextureTool
<img width="256" height="256" alt="T8NTextureTool" src="https://github.com/user-attachments/assets/69576cfe-d607-4f93-9ef8-f02bf75e6d85" />

## Download
Go to the Releases tab and download the latest version:
**1.0.1**

Extract and Run, "T8N TextureTool.exe"

## Windows SmartScreen Warning

Because this app is not code-signed (yet), Windows might show, “Windows protected your PC”

This is normal for independent/open-source tools.

To run:
Click More Info
Click Run Anyway

No system modifications are made by this app.

## Safety

This tool only:
- Reads the selected input image
- Writes output files to your chosen folder
- Optionally deletes extra textures in the chosen output folder

## How It Works

The app uses an embedded version of ImageMagick via Magick.NET, so no external tools are required, and everything runs locally inside the app.

## How to Use

1. Select an input image (File > Select Image)
2. Select an output folder (Export > Select Output Folder)
3. Enter tile resolution (e.g. 128, 256, etc.)
4. Set Astc block (e.g. 6x6, and 4x4)
5. (Optional) configure Mips, Alpha, or Strip Alpha
6. (Optional) Set your filename pattern: **output_{index}** (You will only need to edit this to add something like, v2, a tag like T8N, etc.)
7. (Optional) Delete duplicate textures incase you export another version of the same texture array, and want to delete the extra textures

Click Run

## Discord Server
https://discord.gg/uKejkeu3rC
