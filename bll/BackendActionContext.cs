using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class BackendActionContext
    {
        public class Entity
        {
            public int? Id { get; set; }

            public string StringId { get; set; }
        }

        public static BackendActionContext Current
        {
            get => QPContext.BackendActionContext;
            private set => QPContext.BackendActionContext = value;
        }

        public string ActionTypeCode { get; }

        public string ActionCode { get; }

        public string EntityTypeCode { get; }

        public int FromEntityId { get; set; }

        public bool IsChanged { get; private set; }

        public int? ParentEntityId { get; }

        public Entity[] Entities { get; private set; }

        public void ResetEntityId(int id)
        {
            IsChanged = true;
            Entities = new[]
            {
                new Entity
                {
                    Id = id,
                    StringId = id.ToString()
                }
            };
        }

        public static void SetCurrent(string actionCode, IEnumerable<string> stringEntiryIDs, int? parentEntityId)
        {
            if (Current == null)
            {
                Current = new BackendActionContext(actionCode, stringEntiryIDs, parentEntityId);
            }
            else
            {
                if (!Current.ActionCode.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ArgumentException("Attempt to create BackendAction Context with different Action Code.");
                }
            }
        }

        public static void ResetCurrent()
        {
            Current = null;
        }

        public static void CreateLogs(
            string actionCode,
            IEnumerable<int> ids,
            int? parentId,
            IBackendActionLogRepository repository,
            bool isApi = false)
        {
            CreateLogs(actionCode, ids.Select(n => n.ToString()), parentId ?? 0, repository, isApi);
        }

        public static void CreateLogs(
            string actionCode,
            IEnumerable<string> ids,
            int parentId,
            IBackendActionLogRepository repository,
            bool isApi)
        {
            SetCurrent(actionCode, ids, parentId);
            var logs = BackendActionLog.CreateLogs(Current, repository, isApi);
            repository.Save(logs);
            ResetCurrent();
        }

        private BackendActionContext(string actionCode, IEnumerable<string> stringEntiryIDs, int? parentEntityId)
        {
            IsChanged = false;
            if (string.IsNullOrWhiteSpace(actionCode))
            {
                throw new ArgumentException(@"Action Code is empty", nameof(actionCode));
            }

            var cacheRow = BackendActionRepository.GetActionContextCacheData().SingleOrDefault(a => a.ActionCode.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase));

            ActionCode = actionCode;
            ParentEntityId = parentEntityId;
            ActionTypeCode = cacheRow?.ActionTypeCode ?? throw new ApplicationException("Backend action was not found by code: " + actionCode);
            EntityTypeCode = cacheRow.EntityTypeCode;
            Entities = stringEntiryIDs.Select(sid => new Entity
            {
                StringId = sid,
                Id = Converter.ToNullableInt32(sid)
            }).ToArray();
        }
    }
}
