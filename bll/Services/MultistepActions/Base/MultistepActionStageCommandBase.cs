using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Base
{
	public abstract class MultistepActionStageCommandBase : IMultistepActionStageCommand
	{
		private const string ContextSessionKey = "MultistepActionStageComman.Settings";

		public int ContentId { get; private set; }
		public int ItemCount { get; private set; }
		public int[] Ids { get; private set; }
		public bool? BoundToExternal { get; private set; }
		public int ItemsPerStep { get; private set; }
		public List<MessageResult> Messages { get; private set; }

		public MultistepActionStageCommandBase()
		{
			Messages = (List<MessageResult>)HttpContext.Current.Session[ContextSessionKey] ?? new List<MessageResult>();
		}

		public MultistepActionStageCommandBase(int contentId, int itemCount, int[] ids)
			: base()
        {
            ContentId = contentId;
            Ids = ids;
            ItemCount = itemCount;
        }

		public void Initialize(MultistepActionStageCommandState state)
		{
			ContentId = state.ParentId;
			ItemCount = state.Ids.Length;
			Ids = state.Ids;
			BoundToExternal = state.BoundToExternal;
			ItemsPerStep = state.ItemsPerStep;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				ParentId = ContentId,
				Id = 0,
				Ids = Ids,
				BoundToExternal = BoundToExternal,
				ItemsPerStep = ItemsPerStep,
			};
		}

		public MultistepStageSettings GetStageSettings()
		{			
			return new MultistepStageSettings
			{
				ItemCount = ItemCount,
				StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
				Name = MultistepActionStrings.ResourceManager.GetString(GetType().Name)
			};
		}

		public MultistepActionStepResult Step(int step)
		{
			var ids = Ids.Skip(step * ItemsPerStep).Take(ItemsPerStep).ToArray();
			var result = Step(ids);

			if (result != null)
			{
				Messages.Add(result);
				HttpContext.Current.Session[ContextSessionKey] = Messages;
			}

			string additionalInfo = null;

			if (Messages.Any())
			{
				additionalInfo = string.Join("; ", Messages.Select(m => m.Text));
			}
						
			return new MultistepActionStepResult() { ProcessedItemsCount = ids.Length, AdditionalInfo = additionalInfo };
		}

		protected abstract MessageResult Step(int[] ids);

		public static void TearDown()
		{
			HttpContext.Current.Session.Remove(ContextSessionKey);
		}
	}
}
