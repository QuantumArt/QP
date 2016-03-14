using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using System.Web;
using System.Data;
using Quantumart.QP8.Utils;
using System.Transactions;
using Quantumart.QP8.BLL.Repository.Articles;
using System.IO;
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

			IEnumerable<DataRow> articleData = RecreateDynamicImagesRepository.GetDataToProcess(flds.Item1.BaseImageId.Value);
			RecreateDynamicImagesContext context = new RecreateDynamicImagesContext
			{
				FieldId = fieldId,
				ContentId = contentId,
				ArticleData = articleData
					.Select(r => Tuple.Create(Converter.ToInt32(r.Field<decimal>("ID")), r.Field<string>("DATA")))
					.ToArray(),
				ProcessedImages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)				

			};
			HttpContext.Current.Session[SESSION_KEY] = context;

			int itemCount = articleData.Count();
			int stepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP);

			return new MultistepActionSettings
			{
				Stages = new MultistepStageSettings[] 
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

		public MultistepActionStepResult Step(int step)
		{
			RecreateDynamicImagesContext context = (RecreateDynamicImagesContext)HttpContext.Current.Session[SESSION_KEY];
			IEnumerable<Tuple<int, string>> dataForStep = context.ArticleData
				.Skip(step * ITEMS_PER_STEP)
				.Take(ITEMS_PER_STEP)
				.ToArray();

			var flds = GetFields(context.FieldId);
			DynamicImage dimg = flds.Item1.DynamicImage;
			PathInfo baseImagePathInfo = flds.Item2.PathInfo;
			
			if (dataForStep.Any())
			{
				foreach (var d in dataForStep)
				{
					using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
					{
						string newValue = dimg.GetValue(dimg.GetDesiredFileName(d.Item2));
						RecreateDynamicImagesRepository.UpdateDynamicFieldValue(dimg.Field.Id, d.Item1, newValue);

						if (!context.ProcessedImages.Contains(d.Item2))
						{
							dimg.CreateDynamicImage(baseImagePathInfo.GetPath(d.Item2), d.Item2);
							context.ProcessedImages.Add(d.Item2);
						}
						transaction.Complete();
					}
				}
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
			Field field = FieldRepository.GetById(fieldId);
			if (field == null)
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, fieldId));
			if (field.TypeId != FieldTypeCodes.DynamicImage)
				throw new ApplicationException(String.Format(FieldStrings.FieldIsNotDynamicImage, fieldId));

			Field basefield = FieldRepository.GetById(field.BaseImageId.Value);
			if (basefield == null)
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, field.BaseImageId.Value));

			return Tuple.Create(field, basefield);
		}
	}
}
