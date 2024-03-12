using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class HtmlTag
{
    public string Tag { get; set; }
    public List<string> AllowedDomains { get; set; }
}
