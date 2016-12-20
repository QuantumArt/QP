using System.Data;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class EntityInfo
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public long? ParentId { get; set; }

        public bool IsFolder { get; set; }

        public string Title { get; set; }

        public string ActionCode { get; set; }

        public string EntityTypeName { get; set; }

        public static EntityInfo Create(DataRow row)
        {
            var info = new EntityInfo
            {
                Id = Converter.ToInt64(row["ID"]),
                Code = row["CODE"].ToString(),
                ParentId = Converter.ToNullableInt64(row["PARENT_ID"]),
                IsFolder = Converter.ToBoolean(row["IS_FOLDER"]),
                Title = row["TITLE"].ToString(),
                ActionCode = row["ACTION_CODE"].ToString(),
                EntityTypeName = row["NAME"].ToString()
            };

            return info;
        }
    }
}
