using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Octokit;

namespace THNETII.WebServices.FileProviders.GitHub
{
    public sealed class GitHubFileProvider : IFileProvider, IDisposable
    {
        private readonly GitHubClient gitHubClient;
        private readonly HttpClient httpClient;

        public GitHubFileProvider(string repoOwner, string repoName, GitHubClient gitHubClient, HttpClient httpClient)
        {
            RepositoryOwner = repoOwner;
            RepositoryName = repoName;
            this.gitHubClient = gitHubClient;
            this.httpClient = httpClient;
        }

        public string RootReference { get; }

        public string RepositoryOwner { get; }

        public string RepositoryName { get; }

        public IDirectoryContents GetDirectoryContents(string subpath)
            => GetDirectoryContentsAsync(subpath).GetAwaiter().GetResult();

        public async Task<IDirectoryContents> GetDirectoryContentsAsync(string subpath)
        {
            var (owner, repoName, reference) = (RepositoryOwner, RepositoryName, RootReference);
            try
            {
                var contents = await GetRepositoryContents(owner, repoName, reference, subpath)
                .ConfigureAwait(false);
                return new GitHubRepositoryContentDirectoryContents(owner, repoName, reference, contents, gitHubClient, httpClient);
            }
            catch (NotFoundException)
            {
                return NotFoundDirectoryContents.Singleton;
            }
        }

        private Task<IReadOnlyList<RepositoryContent>> GetRepositoryContents(string owner, string repoName, string reference, string subpath)
        {
            var contentClient = gitHubClient.Repository.Content;
            return reference is null
               ? contentClient.GetAllContents(owner, repoName, subpath)
               : contentClient.GetAllContentsByRef(owner, repoName, subpath, reference)
               ;
        }

        public IFileInfo GetFileInfo(string subpath)
            => GetFileInfoAsync(subpath).GetAwaiter().GetResult();

        public async Task<IFileInfo> GetFileInfoAsync(string subpath)
        {
            var dir = await GetDirectoryContentsAsync(subpath).ConfigureAwait(false);
#if NETSTANDARD2_1
            if (dir is GitHubRepositoryContentDirectoryContents ghDir)
            {
                await using var enumerator = ghDir.GetAsyncEnumerator(default);
                if (await enumerator.MoveNextAsync())
                    return enumerator.Current;
                return new NotFoundFileInfo(subpath);
            }
#endif // !NETSTANDARD2_1 
            return dir.FirstOrDefault() ?? new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
