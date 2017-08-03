using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    internal class SiteFolderProcessor : ITreeProcessor
    {
        private int? _parentEntityId;
        private readonly string _entityTypeCode;
        private readonly bool _returnSelf;
        private readonly int? _entityId;

        public SiteFolderProcessor(int? parentEntityId, string entityTypeCode, bool returnSelf, int? entityId)
        {
            _parentEntityId = parentEntityId;
            _entityTypeCode = entityTypeCode;
            _returnSelf = returnSelf;
            _entityId = entityId;
        }

        public IList<EntityTreeItem> Process()
        {
            if (_parentEntityId == null)
            {
                throw new ArgumentNullException(nameof(_parentEntityId));
            }

            var folderRepository = FolderFactory.Create(_entityTypeCode).CreateRepository();
            var children = _returnSelf
                ? Enumerable.Repeat(folderRepository.GetSelfAndChildrenWithSync(_parentEntityId.Value, _entityId), 1)
                : folderRepository.GetChildrenWithSync(_parentEntityId.Value, _entityId);

            return Mapper.Map<IEnumerable<Folder>, List<EntityTreeItem>>(children);
        }
    }
}
