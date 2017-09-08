using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ICustomFieldParser
    {
        object GetValue(IContentBase content);
    }
}
