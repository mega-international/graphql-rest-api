using System;
using System.Collections.Generic;
using Mega.Bridge.Models;

namespace Mega.WebService.GraphQL.Models
{
    [Serializable]
    public class HopexEnvironmentsWithRepositories
    {
        public List<HopexEnvironmentWithRepositories> Environments { get; set; }

        public HopexEnvironmentsWithRepositories()
        {
            Environments = new List<HopexEnvironmentWithRepositories>();
        }
    }

    [Serializable]
    public class HopexEnvironmentWithRepositories
    {
        public string Id { get; set; }
        public string Name { get; set; }
        //public string Path { get; set; }
        public List<HopexBase> Repositories { get; set; }

        public HopexEnvironmentWithRepositories()
        {
            Repositories = new List<HopexBase>();
        }
    }
}
