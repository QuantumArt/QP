namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal interface IDataProcessor
    {
        void Process();

        void Process(string inputData);
    }
}
