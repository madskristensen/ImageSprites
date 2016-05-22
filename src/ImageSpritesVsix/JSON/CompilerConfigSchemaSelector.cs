using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.JSON.Core.Schema;

namespace ImageSpritesVsix
{
    [Export(typeof(IJSONSchemaSelector))]
    class CompilerConfigSchemaSelector : IJSONSchemaSelector
    {
        public event EventHandler AvailableSchemasChanged { add { } remove { } }

        public Task<IEnumerable<string>> GetAvailableSchemasAsync()
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public string GetSchemaFor(string fileLocation)
        {
            string extension = Path.GetExtension(fileLocation);

            if (extension.Equals(Constants.FileExtension, StringComparison.OrdinalIgnoreCase))
                return GetSchemaFileName("json\\sprite-schema.json");

            return null;
        }

        private static string GetSchemaFileName(string relativePath)
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            string folder = Path.GetDirectoryName(assembly);
            return Path.Combine(folder, relativePath);
        }
    }
}
