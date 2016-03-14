using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using System.Web;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Helpers;

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
			else if (!IsDefaultValueDefined(fieldId))
				return MessageResult.Info(FieldStrings.DefaultValueIsNotDefined);
			else
				return null;
		}


		public MultistepActionSettings SetupAction(int contentId, int fieldId)
		{
			Field field = FieldRepository.GetById(fieldId);
			if (field == null)
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, fieldId));
			IEnumerable<int> contentItemIdsToProcess = FieldDefaultValueRepository.GetItemIdsToProcess(contentId, fieldId, field.DefaultValue, field.IsBlob, field.ExactType == Constants.FieldExactTypes.M2MRelation);
			int itemCount = contentItemIdsToProcess.Count();
			int stepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP);

			FieldDefaultValueContext context = new FieldDefaultValueContext 
			{ 
				ProcessedContentItemIds = contentItemIdsToProcess.ToArray(),
				ContentId = contentId,
				FieldId = fieldId,
				IsBlob = field.IsBlob,
				IsM2M = field.ExactType == Constants.FieldExactTypes.M2MRelation,
				DefaultArticles = field.DefaultArticleIds.ToArray(),
				Symmetric = field.ContentLink.Symmetric
			};
			HttpContext.Current.Session[SESSION_KEY] = context;

			return new MultistepActionSettings
			{
				Stages = new MultistepStageSettings[] 
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
			FieldDefaultValueContext context = (FieldDefaultValueContext)HttpContext.Current.Session[SESSION_KEY];
			IEnumerable<int> idsForStep = context.ProcessedContentItemIds
				.Skip(step * ITEMS_PER_STEP)
				.Take(ITEMS_PER_STEP)
				.ToArray();

			if (idsForStep.Any())
			{
				FieldDefaultValueRepository.SetDefaultValue(context.ContentId, context.FieldId, context.IsBlob, context.IsM2M, idsForStep, context.Symmetric);
			}

			return new MultistepActionStepResult { ProcessedItemsCount = idsForStep.Count() };
		}

		public void TearDown()
		{
			FieldDefaultValueContext context = (FieldDefaultValueContext)HttpContext.Current.Session[SESSION_KEY];
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
			Field field = FieldRepository.GetById(fieldId);
			if (field == null)
				throw new Exception(String.Format(FieldStrings.FieldNotFound, fieldId));
			bool result = !String.IsNullOrEmpty(field.Default);
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
