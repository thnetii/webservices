using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;

using Octokit;

namespace THNETII.WebServices.FileProviders.GitHub
{
    public sealed class GitHubRepositoryContentFileInfo : IFileInfo
    {
        private readonly RepositoryContent content;
        private readonly HttpClient contentDownloader;
        private readonly byte[] base64Data;

        public GitHubRepositoryContentFileInfo(RepositoryContent content, DateTimeOffset lastModified, HttpClient contentDownloader)
        {
            this.content = content;
            base64Data = content?.EncodedContent is string d ? Convert.FromBase64String(d) : null;
            LastModified = lastModified;
            this.contentDownloader = contentDownloader;
        }

        public bool Exists => content is RepositoryContent;

        public long Length => content?.Size ?? 0;

        public string PhysicalPath => null;

        public string Path => content?.Path;

        public string Name => content?.Name;

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory => content is RepositoryContent r && r.Type.Value is ContentType c &&
            (c == ContentType.Dir || c == ContentType.Submodule);

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> CreateReadStreamAsync()
        {
            if (base64Data is byte[])
            {
                return Task.FromResult<Stream>(new MemoryStream(base64Data, writable: false));
            }

            if (content?.DownloadUrl is string url)
            {
                return contentDownloader.GetStreamAsync(url);
            }

            return Task.FromResult<Stream>(null);
        }
    }
}
