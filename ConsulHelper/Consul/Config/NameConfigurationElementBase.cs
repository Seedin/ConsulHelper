using System.Configuration;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    public abstract class NameConfigurationElementBase : ConfigurationElement
    {
        private const string NameItem = "name";
        private const string DescriptionItem = "description";

        [ConfigurationProperty(NameItem, IsKey = true, IsRequired = false)]
        public virtual string Name
        {
            get
            {
                return base[NameItem] as string;
            }
        }

        [ConfigurationProperty(DescriptionItem, IsRequired = false)]
        public virtual string Description
        {
            get
            {
                return base[DescriptionItem] as string;
            }
        }
    }
}
