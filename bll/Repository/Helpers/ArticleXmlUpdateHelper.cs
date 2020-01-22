using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public class ArticleXmlUpdateHelper : IArticleUpdateService
    {
        public Article Article { get; set; }


        private XDocument GetXmlQuery()
        {
            var xmlResult = new XDocument(new XElement("items"));
            var lmb = QPContext.CurrentUserId != 0 ? QPContext.CurrentUserId : Article.LastModifiedBy;
            var xitem = new XElement("item",
                new XAttribute("id", Article.Id),
                new XAttribute("content_id", Article.ContentId),
                new XAttribute("last_modified_by", lmb),
                new XAttribute("status_type_id", Article.StatusTypeId),
                new XAttribute("archive", Article.Archived ? 1 : 0),
                new XAttribute("visible", Article.Visible ? 1 : 0),
                new XAttribute("delayed", Article.Delayed),
                new XAttribute("cancel_split", Article.CancelSplit),
                new XAttribute("permanent_lock", Article.PermanentLock),
                (Article.UniqueId != null) ? new XAttribute("unique_id", Article.UniqueId) : null
            );
            xmlResult.Root?.Add(xitem);


            foreach (var item in Article.FieldValues)
            {
                var xdata = new XElement("data", new XAttribute("field_id", item.Field.Id));
                xitem.Add(xdata);

                var value = item.Value ?? "";

                if (item.Field.Type.Name == FieldTypeName.DynamicImage)
                {
                    value = GetDynamicImageData(item.Field);
                }
                else if (item.Field.Type.Name == FieldTypeName.Numeric)
                {
                    value = Converter.ToDbNumericString(value);
                }
                else if (new[] {FieldTypeName.DateTime, FieldTypeName.Date, FieldTypeName.Time}.Contains(item.Field.Type.Name))
                {
                    value = Converter.ToDbDateTimeString(value);
                }

                xdata.Add(value);

            }

            return xmlResult;

        }


        private string GetDynamicImageData(Field field)
        {
            if (field.DynamicImage == null)
            {
                return string.Empty;
            }

            var baseFieldValue = Article.FieldValues.Single(n => n.Field.Id == field.BaseImageId);
            return field.DynamicImage.GetValue(baseFieldValue.Value);
        }

        public Article Update()
        {
            int id = 0;
            using (new QPConnectionScope())
            {
                Common.PersistArticle(QPConnectionScope.Current.DbConnection, QPContext.CurrentCustomerCode, GetXmlQuery().ToString(), out id);
            }

            Article = ArticleRepository.GetById(id);
            return Article;
        }
    }
}
