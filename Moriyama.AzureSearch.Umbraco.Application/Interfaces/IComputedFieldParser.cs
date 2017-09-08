using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IComputedFieldParser
    {
        object GetValue(IContentBase content);
    }
}
