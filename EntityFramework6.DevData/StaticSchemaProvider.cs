using Quantumart.QP8.CodeGeneration.Services;


namespace Quantumart.QP8.EntityFramework6.DevData
{
    public class StaticSchemaProvider : ISchemaProvider
    {
	   public StaticSchemaProvider()
       {
       }

	    #region ISchemaProvider implementation
        public ModelReader GetSchema()
        {
			var schema = new ModelReader();
			return schema;
        }

        public object GetCacheKey()
        {
            return null;
        }
		#endregion
    }
}
