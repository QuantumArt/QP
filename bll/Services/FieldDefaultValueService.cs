using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using System.Web;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
    public interface IFieldDefaultValueService
    {
        /// <summary>
        /// PreAction
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        MessageResult PreAction(int fieldId);
        /// <summary>
        /// Setup
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        MultistepActionSettings SetupAction(int contentId, int fieldId);
        /// <summary>
        /// TearDown
        /// </summary>
        void TearDown();
        /// <summary>
        /// Step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        MultistepActionStepResult Step(int step);
    }

    [Serializable]
    public class FieldDefaultValueContext
    {
        public int[] ProcessedContentItemIds { get; set; }
        public int ContentId { get; set; }
        public int FieldId { get; set; }
        public bool IsBlob { get; set; }
        public int[] DefaultArticles { get; set; }
        public bool IsM2M { get; set; }
        public bool Symmetric { get; set; }
    }

    public class FieldDefaultValueService : IFieldDefaultValueService
    {
        private static readonly int ITEMS_PER_STEP = 20;
        private static readonly string SESSION_KEY = "FieldDefaultValueService.ProcessingContext";

        

        #region IFieldDefaultValueService Members

        public MessageResult PreAction(int fieldId)
        {
            if (HasAlreadyRun())
                return MessageResult.Info(MultistepActionStrings.ActionHasAlreadyRun);
            return !IsDefaultValueDefined(fieldId) ? MessageResult.Info(FieldStrings.DefaultValueIsNotDefined) : null;
        }


        public MultistepActionSettings SetupAction(int contentId, int fieldId)
        {
            var field = FieldRepository.GetById(fieldId);
            if (field == null)
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, fieldId));
            var contentItemIdsToProcess = FieldDefaultValueRepository.GetItemIdsToProcess(contentId, fieldId, field.DefaultValue, field.IsBlob, field.ExactType == FieldExactTypes.M2MRelation);
            var itemIdsToProcess = contentItemIdsToProcess as int[] ?? contentItemIdsToProcess.ToArray();
            var itemCount = itemIdsToProcess.Length;
            var stepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP);

            var context = new FieldDefaultValueContext 
            { 
                ProcessedContentItemIds = itemIdsToProcess.ToArray(),
                ContentId = contentId,
                FieldId = fieldId,
                IsBlob = field.IsBlob,
                IsM2M = field.ExactType == FieldExactTypes.M2MRelation,
                DefaultArticles = field.DefaultArticleIds.ToArray(),
                Symmetric = field.ContentLink.Symmetric
            };
            HttpContext.Current.Session[SESSION_KEY] = context;

            return new MultistepActionSettings
            {
                Stages = new[] 
                {
                    new MultistepStageSettings
                    {
                        Name = FieldStrings.ApplyDefaultValueStageName,
                        StepCount = stepCount,
                        ItemCount = itemCount
                    }
                }
            };
        }

        public MultistepActionStepResult Step(int step)
        {
            var context = (FieldDefaultValueContext)HttpContext.Current.Session[SESSION_KEY];
            var idsForStep = context.ProcessedContentItemIds
                .Skip(step * ITEMS_PER_STEP)
                .Take(ITEMS_PER_STEP)
                .ToArray();

            if (idsForStep.Any())
            {
                var notificationRepository = new NotificationPushRepository();
                notificationRepository.PrepareNotifications(context.ContentId, idsForStep, NotificationCode.Update);
                FieldDefaultValueRepository.SetDefaultValue(context.ContentId, context.FieldId, context.IsBlob, context.IsM2M, idsForStep, context.Symmetric);
                notificationRepository.SendNotifications();
            }

            return new MultistepActionStepResult { ProcessedItemsCount = idsForStep.Count() };
        }

        public void TearDown()
        {
            var context = (FieldDefaultValueContext)HttpContext.Current.Session[SESSION_KEY];
            ContentRepository.UpdateContentModification(context.ContentId);
            HttpContext.Current.Session.Remove(SESSION_KEY);
        }
        #endregion

        /// <summary>
        /// Задано ли значение по умолчанию для поля 
        /// </summary>
        /// <returns></returns>
        private bool IsDefaultValueDefined(int fieldId)
        {
            var field = FieldRepository.GetById(fieldId);
            if (field == null)
                throw new Exception(string.Format(FieldStrings.FieldNotFound, fieldId));
            var result = !string.IsNullOrEmpty(field.Default);
            return result;
        }

        /// <summary>
        /// Проверяет, запущено ли уже действие?
        /// </summary>
        /// <returns></returns>
        bool HasAlreadyRun()
        {
            return HttpContext.Current.Session[SESSION_KEY] != null;
        }

    }
}
