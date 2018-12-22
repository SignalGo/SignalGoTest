extern alias SignalGoCodeGenerator;

using SignalGoCodeGenerator::SignalGo.CodeGenerator.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGoTest.ViewModels.Models
{
    public class ProjectItemInfo : ProjectItemInfoBase
    {
        public override int GetFileCount()
        {
            return 0;
        }

        public override string GetFileName(short index)
        {
            return "";
        }
    }
}
