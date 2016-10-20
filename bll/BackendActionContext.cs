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
            get { return _current; }
            private set { _current = value; }
        }

        public string ActionTypeCode { get; private set; }

        public string ActionCode { get; }

        public string EntityTypeCode { get; private set; }

        public int FromEntityId { get; set; }

        public bool IsChanged { get; private set; }

        public int? ParentEntityId { get; private set; }

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

        #region Creating Thread Singelton
        [ThreadStatic]
        private static BackendActionContext _current;

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

        private BackendActionContext(string actionCode, IEnumerable<string> stringEntiryIDs, int? parentEntityId)
        {
            IsChanged = false;
            if (string.IsNullOrWhiteSpace(actionCode))
            {
                throw new ArgumentException(@"Action Code is empty", nameof(actionCode));
            }

            var cacheRow = BackendActionRepository.GetActionContextCacheData().SingleOrDefault(a => a.ActionCode.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase));
            if (cacheRow == null)
            {
                throw new ApplicationException("Backend action was not found by code: " + actionCode);
            }

            ActionCode = actionCode;
            ParentEntityId = parentEntityId;
            ActionTypeCode = cacheRow.ActionTypeCode;
            EntityTypeCode = cacheRow.EntityTypeCode;
            Entities = stringEntiryIDs.Select(sid => new Entity
            {
                StringId = sid,
                Id = Converter.ToNullableInt32(sid)
            }).ToArray();
        }
        #endregion
    }
}
