using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class ReIndexModel
    {
        public bool content
        {
            get; set;
        }
        public bool media
        {
            get; set;
        }
        public bool members
        {
            get; set;
        }
    }
}
