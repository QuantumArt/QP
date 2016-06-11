namespace Quantumart.QP8.Scheduler.API
{
	public class ServiceDescriptor
	{
		public string Key { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }

		public ServiceDescriptor(string key ,string name, string description)
		{
			Key = key;
			Name = name;
			Description = description;
		}
	}
}