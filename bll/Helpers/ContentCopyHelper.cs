using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;

namespace Quantumart.QP8.BLL.Helpers
{
    public class ContentCopyHelper
    {
        private int SourceId { get; }

        private int DestinationId { get; }

        public bool SetParent { get; set; }

        private Content Source { get; }

        private Content Destination { get; }

        private readonly Dictionary<int, int> _linksMap = new Dictionary<int, int>();

        private readonly Dictionary<int, int> _fieldsMap = new Dictionary<int, int>();

        internal int[] ForceIds { get; set; }

        internal int[] ForceLinkIds { get; set; }

        public ContentCopyHelper(int sourceId, int destinationId, bool setParent)
        {
            SourceId = sourceId;
            DestinationId = destinationId;
            SetParent = setParent;
            Source = ContentRepository.GetById(sourceId);
            Destination = ContentRepository.GetById(destinationId);
        }

        public void Proceed()
        {
            CopyLinks();
            CopyFields();
            CopyConstraints();
            CopyCustomActions();
            ContentRepository.CopyAccess(SourceId, DestinationId);
        }

        private void CopyCustomActions()
        {
            ContentRepository.CopyCustomActions(SourceId, DestinationId);
        }

        #region private

        private void CopyLinks()
        {
            var links = ContentRepository.GetContentLinks(SourceId);
            var i = 0;
            foreach (var link in links)
            {
                CopyLink(link, i);
                i++;
            }
        }

        private void CopyLink(ContentLink link, int i)
        {
            var newLink = link.Clone(SourceId, DestinationId);
            if (ForceLinkIds != null)
            {
                newLink.ForceLinkId = ForceLinkIds[i];
            }

            newLink = ((IContentRepository)new ContentRepository()).SaveLink(newLink);
            _linksMap.Add(link.LinkId, newLink.LinkId);
        }

        private void CopyFields()
        {
            var i = 0;
            foreach (var field in Source.FieldsForCreateLike)
            {
                CopyField(field, i);
                i++;
            }

            foreach (var field in Destination.Fields.OrderBy(n => n.Order))
            {
                CorrectField(field);
            }
        }

        private void CopyField(Field field, int counter)
        {
            field.Init();
            field.LoadVeBindings();
            field.MutateLinqBackPropertyName();
            var oldFieldId = field.Id;

            if (field.DynamicImage != null)
            {
                field.DynamicImage.Id = 0;
            }

            if (ForceIds != null)
            {
                field.ForceId = ForceIds[counter];
            }

            field.Id = 0;
            field.ContentId = DestinationId;
            field.Content = Destination;
            if (field.ContentLink.LContentId == field.ContentLink.RContentId && !field.ContentLink.IsNew)
            {
                field.DefaultArticleIds = Enumerable.Empty<int>().ToArray();
            }
            field.Constraint = null;
            field.ContentLink = null;
            if (field.LinkId.HasValue && _linksMap.ContainsKey(field.LinkId.Value))
            {
                field.LinkId = _linksMap[field.LinkId.Value];
            }
            if (SetParent)
            {
                field.ParentFieldId = oldFieldId;
            }
            var newField = field.Persist();
            _fieldsMap.Add(oldFieldId, newField.Id);
        }

        private void CorrectField(Field field)
        {
            field.Init();
            if (field.BaseImageId.HasValue && _fieldsMap.ContainsKey(field.BaseImageId.Value))
            {
                field.BaseImageId = _fieldsMap[field.BaseImageId.Value];
            }

            if (field.RelationId.HasValue && _fieldsMap.ContainsKey(field.RelationId.Value))
            {
                field.RelationId = _fieldsMap[field.RelationId.Value];
                field.DefaultValue = null;
            }

            if (field.BackRelationId.HasValue && _fieldsMap.ContainsKey(field.BackRelationId.Value))
            {
                field.BackRelationId = _fieldsMap[field.BackRelationId.Value];
            }

            field.Persist();
        }

        private void CopyConstraints()
        {
            foreach (var constraint in Source.Constraints)
            {
                constraint.Id = 0;
                constraint.ContentId = DestinationId;
                foreach (var rule in constraint.Rules)
                {
                    rule.FieldId = _fieldsMap[rule.FieldId];
                }

                ContentConstraintRepository.Save(constraint);
            }
        }

        #endregion
    }
}
