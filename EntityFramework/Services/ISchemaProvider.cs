using Quantumart.QP8.CodeGeneration.Services;

namespace Quantumart.QP8.EntityFramework.Services
{
    public interface ISchemaProvider
    {
        ModelReader GetSchema();
        object GetCacheKey();
    }
}