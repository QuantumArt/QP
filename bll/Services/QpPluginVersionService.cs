using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class QpPluginVersionService
    {
        private static void Exchange(ref int id1, ref int id2)
        {
            var temp = id1;
            id1 = id2;
            id2 = temp;
        }

        private static Tuple<int, int> GetOrderedIds(int[] ids)
        {
            var id1 = ids[0];
            var id2 = ids[1];

            if (id1 > id2 && id2 != ArticleVersion.CurrentVersionId)
            {
                Exchange(ref id1, ref id2);
            }

            return Tuple.Create(id1, id2);
        }

        public static QpPluginVersion Read(int id, int pluginId = 0)
        {
            var result = QpPluginVersionRepository.GetById(id, pluginId);
            if (result == null)
            {
                throw new Exception(string.Format(QpPluginStrings.PluginVersionNotFound, id));
            }
            return result;
        }


        public static ListResult<QpPluginVersionListItem> List(ListCommand command, int pluginId)
        {
            command.SortExpression = EntityObject.TranslateSortExpression(command.SortExpression);
            var list = QpPluginVersionRepository.List(pluginId, command, out var totalRecords);
            return new ListResult<QpPluginVersionListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public static QpPluginVersion GetMergedVersion(int[] ids, int parentId)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            if (ids.Length != 2)
            {
                throw new ArgumentException("Wrong ids length");
            }

            var parent = QpPluginRepository.GetById(parentId);
            if (parent == null)
            {
                throw new Exception(string.Format(QpPluginStrings.PluginNotFound, parentId));
            }

            var (item1, item2) = GetOrderedIds(ids);
            QpPluginVersion version1, version2;

            version1 = QpPluginVersionRepository.GetById(item1, parentId);
            if (version1 == null)
            {
                throw new Exception(string.Format(QpPluginStrings.PluginVersionNotFound, item1, parentId));
            }

            if (item2 == QpPluginVersion.CurrentVersionId)
            {
                version2 = CreateVersionFromPlugin(parent);
                version2.Id = QpPluginVersion.CurrentVersionId;
            }
            else
            {
                version2 = QpPluginVersionRepository.GetById(item2, parentId);
                if (version2 == null)
                {
                    throw new Exception(string.Format(QpPluginStrings.PluginVersionNotFound, item2, parentId));
                }
            }

            version1.MergeToVersion(version2);
            return version1;
        }

        public static QpPluginVersion CreateVersionFromPlugin(QpPlugin parent) =>
            new QpPluginVersion
            {
                Plugin = parent,
                PluginId = parent.Id,
                Contract = parent.Contract,
                Modified = parent.Modified,
                LastModifiedBy = parent.LastModifiedBy,
                LastModifiedByUser = parent.LastModifiedByUser
            };
 }
}
