namespace Quantumart.QP8.Scheduler.API
{
    public class ServiceDescriptor
    {
        public string Key { get; }

        public string Name { get; }

        public string Description { get; }

        public ServiceDescriptor(string key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
        }
    }
}
