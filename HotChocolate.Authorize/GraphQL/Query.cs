using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace HotChocolateAuthorizeBug.GraphQL
{
    public class Query
    {
        public string Secured => "Secured";
        public string Unsecured => "Unsecured";
    }

    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(t => t.Secured).Authorize(LocalApi.PolicyName);
            descriptor.Field(t => t.Unsecured);
        }
    }
}
