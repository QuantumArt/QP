using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Models
{
    public interface IQPFormService
    {
        string GetFormNameByNetNames(string netContentName, string netFieldName);

        string ReplacePlaceholders(string text);
    }
}
