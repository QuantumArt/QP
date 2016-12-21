using Quantumart.QP8.CodeGeneration.Services;

namespace Quantumart.QP8.EntityFramework6.DevData
{
    public interface ISchemaProvider
    {
        ModelReader GetSchema();
        object GetCacheKey();
    }
}