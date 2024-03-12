using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class TextFieldTagValidationSettings
{
    public bool Enabled { get; set; }
    public bool LogValidationError { get; set; }
    public List<HtmlTag> AllowedTags { get; set; }
}
