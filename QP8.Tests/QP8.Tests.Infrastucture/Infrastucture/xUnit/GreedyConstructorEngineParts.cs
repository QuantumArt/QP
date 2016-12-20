using System.Collections.Generic;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace QP8.Tests.Infrastucture.Infrastucture.xUnit
{
    public class GreedyConstructorEngineParts : DefaultEngineParts
    {
        public override IEnumerator<ISpecimenBuilder> GetEnumerator()
        {
            using (var iter = base.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    if (iter.Current is MethodInvoker)
                    {
                        yield return new MethodInvoker(new CompositeMethodQuery(new GreedyConstructorQuery(), new FactoryMethodQuery()));
                    }
                    else
                    {
                        yield return iter.Current;
                    }
                }
            }
        }
    }
}
