using System.Threading.Tasks;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IReportGenerator
    {
        string GenerateReport(Job job);
    }
}
