using System.Threading.Tasks;
using StoryTeller.Model.Persistence;
using StoryTeller.Remotes;
using ProjectInput = ST.CommandLine.ProjectInput;

namespace ST.Client
{
    public class OpenInput : ProjectInput
    {
        public OpenInput() : base(EngineMode.Interactive)
        {
        }

        public Task<Suite> ReadHierarchy()
        {
            return Task.Factory.StartNew(() => HierarchyLoader.ReadHierarchy(SpecPath));
        }
    }
}
