// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using LibGit2Sharp;

namespace Microsoft.Build.Tasks.Git.UnitTests
{
    internal class TestSubmodule : Submodule
    {
        private readonly string _url;
        private readonly string _path;
        private readonly ObjectId _workDirCommitSha;

        public TestSubmodule(string url, string path, string workDirCommitSha)
        {
            _url = url;
            _path = path;
            _workDirCommitSha = new ObjectId(workDirCommitSha);
        }

        public override string Url => _url;
        public override string Path => _path;
        public override ObjectId WorkDirCommitId => _workDirCommitSha;
    }
}
