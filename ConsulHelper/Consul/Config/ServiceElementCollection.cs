using System;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    public class ServiceElementCollection : NamedConfigurationElementCollectionBase<ServiceElement>
    {
        protected override string ElementName
        {
            get
            {
                return "service";
            }
        }
    }
}
