using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using System.Web;
using System.Data;
using Quantumart.QP8.Utils;
using System.Transactions;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services
{
    public interface IRecreateDynamicImagesService
    {
        MultistepActionSettings SetupAction(int contentId, int fieldId);
        MultistepActionStepResult Step(int step);
        void TearDown();
    }

    [Serializable]
    public class RecreateDynamicImagesContext
    {
        public int ContentId { get; set; }
        public int FieldId { get; set; }
        public Tuple<int, string>[] ArticleData { get; set; }
        public HashSet<string> ProcessedImages { get; set; }
    }

    public class RecreateDynamicImagesService : IRecreateDynamicImagesService
    {
        private static readonly int ITEMS_PER_STEP = 20;
        private static readonly string SESSION_KEY = "RecreateDynamicImagesService.ProcessingContext";

        #region IRecreateDynamicImagesService Members

        public MultistepActionSettings SetupAction(int contentId, int fieldId)
        {
            if (HasAlreadyRun())
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);

            var flds = GetFields(fieldId);

            if (flds.Item1.BaseImageId != null)
            {
                var articleData = RecreateDynamicImagesRepository.GetDataToProcess(flds.Item1.BaseImageId.Value);
                var dataRows = articleData as DataRow[] ?? articleData.ToArray();
                var context = new RecreateDynamicImagesContext
                {
                    FieldId = fieldId,
                    ContentId = contentId,
                    ArticleData = dataRows
                        .Select(r => Tuple.Create(Converter.ToInt32(r.Field<decimal>("ID")), r.Field<string>("DATA")))
                        .ToArray(),
                    ProcessedImages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)				

                };
                HttpContext.Current.Session[SESSION_KEY] = context;

                var itemCount = dataRows.Length;
                var stepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP);

                return new MultistepActionSettings
                {
                    Stages = new[] 
                    {
                        new MultistepStageSettings
                        {
                            Name = FieldStrings.RecreateDynamicImagesStageName,
                            StepCount = stepCount,
                            ItemCount = itemCount
                        }
                    }
                };
            }
            return null;
        }

        public MultistepActionStepResult Step(int step)
        {
            var context = (RecreateDynamicImagesContext)HttpContext.Current.Session[SESSION_KEY];
            IEnumerable<Tuple<int, string>> dataForStep = context.ArticleData
                .Skip(step * ITEMS_PER_STEP)
                .Take(ITEMS_PER_STEP)
                .ToArray();

            var flds = GetFields(context.FieldId);
            var dimg = flds.Item1.DynamicImage;
            var baseImagePathInfo = flds.Item2.PathInfo;
            
            if (dataForStep.Any())
            {
                var ids = dataForStep.Select(d => d.Item1).ToArray();
                var notificationRepository = new NotificationPushRepository() { IgnoreInternal = true };
                notificationRepository.PrepareNotifications(context.ContentId, ids, NotificationCode.Update);

                foreach (var d in dataForStep)
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                    {
                        var newValue = dimg.GetValue(dimg.GetDesiredFileName(d.Item2));
                        RecreateDynamicImagesRepository.UpdateDynamicFieldValue(dimg.Field.Id, d.Item1, newValue);

                        if (!context.ProcessedImages.Contains(d.Item2))
                        {
                            dimg.CreateDynamicImage(baseImagePathInfo.GetPath(d.Item2), d.Item2);
                            context.ProcessedImages.Add(d.Item2);
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
            HttpContext.Current.Session.Remove(SESSION_KEY);
        }
        #endregion

        /// <summary>
        /// Проверяет, запущено ли уже действие?
        /// </summary>
        /// <returns></returns>
        private bool HasAlreadyRun()
        {
            return HttpContext.Current.Session[SESSION_KEY] != null;
        }

        private Tuple<Field, Field> GetFields(int fieldId)
        {
            var field = FieldRepository.GetById(fieldId);
            if (field == null)
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, fieldId));
            if (field.TypeId != FieldTypeCodes.DynamicImage)
                throw new ApplicationException(string.Format(FieldStrings.FieldIsNotDynamicImage, fieldId));

            if (field.BaseImageId != null)
            {
                var basefield = FieldRepository.GetById(field.BaseImageId.Value);
                if (basefield == null)
                    throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, field.BaseImageId.Value));

                return Tuple.Create(field, basefield);
            }
            return null;
        }
    }
}
