// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SourceLink.VSO.Git
{
    public sealed class GetVsoGitSourceLinkUrl : Task
    {
        [Required]
        public ITaskItem SourceRoot { get; set; }

        [Output]
        public string SourceLinkUrl { get; set; }

        public override bool Execute()
        {
            if (!string.IsNullOrEmpty(SourceRoot.GetMetadata("SourceLinkUrl")) ||
                SourceRoot.GetMetadata("SourceControl") != "Git")
            {
                SourceLinkUrl = "N/A";
                return true;
            }

            bool EndsWith(string path, char c)
                => path.Length > 0 && path[path.Length - 1] == c;

            var repoUrl = SourceRoot.GetMetadata("RepositoryUrl");
            if (!Uri.TryCreate(repoUrl, UriKind.Absolute, out var repoUri))
            {
                Log.LogError($"SourceRoot.RepositoryUrl of '{SourceRoot.ItemSpec}' is invalid: '{repoUrl}'");
                return false;
            }

            if (!repoUri.Host.EndsWith(".visualstudio.com", StringComparison.OrdinalIgnoreCase))
            {
                SourceLinkUrl = "N/A";
                return true;
            }

            bool IsHexDigit(char c)
                => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';

            string revisionId = SourceRoot.GetMetadata("RevisionId");
            if (revisionId == null || revisionId.Length != 40 || !revisionId.All(IsHexDigit))
            {
                Log.LogError($"SourceRoot.RevisionId of '{SourceRoot.ItemSpec}' is not a valid commit hash: '{revisionId}'");
                return false;
            }

            var url = repoUri.ToString();
            if (!EndsWith(url, '/'))
            {
                url += "/";
            }

            SourceLinkUrl = url + "items?api-version=1.0&versionType=commit&version=" + revisionId + "&scopePath=/*";
            return true;
        }
    }
}
