﻿using System.Collections.Generic;
using Newtonsoft.Json;
using StoryTeller;
using StoryTeller.Json;

namespace Samples.Fixtures
{
    public class SampleJsonFixture : JsonComparisonFixture
    {
        public SampleJsonFixture()
        {
            Title = "Verifying Json Data";
        }

        [FormatAs("The json is {json}")]
        public void JsonIs(string json)
        {
            Context.State.StoreJson(json);
        }

        public IGrammar CheckOrder()
        {
            return CheckValue<int>("$.order", "The order should be {order}");
        }

        public IGrammar CheckNumberArray()
        {
            return CheckValue<int[]>("$.numbers", "The number array should be {numbers}");
        }

        public IGrammar CheckStringArray()
        {
            return CheckValue<string[]>("$.names", "The names array should be {names}");
        }

        [FormatAs("Deep equals check of {name} and {age}")]
        public void CompareChild(string name, int age)
        {
            var dict = new Dictionary<string, string> {{"name", name}, {"age", age.ToString()}};

            var json = JsonConvert.SerializeObject(dict);
            assertDeepJsonEquals("child", json);
            
        }
    }
}