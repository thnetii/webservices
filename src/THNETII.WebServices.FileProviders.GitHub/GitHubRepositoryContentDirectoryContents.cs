using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
#if NETSTANDARD2_1
using System.Threading;
#endif
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;

using Octokit;

namespace THNETII.WebServices.FileProviders.GitHub
{
    [SuppressMessage("Naming", "CA1710: Identifiers should have correct suffix")]
    public class GitHubRepositoryContentDirectoryContents : IDirectoryContents
#if NETSTANDARD2_1
        , IAsyncEnumerable<IFileInfo>
#endif
    {
        private readonly string owner;
        private readonly string repoName;
        private readonly string reference;
        private readonly IReadOnlyList<RepositoryContent> contents;
        private readonly GitHubClient gitHubClient;
        private readonly HttpClient rawClient;

        public GitHubRepositoryContentDirectoryContents(string owner, string repoName, string reference, IReadOnlyList<RepositoryContent> contents, GitHubClient gitHubClient, HttpClient rawClient)
        {
            this.owner = owner;
            this.repoName = repoName;
            this.reference = reference;
            this.contents = contents;
            this.gitHubClient = gitHubClient;
            this.rawClient = rawClient;
        }

        public bool Exists => contents is IReadOnlyList<RepositoryContent>;

        public IEnumerator<IFileInfo> GetEnumerator()
            => GetEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private async Task<IFileInfo> GetFileInfoAsync(RepositoryContent content)
        {
            var req = new CommitRequest
            {
                Path = content.Path,
                Sha = reference
            };
            var history = await gitHubClient.Repository.Commit.GetAll(
                owner, repoName, req).ConfigureAwait(false);
            var lastModified = history.Max(c => c.Commit.Author.Date);

            return new GitHubRepositoryContentFileInfo(
                content, lastModified, rawClient);
        }

#if NETSTANDARD2_1
        public IAsyncEnumerator<IFileInfo> GetAsyncEnumerator(CancellationToken cancelToken)
            => GetAsyncEnumerable().GetAsyncEnumerator();

        private async IAsyncEnumerable<IFileInfo> GetAsyncEnumerable()
        {
            foreach (var c in contents)
            {
                yield return await GetFileInfoAsync(c).ConfigureAwait(false);
            }
        }
#endif
        private IEnumerable<IFileInfo> GetEnumerable()
        {
            foreach (var c in contents)
            {
                var fi = GetFileInfoAsync(c).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
                yield return fi;
            }
        }
    }
}
