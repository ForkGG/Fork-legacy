using System;
using System.Collections.Generic;
using System.Text;

namespace Fork.logic.model.PluginModels
{
    public class PluginCategory
    {
        public int id { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
