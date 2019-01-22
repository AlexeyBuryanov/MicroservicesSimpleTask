using System;
using System.IO;

namespace ManagementService.Extensions
{
    public static class UriBuilderExtensions
    {
        public static void AppendToPath(this UriBuilder builder, string pathToAdd)
        {
            var completePath = Path.Combine(builder.Path, pathToAdd);
            builder.Path = completePath;
        }
    }
}
