using System.Collections.Generic;
using System.Threading.Tasks;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{    
    /// Converts data structures into file format for saving
    public interface IDataRegistry
    {
        Task<List<Variable>> GetAvailableVariables();
        Task<AvailableVariable> IsAvailable(string variableName, string variableMethod);
    }
}
