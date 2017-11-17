using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    public class ObjectType : EntityObject
    {
        private const string _generic = "Generic";
        private const string _container = "Publishing Container";
        private const string _form = "Publishing Form";
        private const string _javaScript = "JavaScript";
        private const string _css = "Style Sheet (CSS)";

        public static ObjectType GetGeneric() => ObjectTypeRepository.GetByName(_generic);

        public static ObjectType GetContainer() => ObjectTypeRepository.GetByName(_container);

        public static ObjectType GetForm() => ObjectTypeRepository.GetByName(_form);

        public static ObjectType GetJavaScript() => ObjectTypeRepository.GetByName(_javaScript);

        public static ObjectType GetCss() => ObjectTypeRepository.GetByName(_css);
    }
}
