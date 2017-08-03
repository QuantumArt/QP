using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    internal interface ITreeProcessor
    {
        IList<EntityTreeItem> Process();
    }
}
