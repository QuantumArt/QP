using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Helpers
{
	public class ContentCopyHelper
	{
		private int SourceId { get; set; }

		private int DestinationId { get; set; }

		public bool SetParent { get; set; }

		Content Source { get; set; }

        Content Destination { get; set; }

		Dictionary<int, int> LinksMap = new Dictionary<int, int>();

		Dictionary<int, int> FieldsMap = new Dictionary<int, int>();

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
			List<ContentLink> links = ContentRepository.GetContentLinks(SourceId);

			int i = 0;
			foreach (var link in links)
			{
				CopyLink(link, i);
				i++;
			}
		}

		private void CopyLink(ContentLink link, int i)
		{
			ContentLink newLink = link.Clone(SourceId, DestinationId);
			
			if (ForceLinkIds != null)
				newLink.ForceLinkId = ForceLinkIds[i];

			newLink = ContentRepository.SaveLink(newLink);
			LinksMap.Add(link.LinkId, newLink.LinkId);
		}

		private void CopyFields()
		{
			int i = 0;
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
			int oldFieldId = field.Id;
			
			if (field.DynamicImage != null)
				field.DynamicImage.Id = 0;

			if (ForceIds != null)
				field.ForceId = ForceIds[counter];

			field.Id = 0;
			field.ContentId = DestinationId;
			field.Content = Destination;
			if (field.ContentLink.LContentId == field.ContentLink.RContentId && !field.ContentLink.IsNew)
			{
				field.DefaultArticleIds = Enumerable.Empty<int>().ToArray();
			}
			field.Constraint = null;
			field.ContentLink = null;
			if (field.LinkId.HasValue && LinksMap.ContainsKey(field.LinkId.Value))
			{
				field.LinkId = LinksMap[field.LinkId.Value];
			}
			if (SetParent)
			{
				field.ParentFieldId = oldFieldId;
			}
			Field newField = field.Persist();
			FieldsMap.Add(oldFieldId, newField.Id);
		}

        private void CorrectField(Field field)
        {
			field.Init();
			if (field.BaseImageId.HasValue && FieldsMap.ContainsKey(field.BaseImageId.Value)) 
            {
                field.BaseImageId = FieldsMap[field.BaseImageId.Value];
            }

            if (field.RelationId.HasValue && FieldsMap.ContainsKey(field.RelationId.Value))
            {
                field.RelationId = FieldsMap[field.RelationId.Value];
				field.DefaultValue = null;
            }

            if (field.BackRelationId.HasValue && FieldsMap.ContainsKey(field.BackRelationId.Value))
            {
                field.BackRelationId = FieldsMap[field.BackRelationId.Value];
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
					rule.FieldId = FieldsMap[rule.FieldId];
				}
				ContentConstraintRepository.Save(constraint);
			}
		}

		#endregion
	}
}
