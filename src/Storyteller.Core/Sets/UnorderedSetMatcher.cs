using System.Collections.Generic;
using System.Linq;
using Storyteller.Core.Conversion;
using Storyteller.Core.Model;

namespace Storyteller.Core.Sets
{
    public class UnorderedSetMatcher : ISetMatcher
    {
        public static readonly ISetMatcher Flyweight = new UnorderedSetMatcher();

        public virtual SetVerificationResult Match(Cell[] cells, IEnumerable<StepValues> expectedValues,
            IEnumerable<StepValues> actualValues)
        {
            var result = new SetVerificationResult();

            foreach (var expected in expectedValues)
            {
                var actual = actualValues.FirstOrDefault(x => cells.Matches(expected, x));
                if (actual == null)
                {
                    result.MarkMissing(expected.Id);
                }
                else
                {
                    processMatch(actual, result, expected);
                }
            }

            actualValues.Where(x => !x.IsMatched).Each(result.MarkExtra);

            return result;
        }

        protected virtual void processMatch(StepValues actual, SetVerificationResult result, StepValues expected)
        {
            actual.IsMatched = true;
            result.MarkMatched(expected.Id);
        }
    }
}