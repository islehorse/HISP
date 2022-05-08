#!/usr/bin/python3
import os
import subprocess
import time
import datetime
import binascii


def update_asm_info(assemblyinfofile):
    global commit_hash
    global commit_tag
    global commit_branch
    global assembly_version
    lines = open(assemblyinfofile, "r").readlines()
    for i in range(0,len(lines)):
        if lines[i].startswith("[assembly: AssemblyVersion(\""):
            lines[i] = "[assembly: AssemblyVersion(\""+assembly_version+"\")]\n"
        if lines[i].startswith("[assembly: AssemblyFileVersion(\""):
            lines[i] = "[assembly: AssemblyFileVersion(\""+assembly_version+"\")]\n"
    open(assemblyinfofile, "w").writelines(lines)

    
# Determine git stuff.
versioning_folder = os.path.join("LibHISP", "Resources", "Versioning")

if not os.path.exists(versioning_folder):
    os.mkdir(versioning_folder)

commit_hash = "0"*40
commit_tag = "v0.0.0"
commit_branch = "master"

try:
    subprocess.run(['git', 'add', '-A'], stdout=subprocess.PIPE)
    subprocess.run(['git', 'commit', '-m', 'Update made automatically due to pressing build'], stdout=subprocess.PIPE)
    commit_hash = subprocess.run(['git', 'rev-parse', '--verify', 'HEAD'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_tag = subprocess.run(['git', 'describe', '--abbrev=0', '--tags'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_tag += "." + subprocess.run(['git', 'rev-list', commit_tag+'..HEAD', '--count'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
    commit_branch = subprocess.run(['git', 'branch', '--show-current'], stdout=subprocess.PIPE).stdout.replace(b"\r", b"").replace(b"\n", b"").decode("UTF-8")
except FileNotFoundError:
    print("Git not installed")

commit_date = datetime.datetime.now().strftime("%d/%m/%Y")
commit_time = datetime.datetime.now().strftime("%H:%M:%S")

open(os.path.join(versioning_folder, "GitCommit"), "w").write(commit_hash)
open(os.path.join(versioning_folder, "GitTag"   ), "w").write(commit_tag)
open(os.path.join(versioning_folder, "GitBranch"), "w").write(commit_branch)
open(os.path.join(versioning_folder, "BuildDate"), "w").write(commit_date)
open(os.path.join(versioning_folder, "BuildTime"), "w").write(commit_time)

# Derive assembly version 
points = commit_tag.replace("v", "").split(".")
while len(points) < 4:
    points.append("0")
assembly_version = ".".join(points)

update_asm_info(os.path.join("LibHISP", "Properties", "AssemblyInfo.cs"))
update_asm_info(os.path.join("N00BS", "Properties", "AssemblyInfo.cs"))
update_asm_info(os.path.join("HISPd", "Properties", "AssemblyInfo.cs"))

control_file = os.path.join("HISPd", "Resources", "DEBIAN", "control")
lines = open(control_file, "r").readlines()
for i in range(0,len(lines)):
    if lines[i].startswith(b"Version: "):
            lines[i] = b"Version: "+bytes(commit_tag.replace("v", ""), "UTF-8")
open(control_file, "w").writelines(control_file)