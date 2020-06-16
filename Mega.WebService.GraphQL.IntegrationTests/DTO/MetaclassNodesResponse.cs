using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mega.WebService.GraphQL.IntegrationTests.DTO
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    public class MetaclassNodesResponse
    {
        public List<MetaclassNodeInstance> MetaClass { get; set; }

        public class MetaclassNodeInstance
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<MetaThingInstance> MetaAttribute { get; set; }
            public List<MetaThingInstance> MetaClass_SubMetaClass { get; set; }
            public List<MetaThingInstance> MetaClass_SuperMetaClass { get; set; }
            public List<MetaThingInstance> FilteredSubMetaClass { get; set; }
            public List<MetaThingInstance> FilteredSuperMetaClass { get; set; }
            public List<SchemaInstance> Schemas { get; set; }

            public class MetaThingInstance
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public string NameLanguage { get; set; }
                public string MetaLayer { get; set; }
                public List<SchemaInstance> Schemas { get; set; }
            }

            public class SchemaInstance
            {
                public string Name { get; set; }
                public string GraphQLNameInSchema { get; set; }
            }
        }
    }
}
