[Setup]
AppName=Avalonia Test Tables
AppVersion=1.0
DefaultDirName={pf}\AvaloniaTestTables
DefaultGroupName=Avalonia Test Tables
OutputDir=output
OutputBaseFilename=AvaloniaTestTablesSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Release\net6.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Avalonia Test Tables"; Filename: "{app}\AvaloniaTestTables.exe"
Name: "{commondesktop}\Avalonia Test Tables"; Filename: "{app}\AvaloniaTestTables.exe"