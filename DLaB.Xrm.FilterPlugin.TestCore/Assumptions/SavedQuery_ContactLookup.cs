using DLaB.Xrm.FilterPlugin.Entities;
using DLaB.Xrm.Test.Assumptions;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.FilterPlugin.Test.Assumptions
{
    // ReSharper disable once InconsistentNaming
    public class SavedQuery_ContactLookup : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<SavedQuery_ContactLookup, Product>
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            return service.GetFirstOrDefault<SavedQuery>(q => new
                {
                    q.Name,
                    q.FetchXml,
                    q.LayoutXml,
                    q.QueryType,
                    q.ReturnedTypeCode
                },
                SavedQuery.Fields.QueryType, UserQueryQueryType.LookupView,
                new ConditionExpression(SavedQuery.Fields.LayoutXml, ConditionOperator.NotNull),
                SavedQuery.Fields.ReturnedTypeCode, Contact.EntityLogicalName);
        }
    }
}
