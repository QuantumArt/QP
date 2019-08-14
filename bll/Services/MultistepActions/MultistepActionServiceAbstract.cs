using System;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
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

        MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, bool isArchive);

        MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal, bool isArchive);

        MultistepActionStepResult Step(int stage, int step);

        void TearDown();

        IMultistepActionSettings MultistepActionSettings(int parentId, int id);

        IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids);

        IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids, bool isArchive);

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
        protected static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public virtual MessageResult PreAction(int parentId, int id) => null;

        public virtual MessageResult PreAction(int parentId, int id, int[] ids) => null;

        public virtual MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
        {
            if (HasAlreadyRun())
            {
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);
            }

            var context = CreateContext(parentId, id, boundToExternal);
            HttpContext.Session.SetValue(ContextSessionKey, context);
            return CreateActionSettings(parentId, id);
        }
        public virtual MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal) => throw new NotImplementedException();

        public virtual MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, bool isArchive) => throw new NotImplementedException();

        public virtual MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal, bool isArchive) => throw new NotImplementedException();

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
            var context = HttpContext.Session.GetValue<MultistepActionServiceContext>(ContextSessionKey);
            var command = CreateCommand(context.CommandStates[stage]);
            return command.Step(step);
        }

        public virtual void TearDown()
        {
            HttpContext.Session.Remove(ContextSessionKey);
        }

        protected abstract MultistepActionSettings CreateActionSettings(int parentId, int id);

        protected abstract MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal);

        protected abstract string ContextSessionKey { get; }

        protected abstract IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state);

        protected bool HasAlreadyRun() => HttpContext.Session.HasKey(ContextSessionKey);

        public BllObject ReadObjectProperties(int objectId) => ObjectRepository.GetObjectPropertiesById(objectId);

        public PageTemplate ReadTemplateProperties(int templateId) => PageTemplateRepository.GetPageTemplatePropertiesById(templateId);

        public virtual IMultistepActionSettings MultistepActionSettings(int parentId, int id) => null;

        public virtual IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids) => null;

        public virtual IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids, bool isArchive) => null;
    }
}
