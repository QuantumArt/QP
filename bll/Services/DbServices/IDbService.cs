using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.DbServices
{
    public interface IDbService
    {
        Db GetDbSettings();
        bool UseS3();

        S3Options S3Options { get; }
    }
}
