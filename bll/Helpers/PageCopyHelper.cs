using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Helpers
{
    public class PageCopyHelper
    {
        private readonly int SourceId;

        private int DestinationId { get; }

        private Page Source { get; }

        private Page Destination { get; }

        public PageCopyHelper(int sourceId, int destinationId)
        {
            SourceId = sourceId;
            DestinationId = destinationId;
            Source = PageRepository.GetPagePropertiesById(sourceId);
            Destination = PageRepository.GetPagePropertiesById(destinationId);
        }

        public void Proceed()
        {
            var objects = ObjectRepository.GetPageObjects(SourceId);
            foreach (var obj in objects)
            {
                CopyObject(obj);
            }
        }

        private int CopyObject(BllObject sourceObj)
        {
            var tempObj = ObjectRepository.GetObjectPropertiesById(sourceObj.Id);
            tempObj.Id = 0;
            tempObj.PageId = DestinationId;
            tempObj.LockedBy = 0;
            var newObj = DefaultRepository.Save<BllObject, ObjectDAL>(tempObj);
            if (sourceObj.IsObjectContainerType)
            {
                CopyContainer(newObj.Id, sourceObj.Container);
                CopyContainerStatuses(newObj.Id, sourceObj.Id);
            }
            else if (sourceObj.IsObjectFormType)
            {
                CopyContentForm(newObj.Id, sourceObj.ContentForm);
            }
            var formats = ObjectFormatRepository.GetFormatsByObjectId(sourceObj.Id);
            var newDefFormatId = CopyObjectFormats(newObj.Id, sourceObj);
            if (newDefFormatId != 0)
            {
                ObjectRepository.UpdateDefaultFormatId(newObj.Id, newDefFormatId);
            }

            CopyObjectValues(newObj.Id, sourceObj);

            return newObj.Id;
        }

        private void CopyContentForm(int newObjId, ContentForm oldForm)
        {
            oldForm.ObjectId = newObjId;
            oldForm.LockedBy = 0;
            ObjectRepository.SaveContentForm(oldForm);
        }

        private void CopyContainer(int newObjId, Container oldContainer)
        {
            oldContainer.ObjectId = newObjId;
            oldContainer.LockedBy = 0;
            ObjectRepository.SaveContainer(oldContainer);
        }

        private void CopyObjectValues(int newObjId, BllObject oldObject)
        {
            oldObject.InitDefaultValues();
            ObjectRepository.SetDefaultValues(oldObject.DefaultValues, newObjId);
        }

        private void CopyContainerStatuses(int newObjId, int oldObjId)
        {
            var statusIds = ObjectRepository.GetObjectActiveStatusIds(oldObjId);
            ObjectRepository.SetObjectActiveStatuses(newObjId, statusIds, true);
        }

        private int CopyObjectFormats(int newObjId, BllObject oldObj)
        {
            var result = 0;
            var formats = ObjectFormatRepository.GetFormatsByObjectId(oldObj.Id);
            foreach (var frmt in formats)
            {
                frmt.Id = 0;
                frmt.ObjectId = newObjId;
                frmt.LockedBy = 0;

                if (frmt.Id == oldObj.DefaultFormatId)
                {
                    result = FormatRepository.SaveObjectFormatProperties(frmt).Id;
                }

                else
                {
                    FormatRepository.SaveObjectFormatProperties(frmt);
                }
            }

            return result;
        }
    }
}
