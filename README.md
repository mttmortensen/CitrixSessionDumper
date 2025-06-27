# CitrixSessionDumper

## Overview
**CitrixSessionDumper** is a lightweight, self-contained C# utility designed for execution directly on a Citrix VDA. It collects key diagnostic information about the currently active Citrix session, including system context, applied GPOs, and user profile path mappings.

This tool is ideal for helpdesk, sysadmins, or engineers who need a fast, consistent snapshot of a user's Citrix environment without digging manually through tools like Group Policy Management, Event Viewer, or Studio.

---

## Features

### Current Functionality
- **Session Info**
  - Username, domain, machine name, client IP, timestamp
- **Process & System Info**
  - Session PID, memory usage, system uptime
- **Machine-Level GPOs**
  - Pulled from `gpresult /scope:computer`
  - Filters and formats only meaningful applied policy names
- **Profile Path Reference**
  - Old Farm: `\\4life.com\shares\Profiles\UserProfiles\<username>`
  - Current Farm: `\\4life.com\shares\UserProfiles$\<username>`

### In Progress / Planned
- **App-Specific Logs**
  - Pull Event Viewer logs (Warnings/Errors only) scoped to the Citrix-published application name
- **Dynamic App Detection**
  - Detect which published app was launched within the session for log filtering

---

## Example Output
===========User's Info===========  
==== SESSION INFO ====  
Timestamp: 2025-06-27_11-29-51  
Username: mattm  
Domain: 4LIFE  
Machine: [Citrix-VDA]  
Client IP: [Private-User-IP]  

==== PROCESS & SYSTEM INFO ====  
Session PID: 7760  
Memory Usage: 49 MB    
System Uptime: 5d 16h 23m  

==== APPLIED GPOs ====  
Citrix Profile Management Server Policy  
Citrix Microsoft Office Licensing Policy  
Citrix RDS Licensing  
Citrix Keyboard Sync  
Citrix Session Time Limit Policy  
WSUS Citrix Servers Group  
Default Domain Policy  
Internet Security Policy  
Default Domain Policy v2  
PA SSL Cert  
Local Group Policy  

==== CITRIX PROFILE PATHS ====  
Old Farm Profile: \4life.com\shares\Profiles\UserProfiles\mattm  
Current Farm Profile: \4life.com\shares\UserProfiles$\mattm  

## Usage

1. Build the executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -o ./publish  
2. Transfer the .exe from the ./publish directory to any Citrix VDA.
3. Run it locally on the VDA (manually or via script/Studio command).
Dump will be saved to:  
```C:\Logs\CitrixSesdsionDump.txt```

## Output
Each run appends a new session dump to the same log file, clearly delimited and timestamped for easy review.  
### Requirements
- No .NET Framework needed when using --self-contained true  
- Must be run on the Citrix VDA (not from the client side)  
- Basic permission to run local PowerShell commands and query system info  

### 🚧 Notes
This project is being built iteratively to replace slow or manual Citrix troubleshooting practices.  
All code is written in raw C# — no ASP.NET, no LINQ, no frameworks.  

#### 🧠 Author
Matt Mortensen  
Citrix Admin | Developer-in-Progress | Systems Sleuth  

📌 License
Internal use only. Not yet licensed for distribution.
