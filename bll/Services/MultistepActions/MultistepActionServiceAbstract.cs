using System;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

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
            {
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);
            }

            var context = CreateContext(parentId, id, boundToExternal);
            HttpContext.Current.Session[ContextSessionKey] = context;
            return CreateActionSettings(parentId, id);
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
            var context = (MultistepActionServiceContext)HttpContext.Current.Session[ContextSessionKey];
            var state = context.CommandStates[stage];
            var command = CreateCommand(state);
            var result = command.Step(step);
            return result;
        }

        public virtual void TearDown()
        {
            HttpContext.Current.Session.Remove(ContextSessionKey);
        }

        protected abstract MultistepActionSettings CreateActionSettings(int parentId, int id);

        protected abstract MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal);

        protected abstract string ContextSessionKey { get; }

        protected abstract IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state);

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
