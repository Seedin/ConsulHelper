using System.Configuration;
using System;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    [ConfigurationCollection(typeof(NameConfigurationElementBase),
        CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public abstract class NamedConfigurationElementCollectionBase<T> : ConfigurationElementCollection
        where T : NameConfigurationElementBase, new()
    {
        public T this[int index]
        {
            get
            {
                return (T)base.BaseGet(index);
            }
        }

        public new T this[string name]
        {
            get
            {
                return (T)base.BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as T).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }
}
