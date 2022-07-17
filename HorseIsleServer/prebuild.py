#!/usr/bin/python3

import os
import subprocess
import time
import datetime
import binascii
os.chdir(os.path.dirname(os.path.realpath(__file__)))

# Updates version inside a AssemblyInfo.cs file
def update_asm_info(assemblyinfofile):
    global commit_hash
    global commit_tag
    global commit_branch
    global assembly_version
    print("Updating Verson inside: "+assemblyinfofile)
    lines = open(assemblyinfofile, "rb").readlines()
    for i in range(0,len(lines)):
        if lines[i].startswith(b"[assembly: AssemblyVersion(\""):
            lines[i] = b"[assembly: AssemblyVersion(\""+bytes(assembly_version, "UTF-8")+b"\")]\r\n"
        if lines[i].startswith(b"[assembly: AssemblyFileVersion(\""):
            lines[i] = b"[assembly: AssemblyFileVersion(\""+bytes(assembly_version, "UTF-8")+b"\")]\r\n"
    open(assemblyinfofile, "wb").writelines(lines)

    
# Create "versioning" folder
try:
    versioning_folder = os.path.join("LibHISP", "Resources", "Versioning")
    if not os.path.exists(versioning_folder):
        os.mkdir(versioning_folder)
except FileExistsError:
    pass

# Defaults (for if git isn't installed)
    
commit_hash    = "0"*40
commit_tag     = "v0.0.0"
commit_branch  = "master"

# Run git to determine version info
try:
    commit_hash     =  subprocess.run(['git', 'rev-parse', '--verify', 'HEAD'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_tag      =  subprocess.run(['git', 'describe', '--abbrev=0', '--tags'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_tag      += "." + subprocess.run(['git', 'rev-list', commit_tag+'..HEAD', '--count'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_branch   =  subprocess.run(['git', 'branch', '--show-current'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
except FileNotFoundError:
    pass

# Get current time and date of this build
commit_date = datetime.datetime.now().strftime("%d/%m/%Y")
commit_time = datetime.datetime.now().strftime("%H:%M:%S")

# Write resources
open(os.path.join(versioning_folder, "GitCommit"), "w").write(commit_hash)
open(os.path.join(versioning_folder, "GitTag"   ), "w").write(commit_tag)
open(os.path.join(versioning_folder, "GitBranch"), "w").write(commit_branch)
open(os.path.join(versioning_folder, "BuildDate"), "w").write(commit_date)
open(os.path.join(versioning_folder, "BuildTime"), "w").write(commit_time)

# Get assembly version
points = commit_tag.replace("v", "").split(".")
while len(points) < 4:
    points.append("0")
assembly_version = ".".join(points)

# Update AssemblyInfo.cs files
update_asm_info(os.path.join("LibHISP", "Properties", "AssemblyInfo.cs"))
update_asm_info(os.path.join("N00BS"  , "Properties", "AssemblyInfo.cs"))
update_asm_info(os.path.join("HISPd"  , "Properties", "AssemblyInfo.cs"))

# Update control file in dpkg
control_file = os.path.join("HISPd", "Resources", "DEBIAN", "control")
print("Updating Verson inside: "+control_file)
lines = open(control_file, "rb").readlines()
for i in range(0,len(lines)):
    if lines[i].startswith(b"Version: "):
            lines[i] = b"Version: "+bytes(commit_tag.replace("v", ""), "UTF-8")+b"\n"
open(control_file, "wb").writelines(lines)

