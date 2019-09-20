using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SharpScss;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebOptimizer.Sass
{
    /// <summary>
    /// Compiles Sass files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class Compiler : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context) => string.Empty;

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)context.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = context.Asset.GetFileProvider(env);

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = fileProvider.GetFileInfo(route);

                var settings = new ScssOptions { InputFile = file.PhysicalPath };
                settings.TryImport = 
                    (string file, string parentPath, out string scss, out string map) => 
                    { 
                        // System.Console.WriteLine("File to import is " + file);
                        // System.Console.WriteLine("Parent Path is " + parentPath);
                        var basePath = "/styles/";
                        var path = GetFilePath(file, parentPath);
                        var f = fileProvider.GetFileInfo(basePath + path + ".scss");
                        using (var reader = new System.IO.StreamReader(f.CreateReadStream(), System.Text.Encoding.UTF8))
                        {
                            string value = reader.ReadToEnd();
                            // System.Console.WriteLine("SCSS is " + value);
                            scss = value;
                            map = null;
                        }
                        return true;
                    };

                System.Console.WriteLine("FP " + fileProvider.GetType().Name);
                System.Console.WriteLine("Route is " + route);
                System.Console.WriteLine("File is " + file.PhysicalPath);
                System.Console.WriteLine("Settings: " + settings.IncludePaths);

                ScssResult result = Scss.ConvertToCss(context.Content[route].AsString(), settings);

                content[route] = result.Css.AsByteArray();
            }

            context.Content = content;

            foreach (string key in context.Content.Keys)
            {
                IFileInfo input = fileProvider.GetFileInfo(key);
                IFileInfo output = fileProvider.GetFileInfo(context.Asset.Route);

                System.Console.WriteLine("Input is " + input.Name + " + " + input.PhysicalPath);
                System.Console.WriteLine("Output is " + output.Name + " + " + output.PhysicalPath);
                string absoluteOutputPath = new System.IO.FileInfo(output.PhysicalPath).FullName;
                System.Console.WriteLine("Output actual path is " + absoluteOutputPath);
            }


            return Task.CompletedTask;
        }

        private string GetFilePath(string fileName, string parentFileName) {
            if (!fileName.Contains("../")) {
                return fileName;
            }
            // The file name is relative.
            var directories = parentFileName.Split("/");
            if (directories.Length == 1) {
                return ""; // Cannot go any higher. Is not valid!
            }
            return fileName.Substring(3,fileName.Length-3);
        }
    }
}
