using Quantumart.QP8.BLL.Services.DTO;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    internal interface ITreeProcessor
    {
        IList<EntityTreeItem> Process();
    }
}
