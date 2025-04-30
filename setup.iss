[Setup]
AppName=Seismoscope
AppVersion=1.0.0
DefaultDirName={pf}\Seismoscope
DefaultGroupName=Seismoscope
OutputBaseFilename=setup
OutputDir=.
Compression=lzma
SolidCompression=yes

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Seismoscope"; Filename: "{app}\Seismoscope.exe"

[Run]
Filename: "{app}\Seismoscope.exe"; Description: "Lancer Seismoscope"; Flags: nowait postinstall skipifsilent
