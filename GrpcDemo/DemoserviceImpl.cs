using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using BitAuto.Ucar.Utils.Common;

namespace GrpcDemo
{
    public class DemosSrviceImpl : DemoService.DemoServiceBase
    {
        public override Task<ServiceValue> GetKeyValue(ServiceKey request, ServerCallContext context)
        {
            return Task.FromResult(new ServiceValue()
            {
                Value = ConsulHelper.Instance.GetKeyValue(
                ConsulHelper.Instance.GetServiceConfigKey(request.Key))
            });
        }
    }
}
