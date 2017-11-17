using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.SharedLogic
{
    /// <summary>
    /// Определяет статус Custom Action
    /// </summary>
    internal static class CustomActionResolver
    {
        public static IEnumerable<BackendActionStatus> ResolveStatus(string entityTypeCode, int entityId, int parentEntityId, BackendActionStatus[] statuses)
        {
            var testEntityTypeCode = entityTypeCode != EntityTypeCode.VirtualArticle ? entityTypeCode : EntityTypeCode.VirtualContent;
            var testEntityId = entityTypeCode != EntityTypeCode.VirtualArticle ? entityId : parentEntityId;

            // Получить родительский контент и сайт
            IEnumerable<EntityInfo> bindableParentEntities =
                EntityObjectRepository.GetParentsChain(testEntityTypeCode, testEntityId)
                    .Where(ei =>
                        ei.Id > 0 &&
                        (
                            ei.Code.Equals(EntityTypeCode.Site, StringComparison.InvariantCultureIgnoreCase)
                            || ei.Code.Equals(EntityTypeCode.Content, StringComparison.InvariantCultureIgnoreCase)
                            || ei.Code.Equals(EntityTypeCode.VirtualContent, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                    .ToArray();

            var customActions = CustomActionRepository.GetListByCodes(statuses.Select(s => s.Code).ToArray());

            // Если есть как минимум сайт - то проверяем							
            if (customActions.Any())
            {
                if (bindableParentEntities.Any())
                {
                    var parentSiteInfo = bindableParentEntities.FirstOrDefault(ei => ei.Code.Equals(EntityTypeCode.Site, StringComparison.InvariantCultureIgnoreCase));
                    var parentContentInfo = bindableParentEntities.FirstOrDefault(ei => ei.Code.Equals(EntityTypeCode.Content, StringComparison.InvariantCultureIgnoreCase)
                        || ei.Code.Equals(EntityTypeCode.VirtualContent, StringComparison.InvariantCultureIgnoreCase)
                    );

                    foreach (var ca in customActions)
                    {
                        var status = statuses.Single(s => s.Code.Equals(ca.Action.Code, StringComparison.InvariantCultureIgnoreCase));
                        if (status.Visible)
                        {
                            var visibleBySite = false;
                            var visibleByContent = false;

                            if (ca.Sites.Any(s => s.Id == parentSiteInfo.Id)) // сайт выбран для текущего Custom Action
                            {
                                visibleBySite = ca.SiteExcluded;
                            }
                            else
                            {
                                visibleBySite = !ca.SiteExcluded;
                            }

                            if (visibleBySite)
                            {
                                if (parentContentInfo != null)
                                {
                                    if (ca.Contents.Any(c => c.Id == parentContentInfo.Id)) // контент выбран для текущего Custom Action						
                                    {
                                        visibleByContent = ca.ContentExcluded;
                                    }

                                    //else if (!ca.Contents.Any())
                                    //    visibleByContent = true;
                                    else
                                    {
                                        visibleByContent = !ca.ContentExcluded;
                                    }
                                }
                                else
                                {
                                    visibleByContent = visibleBySite;
                                }
                            }

                            // Побеждает всегда False если он есть
                            status.Visible = visibleBySite && visibleBySite == visibleByContent;

                            if (status.Visible && ca.Action.ExcludeCodes != null)
                            {
                                foreach (var code in ca.Action.ExcludeCodes)
                                {
                                    var excludedStatus = statuses.SingleOrDefault(s => s.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));
                                    if (excludedStatus != null)
                                    {
                                        excludedStatus.Visible = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var ca in customActions)
                    {
                        var status = statuses.Single(s => s.Code.Equals(ca.Action.Code, StringComparison.InvariantCultureIgnoreCase));
                        status.Visible = false;
                    }
                }
            }

            return statuses;
        }

        /// <summary>
        /// Определяет, можно ли выполнить Custom Action
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static bool CanExecute(int entityId, CustomAction customAction) => ResolveStatus(customAction.Action.EntityType.Code, entityId, 0, new[]
            {
                new BackendActionStatus { Code = customAction.Action.Code, Visible = true }
            })
            .Single().Visible;

        /// <summary>
        /// Возвращает коды тех Action которые можно выполнить
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static IEnumerable<string> CanExecuteFilter(string entityTypeCode, int entityId, int parentEntityId, IEnumerable<string> actionCodes)
        {
            return ResolveStatus(entityTypeCode, entityId, parentEntityId,
                    actionCodes.Select(ac => new BackendActionStatus { Code = ac, Visible = true }).ToArray()
                )
                .Where(s => s.Visible)
                .Select(s => s.Code)
                .ToArray();
        }
    }
}
