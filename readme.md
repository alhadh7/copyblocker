File Protection System
A Windows application that prevents unauthorized file copying from protected folders.
Features

* Prevents copying files out of protected folders
* Monitors clipboard for file operations
* System tray integration
* Real-time file system monitoring
* Administrative privileges enforcement

Requirements:

Windows 10/11
.NET 6.0 SDK or later
Visual Studio Code (recommended) or Visual Studio 2019/2022

Quick Start:

Clone the repository

cd FileProtectionSystem

Build the project:

dotnet build

Run the application (requires administrator privileges):


Navigate to bin/Debug/net6.0-windows/
Right-click FileProtectionSystem.exe
Select "Run as administrator"

Default Configuration

Protected folder path: C:\ProtectedFolder
To change the protected folder, modify the rootPath variable in Program.cs

Project Structure
CopyFileProtectionSystem/
├── Program.cs              # Main application entry point
├── FileProtectionSystem.csproj   # Project configuration
├── app.manifest            # Administrator privileges manifest
└── README.md              # This file

How It Works:

The application runs in the system tray
It monitors the protected folder for:

File creation
File deletion
File modifications
File renaming


Clipboard monitoring prevents:

Copying protected files out
Pasting unauthorized files in


Safety Features:

Administrator privileges requirement
Path traversal protection
Proper resource cleanup
Comprehensive error handling

Known Limitations:

Requires administrator privileges
Works only on Windows systems
Cannot prevent file access through command line or programming APIs
Does not encrypt files
only for copying not for moving

Troubleshooting:

"Requires elevation" error

Run as administrator


Protected folder access denied

Check folder permissions
Verify administrator privileges


System tray icon not appearing

Check if another instance is running
Restart the application



Development:

Open in Visual Studio Code:

copyblocker .

Required extensions:

C# for Visual Studio Code
.NET Core Tools


Build and run:

dotnet build
dotnet run


Assumptions Made:

Users have administrative privileges
Single protected folder is sufficient
Windows is the target operating system
File operations through standard Windows UI
No network share protection required

Security Considerations

This tool prevents casual copying but is not a replacement for:

File encryption
Access control lists
Network security
DLP solutions


The tool cannot prevent:

Screen captures
Command line operations
Programmatic file access


There are several ways to close the File Protection System program:

Through System Tray (Recommended way):

Look for the shield icon in the system tray (bottom right corner of Windows)
Right-click on the shield icon
Click "Exit" or "Close"


Through Task Manager:

Press Ctrl + Shift + Esc to open Task Manager
Find "FileProtectionSystem" in the list of running processes
Select it and click "End Task"