using DLaB.Xrm.Test.Builders;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.FilterPlugin.Test.Builders
{
    public abstract class EntityBuilder<TEntity, TBuilder> : DLaBEntityBuilder<TEntity, TBuilder>
        where TBuilder : EntityBuilder<TEntity, TBuilder>
        where TEntity : Entity
    {

    }
}
