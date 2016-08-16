using System;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    public class KeyValueElementCollection : NamedConfigurationElementCollectionBase<KeyValueElement>
    {
        protected override string ElementName
        {
            get
            {
                return "keyValue";
            }
        }
    }
}
