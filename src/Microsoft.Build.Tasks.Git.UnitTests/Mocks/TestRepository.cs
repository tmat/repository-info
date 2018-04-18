// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Microsoft.Build.Tasks.Git.UnitTests
{
    internal class TestRepositoryInformation : RepositoryInformation
    {
        private readonly string _workingDirectory;

        public TestRepositoryInformation(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public override string WorkingDirectory => _workingDirectory;
        public override bool IsBare => false;
    }

    internal class TestNetwork : Network
    {
        private readonly IReadOnlyList<Remote> _remotes;

        public TestNetwork(IReadOnlyList<Remote> remotes)
        {
            _remotes = remotes;
        }

        public override RemoteCollection Remotes 
            => new TestRemoteCollection(_remotes);
    }

    internal class TestRemote : Remote
    {
        private readonly string _name;
        private readonly string _url;
                                
        public TestRemote(string name, string url)
        {
            _name = name;
            _url = url;
        }

        public override string Name => _name;
        public override string Url => _url;
    }

    internal class TestRemoteCollection : RemoteCollection
    {
        private readonly IReadOnlyList<Remote> _remotes;

        public TestRemoteCollection(IReadOnlyList<Remote> remotes)
        {
            _remotes = remotes;
        }

        public override Remote this[string name] 
            => _remotes.FirstOrDefault(r => r.Name == name);

        public override IEnumerator<Remote> GetEnumerator()
            => _remotes.GetEnumerator();
    }

    internal class TestCommit : Commit
    {
        private readonly string _sha;

        public TestCommit(string sha)
        {
            _sha = sha;
        }

        public override string Sha => _sha;
    }

    internal class TestBranch : Branch
    {
        private readonly string _tipCommitSha;

        public TestBranch(string tipCommitSha)
        {
            _tipCommitSha = tipCommitSha;
        }

        public override Commit Tip => new TestCommit(_tipCommitSha);
    }

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

    internal class TestSubmoduleCollection : SubmoduleCollection
    {
        private readonly IReadOnlyList<Submodule> _submodules;

        public TestSubmoduleCollection(IReadOnlyList<Submodule> submodules)
        {
            _submodules = submodules;
        }

        public override IEnumerator<Submodule> GetEnumerator()
            => _submodules.GetEnumerator();
    }

    internal class TestIgnore : Ignore
    {
        private readonly HashSet<string> _ignoredPaths;

        public TestIgnore(IEnumerable<string> ignoredPaths)
        {
            _ignoredPaths = new HashSet<string>(ignoredPaths);
        }

        public override bool IsPathIgnored(string relativePath)
            => _ignoredPaths.Contains(relativePath);
    }

    internal class TestRepository : IRepository
    {
        private readonly IReadOnlyList<TestRemote> _remotes;
        private readonly IReadOnlyList<Submodule> _submodules;
        private readonly IReadOnlyList<string> _ignoredPaths;
        private readonly string _headTipCommitSha;
        private readonly string _workingDir;

        public TestRepository(
            string workingDir,
            string headTipCommitSha,
            IReadOnlyList<TestRemote> remotes,
            IReadOnlyList<Submodule> submodules,
            IReadOnlyList<string> ignoredPaths)
        {
            _workingDir = workingDir;
            _remotes = remotes;
            _headTipCommitSha = headTipCommitSha;
            _submodules = submodules;
            _ignoredPaths = ignoredPaths;
        }

        public RepositoryInformation Info
            => new TestRepositoryInformation(_workingDir);

        public Network Network 
            => new TestNetwork(_remotes);

        public Branch Head
            => new TestBranch(_headTipCommitSha);

        public SubmoduleCollection Submodules
            => new TestSubmoduleCollection(_submodules);

        public Ignore Ignore
            => new TestIgnore(_ignoredPaths);

        #region Not Implemented

        public Configuration Config => throw new NotImplementedException();

        public Index Index => throw new NotImplementedException();

        public ReferenceCollection Refs => throw new NotImplementedException();

        public IQueryableCommitLog Commits => throw new NotImplementedException();

        public BranchCollection Branches => throw new NotImplementedException();

        public TagCollection Tags => throw new NotImplementedException();

        public Diff Diff => throw new NotImplementedException();

        public ObjectDatabase ObjectDatabase => throw new NotImplementedException();

        public NoteCollection Notes => throw new NotImplementedException();

        public Rebase Rebase => throw new NotImplementedException();
        
        public StashCollection Stashes => throw new NotImplementedException();

        public BlameHunkCollection Blame(string path, BlameOptions options)
        {
            throw new NotImplementedException();
        }

        public void Checkout(Tree tree, IEnumerable<string> paths, CheckoutOptions opts)
        {
            throw new NotImplementedException();
        }

        public void CheckoutPaths(string committishOrBranchSpec, IEnumerable<string> paths, CheckoutOptions checkoutOptions)
        {
            throw new NotImplementedException();
        }

        public CherryPickResult CherryPick(Commit commit, Signature committer, CherryPickOptions options)
        {
            throw new NotImplementedException();
        }

        public Commit Commit(string message, Signature author, Signature committer, CommitOptions options)
        {
            throw new NotImplementedException();
        }

        public string Describe(Commit commit, DescribeOptions options)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public GitObject Lookup(ObjectId id)
        {
            throw new NotImplementedException();
        }

        public GitObject Lookup(string objectish)
        {
            throw new NotImplementedException();
        }

        public GitObject Lookup(ObjectId id, ObjectType type)
        {
            throw new NotImplementedException();
        }

        public GitObject Lookup(string objectish, ObjectType type)
        {
            throw new NotImplementedException();
        }

        public MergeResult Merge(Commit commit, Signature merger, MergeOptions options)
        {
            throw new NotImplementedException();
        }

        public MergeResult Merge(Branch branch, Signature merger, MergeOptions options)
        {
            throw new NotImplementedException();
        }

        public MergeResult Merge(string committish, Signature merger, MergeOptions options)
        {
            throw new NotImplementedException();
        }

        public MergeResult MergeFetchedRefs(Signature merger, MergeOptions options)
        {
            throw new NotImplementedException();
        }

        public void RemoveUntrackedFiles()
        {
            throw new NotImplementedException();
        }

        public void Reset(ResetMode resetMode, Commit commit)
        {
            throw new NotImplementedException();
        }

        public void Reset(ResetMode resetMode, Commit commit, CheckoutOptions options)
        {
            throw new NotImplementedException();
        }

        public FileStatus RetrieveStatus(string filePath)
        {
            throw new NotImplementedException();
        }

        public RepositoryStatus RetrieveStatus(StatusOptions options)
        {
            throw new NotImplementedException();
        }

        public RevertResult Revert(Commit commit, Signature reverter, RevertOptions options)
        {
            throw new NotImplementedException();
        }

        public void RevParse(string revision, out Reference reference, out GitObject obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
