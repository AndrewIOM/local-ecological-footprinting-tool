using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Ecoset.WebUI.Utils {

    public static class Files {

        public static IFileProvider GetFileProvider(IServiceProvider serviceProvider)
        {
            var staticFileOptions = (IOptions<StaticFileOptions>)serviceProvider.GetService(typeof(IOptions<StaticFileOptions>));
            var staticFileProvider = staticFileOptions.Value.FileProvider;
            return staticFileProvider;
        }
    }
}