using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntk.ToolsProject.Windows.AvahangHelper.Models
{
    public class ActionStatusModel
    {
        public ActionStatusModel(ProcessType actionType, string action)
        {
            Id = Guid.NewGuid();
            CompleteStatus = false;
            this.Action = action;
            ActionType = actionType;
            Error = "";
        }
        public Guid Id { get; set; }
        public bool CompleteStatus { get; set; }
        public ProcessType ActionType { get; set; }
        public string Action { get; set; }
        public string Error { get; set; }
    }
}
