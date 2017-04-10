using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitAuto.Ucar.Utils.Common;

namespace ThriftDemo
{
    public class DemoServiceImpl: DemoService.Iface
    {
        public ServiceValue GetKeyValue(ServiceKey key)
        {
            return new ServiceValue()
            {
                Value = ConsulHelper.Instance.GetKeyValue(
                ConsulHelper.Instance.GetServiceConfigKey(key.Key))
            };
        }
    }
}
