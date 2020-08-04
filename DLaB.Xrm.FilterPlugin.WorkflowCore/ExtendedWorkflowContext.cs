using System.Activities;
using Source.DLaB.Xrm.Workflow;

namespace DLaB.Xrm.FilterPlugin.Workflow
{
    public class ExtendedWorkflowContext: DLaBExtendedWorkflowContext
    {
        public ExtendedWorkflowContext(CodeActivityContext executionContext, CodeActivity codeActivity) 
            : base(executionContext, codeActivity)
        {
        }
    }
}
