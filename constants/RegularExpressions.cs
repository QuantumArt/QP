namespace Quantumart.QP8.Constants
{
    public static class RegularExpressions
    {
        public const string Ip = @"^(((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))\.){3}((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))$";
        public const string EntityName = @"^[^!-,\.\/:-@{-~\[\]]+$";
        public const string FieldName = @"^[^!-,\/:-@{-~\[\]]+$";
        public const string UserName = @"^[^""#%-\)\+,\/:-\?{-~\[\]]+$";
        public const string FolderName = @"^[^<>:""\/\\|\?\*]+$";
        public const string NetName = @"^[_a-zA-Z][_a-zA-Z0-9]*$";
        public const string FullQualifiedNetName = @"^[_a-zA-Z][_a-zA-Z0-9]*(\.[_a-zA-Z][_a-zA-Z0-9]*)*$";
        public const string Email = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string AbsoluteUrl = @"^http(s)?://(?:[\w\-]{1,63}|(?:(?!\d+\.|-)[\w\-]{1,63}(?<!-)\.)+(?:[\w\-]{1,}\w))(:[0-9]{1,5})?(/[\w\- ./?%&=]*)?$";
        public const string RelativeUrl = @"^/([\w\- ./?%&=]*$)";
        public const string AbsoluteWebFolderUrl = @"^http(s)?://(?:[\w\-]{1,63}|(?:(?!\d+\.|-)[\w\-]{1,63}(?<!-)\.)+(?:[\w\-]{1,}\w))(:[0-9]{1,5})?(/[\w\-/\.]*)?$";
        public const string RelativeWebFolderUrl = @"^/([\w\-_\./]*$)";
        public const string AbsoluteWindowsFolderPath = @"^(([a-zA-Z]:\\)|(\\))([\w\-\\\.]*)$";
        public const string RelativeWindowsFolderPath = @"^([\w\-\\\.]*)$";
        public const string DomainName = @"^(?:[\w\-]{1,63}|(?:(?!\d+\.|-)[\w\-]{1,63}(?<!-)\.)+(?:[\w\-]{1,}\w))(:[0-9]{1,5})?$";
        public const string FileName = @"^[\w\- ]+[\w\-. ]*$";
        public const string RgbColor = @"^[a-fA-F0-9]{6}$";
    }
}
