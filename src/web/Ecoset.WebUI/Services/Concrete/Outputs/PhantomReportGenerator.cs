using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Services.Concrete
{
    public class PhantomReportGenerator : IReportGenerator
    {
        private IOptions<PhantomOptions> _phantomOptions;
        private IOutputPersistence _persistence;
        private IHttpContextAccessor _context;

        public PhantomReportGenerator(IOptions<PhantomOptions> options, IOutputPersistence persistence, IHttpContextAccessor httpContextAccessor) 
        {
            _phantomOptions = options;
            _persistence = persistence;
            _context = httpContextAccessor;
        }

        public string GenerateReport(Job job)
        {
            var fileName = _persistence.GetReport(job.Id);
            if (string.IsNullOrEmpty(fileName)) 
            {
                var cache = _phantomOptions.Value.LocalCacheDirectory;
                var cacheFile = cache + job.JobProcessorReference + ".pdf";
                var port = _phantomOptions.Value.Port;
                using (Process phantomProcess = Process.Start(_phantomOptions.Value.PhantomJsPath, "pdf/rasterise.js http://localhost:" + port + "/Analysis/Home/GenerateReport?id=" + job.Id + " " + cacheFile + " 29.7cm*21cm"))
                {
                    phantomProcess.WaitForExit();
                }
                fileName = _persistence.PersistReport(job.Id, cacheFile);
            }
            return fileName;
        }
    }
}
