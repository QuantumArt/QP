using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Quantumart.QP8.BLL.Services
{
    public class RecreateDynamicImagesService : IRecreateDynamicImagesService
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const int ItemsPerStep = 20;

        public MultistepActionSettings SetupAction(int contentId, int fieldId)
        {
            if (HasAlreadyRun())
            {
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);
            }

            var flds = GetFields(fieldId);
            if (flds.Item1.BaseImageId != null)
            {
                var articleData = RecreateDynamicImagesRepository.GetDataToProcess(flds.Item1.BaseImageId.Value);
                var dataRows = articleData as DataRow[] ?? articleData.ToArray();
                var context = new RecreateDynamicImagesContext
                {
                    FieldId = fieldId,
                    ContentId = contentId,
                    ArticleData = dataRows.Select(r => Tuple.Create(Converter.ToInt32(r["ID"]), r.Field<string>("DATA"))).ToList(),
                };

                HttpContext.Session.SetValue(HttpContextSession.RecreateDynamicImagesServiceProcessingContext, context);

                var itemCount = dataRows.Length;
                var stepCount = MultistepActionHelper.GetStepCount(itemCount, ItemsPerStep);
                var result = new MultistepActionSettings();
                result.Stages.Add(new MultistepStageSettings
                    {
                        Name = FieldStrings.RecreateDynamicImagesStageName,
                        StepCount = stepCount,
                        ItemCount = itemCount
                    }
                );
                return result;
            }

            return null;
        }

        public MultistepActionStepResult Step(int step, PathHelper pathHelper)
        {
            var context = HttpContext.Session.GetValue<RecreateDynamicImagesContext>(HttpContextSession.RecreateDynamicImagesServiceProcessingContext);
            var currentProcessedImages = new HashSet<string>(context.ProcessedImages);
            var dataForStep = context.ArticleData.Skip(step * ItemsPerStep).Take(ItemsPerStep).ToArray();

            var flds = GetFields(context.FieldId);
            var dimg = flds.Item1.DynamicImage;
            var baseImagePathInfo = flds.Item2.PathInfo;

            if (dataForStep.Any())
            {
                var ids = dataForStep.Select(d => d.Item1).ToArray();
                var notificationRepository = new NotificationPushRepository { IgnoreInternal = true };
                notificationRepository.PrepareNotifications(context.ContentId, ids, NotificationCode.Update);

                foreach (var dfs in dataForStep)
                {
                    using (var transaction = QPConfiguration.CreateTransactionScope(IsolationLevel.ReadCommitted))
                    {
                        var newValue = dimg.GetValue(dimg.GetDesiredFileName(dfs.Item2));
                        RecreateDynamicImagesRepository.UpdateDynamicFieldValue(dimg.Field.Id, dfs.Item1, newValue);

                        if (!currentProcessedImages.Contains(dfs.Item2))
                        {
                            dimg.CreateDynamicImage(baseImagePathInfo.GetPath(dfs.Item2), dfs.Item2, pathHelper);
                            currentProcessedImages.Add(dfs.Item2);
                            context.ProcessedImages.Add(dfs.Item2);
                        }
                        transaction.Complete();
                    }
                }

                notificationRepository.SendNotifications();
            }

            return new MultistepActionStepResult { ProcessedItemsCount = dataForStep.Count() };
        }

        public void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.RecreateDynamicImagesServiceProcessingContext);
        }

        private static bool HasAlreadyRun() => HttpContext.Session.HasKey(HttpContextSession.RecreateDynamicImagesServiceProcessingContext);

        private static Tuple<Field, Field> GetFields(int fieldId)
        {
            var field = FieldRepository.GetById(fieldId);
            if (field == null)
            {
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, fieldId));
            }

            if (field.TypeId != FieldTypeCodes.DynamicImage)
            {
                throw new ApplicationException(string.Format(FieldStrings.FieldIsNotDynamicImage, fieldId));
            }

            if (field.BaseImageId != null)
            {
                var basefield = FieldRepository.GetById(field.BaseImageId.Value);
                if (basefield == null)
                {
                    throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, field.BaseImageId.Value));
                }

                return Tuple.Create(field, basefield);
            }

            return null;
        }
    }
}
