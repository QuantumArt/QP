using System.Xml.Linq;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class XElementExtensions
    {
        public static XElement RemoveDescendants(this XElement elem)
        {
            var xmlElement = new XElement(elem);
            xmlElement.RemoveNodes();
            return xmlElement;
        }
    }
}
