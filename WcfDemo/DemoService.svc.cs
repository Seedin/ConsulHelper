using System;
using BitAuto.Ucar.Utils.Common;

namespace WcfDemo
{
    public class DemoService : IDemoService
    {
        public string GetData(int value)
        {
            try
            {
                return string.Format("You entered {0} and get {1}", value, ConsulHelper.Instance.GetHashCode());
            }
            catch (Exception err)
            {
                return err.Message + err.StackTrace;
            }
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            //延迟启动ConsulHelper
            ConsulHelper.Instance.GetHashCode();
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
