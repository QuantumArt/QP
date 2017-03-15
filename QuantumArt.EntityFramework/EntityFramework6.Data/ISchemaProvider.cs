using Quantumart.QP8.CodeGeneration.Services;

namespace Quantumart.QP8.EntityFramework6.Data
{
    public interface ISchemaProvider
    {
        ModelReader GetSchema();
        object GetCacheKey();
    }
}