using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage;

public class UserTasksViewModel : ListViewModel
{
    public static UserTasksViewModel Create(int id, string tabId, int parentId)
    {
        var model = Create<UserTasksViewModel>(tabId, parentId);
        model.UseParentEntityId = true;
        return model;
    }

    public override string EntityTypeCode => Constants.EntityTypeCode.Article;

    public override string ContextMenuCode => "";

    public override string ActionCode => Constants.ActionCode.ListExternalWorkflowUserTasks;
    public override bool AllowMultipleEntitySelection => false;
    public override bool LinkOpenNewTab => true;
    public override bool IsListDynamic => true;
    public string DataBindingControllerName => "Home";
    public string DataBindingActionName { get; set; }
}
