using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


// Set Working Directory
private static string SOLUTION_DIR = Path.GetDirectoryName(Path.GetDirectoryName(GetSourceFile()));
Directory.SetCurrentDirectory(SOLUTION_DIR);

private static string VERSIONING_FOLDER = Path.Combine("LibHISP", "Resources", "Versioning");

// Defaults (for if git isn't installed)
private static string COMMIT_HASH = "0000000000000000000000000000000000000000";
private static string COMMIT_TAG = "v0.0.0";
private static string COMMIT_BRANCH = "master";


// Get Build Date
private static string COMMIT_DATE = DateTime.UtcNow.ToString("dd/MM/yyyyy");
private static string COMMIT_TIME = DateTime.UtcNow.ToString("H:M:s");

// IDK how this works, found it on stackoverflow, but it returns the path to the prebuild.csx
private static string GetSourceFile([CallerFilePath] string file = "")
{
    return file;
}

// Updates version inside a AssemblyInfo.cs file
private static void UpdateAsmInfo(string assemblyInfoFile)
{
    string assembly_version = DetermineAssemblyVersion();

    Console.WriteLine("Updating Verson inside: " + assemblyInfoFile);
    string[] lines = File.ReadAllLines(assemblyInfoFile);
    for(int i = 0; i < lines.Length; i++)
    {
        if (lines[i].StartsWith("[assembly: AssemblyVersion(\""))
        {
            lines[i] = "[assembly: AssemblyVersion(\"" + assembly_version + "\")]";
        }

        else if (lines[i].StartsWith("[assembly: AssemblyFileVersion(\""))
        {
            lines[i] = "[assembly: AssemblyFileVersion(\"" + assembly_version + "\")]";
        }
    }

    File.WriteAllLines(assemblyInfoFile, lines);
}


// Create "versioning" folder
public static void CreateVersioningFolder()
{
    if (!Directory.Exists(VERSIONING_FOLDER))
    {
        Directory.CreateDirectory(VERSIONING_FOLDER);
    }
}

// Function for running a process
public static string StartProcess(string[] cmd)
{
    using (Process proc = new Process())
    {
        proc.StartInfo.FileName = cmd[0];
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.Arguments = String.Join(" ", cmd.Skip(1).ToArray());
        proc.Start();
        string output = proc.StandardOutput.ReadToEnd().Replace("\r", "").Replace("\n", "");
        return output;
    }
}

// Run git to determine version
private static void RunGit()
{
    try
    {
        COMMIT_HASH = StartProcess(new string[] { "git", "rev-parse", "--verify", "HEAD" });
        COMMIT_TAG = StartProcess(new string[] { "git", "describe", "--abbrev=0", "--tags" });
        COMMIT_TAG += "." + StartProcess(new string[] { "git", "rev-list", COMMIT_TAG + "..HEAD", "--count" });
        COMMIT_BRANCH = StartProcess(new string[] { "git", "branch", "--show-current" });
    } catch (Exception e) { Console.Error.WriteLine(e.Message); }
}

// Write Resources to Versioning Folder
private static void WriteResources()
{
    File.WriteAllText(Path.Combine(VERSIONING_FOLDER, "GitCommit"), COMMIT_HASH);
    File.WriteAllText(Path.Combine(VERSIONING_FOLDER, "GitTag"   ), COMMIT_TAG);
    File.WriteAllText(Path.Combine(VERSIONING_FOLDER, "GitBranch"), COMMIT_BRANCH);
    File.WriteAllText(Path.Combine(VERSIONING_FOLDER, "BuildDate"), COMMIT_DATE);
    File.WriteAllText(Path.Combine(VERSIONING_FOLDER, "BuildTime"), COMMIT_TIME);

}

// Find assembly version based on commit tag
private static string DetermineAssemblyVersion()
{
    List<String> points = COMMIT_TAG.Replace("v", "").Split('.').ToList();
    while(points.Count < 4)
    {
        points.Add("0");
    }
    return String.Join(".", points.ToArray());
}

private static void UpdateVersionInControlFile(string controlFile)
{
    Console.WriteLine("Updating Verson inside: " + controlFile);
    string[] lines = File.ReadAllLines(controlFile);
    for (int i = 0; i < lines.Length; i++)
    {
        if (lines[i].StartsWith("Version: "))
        {
            lines[i] = "Version: " + COMMIT_TAG.Replace("v", "");
        }
    }
    File.WriteAllLines(controlFile, lines);
}

CreateVersioningFolder();
RunGit();
WriteResources();

// Update AssemblyInfo.cs files
UpdateAsmInfo(Path.Combine("LibHISP", "Properties", "AssemblyInfo.cs"));
UpdateAsmInfo(Path.Combine("MPN00BS", "Properties", "AssemblyInfo.cs"));
UpdateAsmInfo(Path.Combine("HISPd",   "Properties", "AssemblyInfo.cs"));

// Update control file in dpkg.
UpdateVersionInControlFile(Path.Combine("HISPd", "Resources", "DEBIAN", "control"));