using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class TextFieldTagValidationSettings
{
    public bool Enabled { get; set; }
    public bool AllowEventAttributes { get; set; }
    public bool LogValidationError { get; set; }
    public List<AllowedTag> AllowedTags { get; set; }
}
