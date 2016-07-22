using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.DataContext
{
    public interface IQPFormService
    {
        string GetFormNameByNetNames(string netContentName, string netFieldName);
        string ReplacePlaceholders(string text);
    }
}