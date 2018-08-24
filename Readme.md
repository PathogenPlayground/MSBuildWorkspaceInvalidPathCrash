This repository demonstrates a crash in the Roslyn MSBuild workspace infrastructure.

<!-- TOC -->

- [Project Overview](#project-overview)
    - [`DumpAdditionalFilesAnalyzer`](#dumpadditionalfilesanalyzer)
    - [`DumpAdditionalFilesTool`](#dumpadditionalfilestool)
    - [`MalformedProject`](#malformedproject)
- [Actual Behavior](#actual-behavior)
    - [Actual behavior when building from Visual Studio](#actual-behavior-when-building-from-visual-studio)
    - [Actual behavior when running `msbuild` to build directly](#actual-behavior-when-running-msbuild-to-build-directly)
    - [Actual behavior in `Microsoft.CodeAnalysis.Workspaces.MSBuild`](#actual-behavior-in-microsoftcodeanalysisworkspacesmsbuild)
- [Expected Behavior for `Microsoft.CodeAnalysis.Workspaces.MSBuild`](#expected-behavior-for-microsoftcodeanalysisworkspacesmsbuild)

<!-- /TOC -->

# Project Overview

## `DumpAdditionalFilesAnalyzer`

A simple analyzer that lists all additional files in the compilation.

This analyzer is provided to show how the additional files are exposed within `csc`.

## `DumpAdditionalFilesTool`

A standalone tool that loads an MSBuild project and lists all additional files in it.

## `MalformedProject`

A project with a malformed `AdditionalFiles` entry.

# Actual Behavior

## Actual behavior when building from Visual Studio

The project with the malformed path loads fine can can be built.

Visual Studio will emit a warning to the Error List window. (This warning can be silenced if the file is marked as invisible with `<Visible>false</Visible>`.)

Upon building, `DumpAdditionalFilesAnalyzer` will list the additional files:

```
2>CSC : warning AdditionalFilesAnalyzer: Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\ValidAdditionalFile.txt'
2>CSC : warning AdditionalFilesAnalyzer: Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\TEST::'
```

## Actual behavior when running `msbuild` to build directly

The project with the malformed path loads find and can be built.

MSBuild will emit a warning if the logging verbosity is set to "detailed".

`DumpAdditionalFilesAnalyzer` will list the additional files:

```
  CSC : warning AdditionalFilesAnalyzer: Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\ValidAdditionalFile.txt' [C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\MalformedProject.csproj]
  CSC : warning AdditionalFilesAnalyzer: Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\TEST::' [C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\MalformedProject.csproj]
```

Note: Observing the output from MSBuild shows that the paths from MSBuild are passed to `csc` directly:

`[...]csc.exe [...] /additionalfile:ValidAdditionalFile.txt /additionalfile:TEST:: Class1.cs [...]`

So `csc.exe` is the one adding the project paths here.

## Actual behavior in `Microsoft.CodeAnalysis.Workspaces.MSBuild`

(Run `DumpAdditionalFilesTool` to demonstrate.)

A malformed path in an MSBuild project file causes `MSBuildWorkspace.OpenProjectAsync` to throw a `NotSupportedException` with the message "The given path's format is not supported." and the following stack trace:

```
   at System.Security.Permissions.FileIOPermission.EmulateFileIOPermissionChecks(String fullPath)
   at System.Security.Permissions.FileIOPermission.QuickDemand(FileIOPermissionAccess access, String fullPath, Boolean checkForDuplicates, Boolean needFullPath)
   at System.IO.Path.GetFullPath(String path)
   at Microsoft.CodeAnalysis.MSBuild.ProjectFile.GetAbsolutePathRelativeToProject(String path) in /_/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs:line 201
   at Microsoft.CodeAnalysis.MSBuild.ProjectFile.GetDocumentFilePath(ITaskItem documentItem) in /_/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs:line 206
   at Microsoft.CodeAnalysis.MSBuild.ProjectFile.MakeAdditionalDocumentFileInfo(ITaskItem documentItem) in /_/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs:line 184
   at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at System.Collections.Immutable.ImmutableArray.CreateRange[T](IEnumerable`1 items)
   at System.Collections.Immutable.ImmutableArray.ToImmutableArray[TSource](IEnumerable`1 items)
   at Microsoft.CodeAnalysis.MSBuild.ProjectFile.CreateProjectFileInfo(ProjectInstance project) in /_/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs:line 133
   at Microsoft.CodeAnalysis.MSBuild.ProjectFile.<BuildProjectFileInfoAsync>d__17.MoveNext() in /_/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs:line 101
** Snipped **
```

The problem line in question appears to be the use of `Path.GetFullPath` at [ProjectFile.cs:202](https://github.com/dotnet/roslyn/blob/66f85bd989527c50802669b3474ece40dccde909/src/Workspaces/Core/MSBuild/MSBuild/ProjectFile/ProjectFile.cs#L202).

# Expected Behavior for `Microsoft.CodeAnalysis.Workspaces.MSBuild`

The crash should not occur and the tool should list the additional files:

```
Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\ValidAdditionalFile.txt'
Additional file: 'C:\Development\Playground\MSBuildWorkspaceInvalidPathCrash\MalformedProject\TEST::'
```
