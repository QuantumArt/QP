using System.Text.RegularExpressions;

namespace Quantumart.QP8.BLL;

public class BackendActionLogUserGroup
{
    public int Id { get; set; }
    public int BackendActionLogId { get; set; }
    public decimal GroupId { get; set; }

    public BackendActionLog ActionLog { get; set; }
    public Group UserGroup { get; set; }
}
