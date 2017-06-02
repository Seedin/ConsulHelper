namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    /// <summary>
    /// 共有元素
    /// </summary>
    public abstract class NameConfigurationElementBase
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }
    }
}
