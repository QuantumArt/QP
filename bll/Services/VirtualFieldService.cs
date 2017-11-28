using System;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public static class VirtualFieldService
    {
        public static Field Update(Field item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!FieldRepository.Exists(item.Id))
            {
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, item.Id));
            }

            if (item.Content.VirtualType == VirtualType.None)
            {
                throw new ApplicationException($"Content {item.Content.Id} of field {item.Id} is not virtual.");
            }

            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(item.ContentId))
            {
                var newField = ((IFieldRepository)new FieldRepository()).Update(item);
                var helper = new VirtualContentHelper();

                helper.DropContentViews(newField.Content);
                helper.CreateContentViews(newField.Content);

                // Обновить дочерние виртуальные контенты
                helper.UpdateVirtualFields(newField);
                return newField;
            }
        }
    }
}
