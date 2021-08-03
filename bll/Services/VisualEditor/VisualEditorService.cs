using System;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.VisualEditor
{
    public class VisualEditorService : IVisualEditorService
    {
        public InitListResult InitVisualEditorStyleList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewVisualEditorStyle) &&
                SecurityRepository.IsEntityAccessible(EntityTypeCode.VisualEditorStyle, parentId, ActionTypeCode.Update)
        };

        public InitListResult InitList(int contentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewVisualEditorPlugin) &&
                SecurityRepository.IsEntityAccessible(EntityTypeCode.VisualEditorPlugin, contentId, ActionTypeCode.Update)
        };

        public ListResult<VisualEditorPluginListItem> GetVisualEditorPlugins(ListCommand cmd, int contentId)
        {
            var list = VisualEditorRepository.List(cmd, contentId, out var totalRecords);
            return new ListResult<VisualEditorPluginListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public ListResult<VisualEditorStyleListItem> GetVisualEditorStyles(ListCommand cmd, int contentId)
        {
            var list = VisualEditorRepository.ListStyles(cmd, contentId, out var totalRecords);
            return new ListResult<VisualEditorStyleListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public VisualEditorPlugin ReadVisualEditorPluginProperties(int id)
        {
            var plugin = VisualEditorRepository.GetPluginPropertiesById(id);
            if (plugin == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorPluginNotFound, id));
            }

            return plugin;
        }

        public VisualEditorPlugin ReadVisualEditorPluginPropertiesForUpdate(int id) => ReadVisualEditorPluginProperties(id);

        public VisualEditorPlugin UpdateVisualEditorProperties(VisualEditorPlugin visualEditorPlugin) => VisualEditorRepository.UpdatePluginProperties(visualEditorPlugin);

        public MessageResult Remove(int id)
        {
            var plugin = VisualEditorRepository.GetPluginPropertiesById(id);
            if (plugin == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorPluginNotFound, id));
            }

            VisualEditorRepository.Delete(id);
            return null;
        }

        public VisualEditorPlugin NewVisualEditorPluginProperties(int parentId) => VisualEditorPlugin.Create();

        public VisualEditorPlugin NewVisualEditorPluginPropertiesForUpdate(int parentId) => NewVisualEditorPluginProperties(parentId);

        public VisualEditorPlugin SaveVisualEditorPluginProperties(VisualEditorPlugin visualEditorPlugin) => VisualEditorRepository.SavePluginProperties(visualEditorPlugin);

        public VisualEditorStyle ReadVisualEditorStyleProperties(int id)
        {
            var style = VisualEditorRepository.GetStylePropertiesById(id);
            if (style == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorStyleNotFound, id));
            }

            return style;
        }

        public VisualEditorStyle ReadVisualEditorStylePropertiesForUpdate(int id) => ReadVisualEditorStyleProperties(id);

        public VisualEditorStyle UpdateVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle) => VisualEditorRepository.UpdateStyleProperties(visualEditorStyle);

        public MessageResult RemoveVisualEditorStyle(int id)
        {
            var style = VisualEditorRepository.GetStylePropertiesById(id);
            if (style == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorStyleNotFound, id));
            }

            if (style.IsSystem)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.SystemWarning, id));
            }

            VisualEditorRepository.DeleteStyle(id);
            return null;
        }

        public VisualEditorStyle NewVisualEditorStyleProperties(int parentId) => VisualEditorStyle.Create().Init();

        public VisualEditorStyle NewVisualEditorStylePropertiesForUpdate(int parentId) => NewVisualEditorStyleProperties(parentId);

        public VisualEditorStyle SaveVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle) => VisualEditorRepository.SaveStyleProperties(visualEditorStyle);
    }
}
