﻿using System;
using System.IO;
using System.Linq;
using SpecFlow.Contrib.JsonData.SpecFlowPlugin.DataSources;

namespace SpecFlow.Contrib.JsonData.SpecFlowPlugin.Loaders
{
    public abstract class FileBasedLoader : IDataSourceLoader
    {
        private readonly string _formatName;
        private readonly string[] _acceptedExtensions;

        protected FileBasedLoader(string formatName, params string[] acceptedExtensions)
        {
            _formatName = formatName;
            _acceptedExtensions = acceptedExtensions;
        }

        public virtual bool AcceptsSourceFilePath(string sourceFilePath)
        {
            var extension = Path.GetExtension(sourceFilePath);
            return extension != null && _acceptedExtensions.Any(e => e.Equals(extension, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual DataSource LoadDataSource(string path, string sourceFilePath, string dataSet)
        {
            var filePath = ResolveFilePath(path, sourceFilePath);
            try
            {
                return LoadDataSourceFromFilePath(filePath, dataSet);
            }
            catch (ExternalDataPluginException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ExternalDataPluginException($"Unable to load {_formatName} file from '{filePath}': {ex.Message}", ex);
            }
        }

        protected abstract DataSource LoadDataSourceFromFilePath(string filePath, string dataSet);

        private string ResolveFilePath(string path, string sourceFilePath)
        {
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                string sourceDirectoryName = Path.GetDirectoryName(sourceFilePath);
                if (sourceDirectoryName != null)
                    return Path.GetFullPath(Path.Combine(sourceDirectoryName, path));
            }
            return path;
        }

        protected string ReadTextFileContent(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new ExternalDataPluginException($"Unable to load {_formatName} file from '{filePath}': {ex.Message}", ex);
            }
        }
    }
}
