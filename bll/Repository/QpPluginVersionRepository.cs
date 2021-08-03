using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository
{
    internal class QpPluginVersionRepository
    {
        internal static List<QpPluginVersion> GetList(int pluginId, ListCommand command)
        {
            var result = QPContext.EFContext
                .PluginVersionSet
                .Where(x => x.PluginId == pluginId)
                .Include(x => x.LastModifiedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(command.SortExpression))
            {
                result = result.OrderBy(command.SortExpression);
            }

            return DefaultMapper.GetBizList<QpPluginVersion, PluginVersionDAL>(result.ToList());
        }

        internal static QpPluginVersion GetById(int id, int pluginId = 0)
        {
            QpPluginVersion qpPluginVersion;

            if (id == QpPluginVersion.CurrentVersionId)
            {
                if (pluginId == 0)
                {
                    throw new Exception("Article id is not specified!");
                }

                var plugin = QpPluginRepository.GetById(pluginId);
                qpPluginVersion = new QpPluginVersion
                {
                    PluginId = pluginId,
                    Id = id,
                    Modified = plugin.Modified,
                    LastModifiedBy = plugin.LastModifiedBy,
                    LastModifiedByUser = plugin.LastModifiedByUser,
                    Plugin = plugin
                };
            }
            else
            {
                var dal = QPContext.EFContext.PluginVersionSet.Include(n => n.LastModifiedByUser).SingleOrDefault(n => n.Id == id);
                qpPluginVersion = DefaultMapper.GetBizObject<QpPluginVersion, PluginVersionDAL>(dal);
                if (qpPluginVersion != null)
                {
                    qpPluginVersion.Plugin = QpPluginRepository.GetById(qpPluginVersion.PluginId);
                }
            }

            return qpPluginVersion;
        }

        internal static void Delete(int id)
        {
            DefaultRepository.Delete<PluginVersionDAL>(id);
        }

        internal static void MultipleDelete(int[] ids)
        {
            DefaultRepository.Delete<PluginVersionDAL>(ids);
        }

        internal static QpPluginVersion GetLatest(int pluginId)
        {
            return GetById(QPContext.EFContext.PluginVersionSet.Where(n => n.PluginId == pluginId).OrderByDescending(n => n.Id).Select(n => (int)n.Id).FirstOrDefault());
        }

        internal static QpPluginVersion GetEarliest(int articleId)
        {
            return GetById(QPContext.EFContext.PluginVersionSet.Where(n => n.PluginId == articleId).OrderBy(n => n.Id).Select(n => (int)n.Id).FirstOrDefault());
        }

        internal static int GetVersionsCount(int articleId)
        {
            return QPContext.EFContext.PluginVersionSet.Count(n => n.PluginId == articleId);
        }

        internal static IEnumerable<int> GetIds(int articleId)
        {
            return QPContext.EFContext.PluginVersionSet.Where(n => n.PluginId == articleId).Select(n => (int)n.Id).ToArray();
        }

        internal static IEnumerable<int> GetIds(int[] ids)
        {
            var decIds = ids.Select(n => (decimal)n).ToArray();
            return QPContext.EFContext.ArticleVersionSet.Where(n => decIds.Contains(n.ArticleId)).Select(n => (int)n.Id).ToArray();
        }
    }
}
