using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Removing;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.MultistepActions.CopySite;

namespace Quantumart.QP8.BLL.Services.MultistepActions
{
	/// <summary>
	/// Интерфейс сервиса удаления данных
	/// </summary>
	public interface IMultistepActionService
	{
		MessageResult PreAction(int parentId, int id);
		MessageResult PreAction(int parentId, int id, int[] ids);
		MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal);
		MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal);
        MultistepActionStepResult Step(int stage, int step);
        void TearDown();

        IMultistepActionSettings MultistepActionSettings(int parentId, int id);
		IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids);

        void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams);
        void SetupWithParams(int parentId, int[] ids, IMultistepActionParams settingsParams);

		BllObject ReadObjectProperties(int parentId);

		PageTemplate ReadTemplateProperties(int parentId);
	}

	/// <summary>
	/// Абстрактный класс сервиса удаления данных
	/// </summary>
	public abstract class MultistepActionServiceAbstract : IMultistepActionService
	{
		#region IRemovingService Members

		public virtual MessageResult PreAction(int parentId, int id)
		{
			return null;
		}

		public virtual MessageResult PreAction(int parentId, int id, int[] ids)
		{
			return null;
		}

		public virtual MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
		{
			if (HasAlreadyRun())
				throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);

			MultistepActionServiceContext context = CreateContext(parentId, id, boundToExternal);
			HttpContext.Current.Session[ContextSessionKey] = context;

			MultistepActionSettings settings = CreateActionSettings(parentId, id);
			return settings;
		}

		public virtual MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal)
        {
            throw new NotImplementedException();
        }

        public virtual void SetupWithParams(int parentId, int[] ids, IMultistepActionParams settingsParams)
        {
            throw new NotImplementedException();
        }

        public virtual void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            throw new NotImplementedException();
        }


		public MultistepActionStepResult Step(int stage, int step)
		{
			MultistepActionServiceContext context = (MultistepActionServiceContext)HttpContext.Current.Session[ContextSessionKey];
			MultistepActionStageCommandState state = context.CommandStates[stage];
			IMultistepActionStageCommand command = CreateCommand(state);
			MultistepActionStepResult result = command.Step(step);
			return result;
		}

		public virtual void TearDown()
		{
			HttpContext.Current.Session.Remove(ContextSessionKey);
		}

		#endregion

		#region Abstact
		protected abstract MultistepActionSettings CreateActionSettings(int parentId, int id);
		protected abstract MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal);
		protected abstract string ContextSessionKey { get; }
		protected abstract IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state);
		#endregion

		protected bool HasAlreadyRun()
		{
			return HttpContext.Current.Session[ContextSessionKey] != null;
		}

		public BllObject ReadObjectProperties(int objectId)
		{
			return ObjectRepository.GetObjectPropertiesById(objectId);
		}

		public PageTemplate ReadTemplateProperties(int templateId)
		{
			return PageTemplateRepository.GetPageTemplatePropertiesById(templateId);
		}

        public virtual IMultistepActionSettings MultistepActionSettings(int parentId, int id)
        {
            return null;
        }

        public virtual IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids)
        {
            return null;
        }

    }
}
