// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LibGit2Sharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks.Git
{
    internal static class Implementation
    {
        static Implementation()
        {
            // .NET Core apps that depend on native libraries load them directly from paths specified
            // in .deps.json file of that app and the native library loader just works.
            // However, .NET Core currently doesn't support .deps.json for plugins such as msbuild tasks.
            if (IsRunningOnNetCore())
            {
                var dir = Path.GetDirectoryName(typeof(Implementation).Assembly.Location);
                GlobalSettings.NativeLibraryPath = Path.Combine(dir, "runtimes", GetNativeLibraryRuntimeId(), "native");
            }
        }

        /// <summary>
        /// Returns true if the runtime is .NET Core.
        /// </summary>
        private static bool IsRunningOnNetCore()
            => typeof(object).Assembly.GetName().Name != "mscorlib";

        /// <summary>
        /// Determines the RID to use when loading libgit2 native library.
        /// This method only supports RIDs that are currently used by LibGit2Sharp.NativeBinaries.
        /// </summary>
        private static string GetNativeLibraryRuntimeId()
        {
            var processorArchitecture = IntPtr.Size == 8 ? "x64" : "x86";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "win7-" + processorArchitecture;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux-" + processorArchitecture;
            }

            throw new PlatformNotSupportedException();
        }

        public static string LocateRepository(string directory)
        {
            string DiscoverRoot(string dir)
            {
                // Repository.Discover returns the path to .git directory, repository root is its parent directory.
                // Returns null if the path is invalid or no repository is found.
                var gitDir = Repository.Discover(directory);
                if (gitDir == null)
                {
                    return null;
                }

                return Path.Combine(gitDir, "..");
            }

            var root = DiscoverRoot(directory);
            if (root == null)
            {
                return null;
            }

            return Path.GetFullPath(root).EndWithSeparator();
        }

        public static string GetRepositoryUrl(this Repository repository, string remoteName = null)
        {
            var remotes = repository.Network.Remotes;
            var remote = string.IsNullOrEmpty(remoteName) ? (remotes["origin"] ?? remotes.FirstOrDefault()) : remotes[remoteName];
            var url = remote?.Url;
            return url.EndsWith(".git") ? url.Substring(0, url.Length - ".git".Length) : url;
        }

        public static string GetRevisionId(this Repository repository)
        {
            return repository.Head.Tip.Sha;
        }

        public static ITaskItem[] GetSourceRoots(this Repository repository)
        {
            var result = new List<TaskItem>();

            var repoRoot = Path.GetFullPath(repository.Info.WorkingDirectory).EndWithSeparator();

            var item = new TaskItem(repoRoot);
            item.SetMetadata("SourceControl", "Git");
            item.SetMetadata("RepositoryUrl", GetRepositoryUrl(repository));
            item.SetMetadata("RevisionId", GetRevisionId(repository));
            result.Add(item);

            foreach (var submodule in repository.Submodules)
            {
                item = new TaskItem(Path.GetFullPath(Path.Combine(repoRoot, submodule.Path)).EndWithSeparator());
                item.SetMetadata("SourceControl", "Git");
                item.SetMetadata("RepositoryUrl", submodule.Url);
                item.SetMetadata("RevisionId", submodule.HeadCommitId.Sha); // TODO: https://github.com/tmat/repository-info/issues/1: HeadCommitId might be null if the submodule is not commited
                item.SetMetadata("ContainingRoot", repoRoot);
                item.SetMetadata("NestedRoot", submodule.Path.EndWithSeparator());
                result.Add(item);
            }

            return result.ToArray();
        }

        public static ITaskItem[] GetUntrackedFiles(this Repository repository, ITaskItem[] files)
        {
            var repoRoot = repository.Info.WorkingDirectory.EndWithSeparator();
            var pathComparer = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // TODO: 
            // file.ItemSpec are relative to the project dir.
            // Does gitlib handle backslashes on Windows?
            // Consider using paths relative to the repo root to avoid GetFullPath.
            return files.Where(file =>
            {
                var fullPath = Path.GetFullPath(file.ItemSpec);
                return fullPath.StartsWith(repoRoot, pathComparer) && repository.Ignore.IsPathIgnored(fullPath);
            }).ToArray();
        }
    }
}
