// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks.Git
{
    internal static class Implementation
    {
        public static string LocateRepository(string directory, bool outermost)
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

            if (outermost)
            {
                while (true)
                {
                    var outerRoot = DiscoverRoot(Path.Combine(root, ".."));
                    if (outerRoot == null)
                    {
                        break;
                    }

                    root = outerRoot;
                }
            }

            return Path.GetFullPath(root).EndWithSeparator();
        }

        public static string GetRepositoryUrl(this Repository repository, string remoteName = null)
        {
            var remotes = repository.Network.Remotes;
            var remote = string.IsNullOrEmpty(remoteName) ? (remotes["origin"] ?? remotes.FirstOrDefault()) : remotes[remoteName];
            return remote?.Url;
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
                item.SetMetadata("RevisionId", submodule.HeadCommitId.Sha);
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
