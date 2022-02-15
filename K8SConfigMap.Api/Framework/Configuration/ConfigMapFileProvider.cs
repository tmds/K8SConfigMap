using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System.IO;

namespace K8SConfigMap.Api.Framework.Configuration
{
    /// <summary>
    /// Simple <see cref="IFileProvider"/> implementation using config maps as source. Config maps volumes in Linux/Kubernetes are implemented as symlink files.
    /// Once reloaded their Last Modified date does not change. This implementation uses a check sum of the content to verify if the file has changed or not. The
    /// Watch method is called multiple times per file change.
    /// </summary>
    public class ConfigMapFileProvider : IFileProvider
    {
        private const int DetectChangeIntervalMilliseconds = 5000;
        private readonly string _fullPath;
        private ConfigMapFileProviderChangeToken _watcher;
        private readonly object _lock = new();

        public ConfigMapFileProvider(string fullPath)
        {
            _fullPath = fullPath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new PhysicalDirectoryContents(_fullPath);
        }

        public IFileInfo GetFileInfo(string filename)
        {
            return new PhysicalFileInfo(new FileInfo(Path.Combine(_fullPath, filename)));
        }

        public IChangeToken Watch(string filename)
        {
            lock (_lock)
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                }

                _watcher = new ConfigMapFileProviderChangeToken(Path.Combine(_fullPath, filename), DetectChangeIntervalMilliseconds);
                _watcher.Start();

                return _watcher;
            }
        }
    }
}
