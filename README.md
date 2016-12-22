# ConsulHelper
ConsulHelper，.Net微服务基础框架，具备服务发现、健康检查、服务分级、分布式配置、版本控制及RPC高可用代理功能（当前已实现Http、Thrift、grpc及Wcf代理），基于Consul。

应用前准备：

A、确定消费者服务名，即应用名，设为appName，同时确认应用是否对外提供接口服务；

B、确定提供者服务名，即待调用服务名，设为svcName可能调用多个服务，服务名可能已于Consul登记，不存在则需要额外登记；

C、确定调用服务方式，即通信协议，设为protocolTag。

使用步骤：

1、部署Consul Client，消费者、提供者服务所在主机都需要部署Consul Client，Consul Server部署请参考Consul官网。

注意事项：

A、安装包路径，见Demo下Tool目录，已提供Winidows/Linux双版本，解压后执行install.bat或install脚本。

B、x86系统不兼容，默认安装包仅支持x64系统，如出现此类系统请单独与管理员联系获取专用版本。

C、指定Consul服务端，安装前请修改joinCluster脚本，将serverIp设为部署环境内Consul集群中一台服务端结点IP。

D、主机多网卡，常见于测试机与仿真机，应修改etc\consul.d\consul.json文件内bind_addr为绑定IP，单网卡时配置空值即可，若IP非固定（如DHCP），每次重启请注意调整绑定IP。

E、待调用服务仍未注册，参考Consul官网添加监控服务配置步骤，在待调用服务主机增加相应配置文件。

F、主机重启时Consul客户端可设置为自启动，但默认不会自动加入Consul集群，应自行执行joinCluster脚本将结点加入集群，即上线。

G、无论任何变更，都可以重装客户端方式解决，即执行uninstall后再次执行install脚本，注意操作前确认配置如绑定IP、集群服务端IP、服务配置。

H、请确保主机名唯一，如重复请进行更改，尤其在线上环境虚机、容器常以克隆方式部署，可能出现主机名重复，此时Consul相关主机名功能将出现混乱。

I、安装过程中提示Consul已安装，则不必再次安装。  

2、初始化开发环境，引入依赖库，定制配置文件：

注意事项，

A、Nugut依赖库下载失败，请更新Nugut或调整下载源协议或地址，如不使用Nuget，请核对必要依赖库清单；

B、依赖库清单，
.Net FrameWork 4.5，框架组件，必须，.Net Core待测试；
Consul.dll，Consul官方.Net客户端，封装Restful接口调用，必须；
ConsulHelper.dll，即本组件，必须；
Newtonsoft.Json.dll，Json序列化组件，使用Http通信时必须；
Thrift.dll，Thrift.Net组件，使用Thrift通信时必须；
Grpc.Core.dll，Grpc.Net组件，使用Grpc通信时必须；
System.Interactive.Async.dll，Grpc.Net依赖组件，使用Grpc通信时必须；
grpc_csharp_ext.x64.dll，Grpc.Net依赖组件，x64环境使用Grpc通信时必须；
grpc_csharp_ext.x86.dll，Grpc.Net依赖组件，x86环境使用Grpc通信时必须。

C、配置文件引入，参考Demo，将consulplugin引入，配置片段建议为相对路径Config\consulplugin.config。

D、自注册配置项细节，参考Demo中consulplugin内注释，若服务角色为消费者，注意servicetags、serviceport、httpcheck与tcpcheck属性配置。

E、服务标签含义，即servicetags属性，对外提供服务时，protocolTag（wcf、http、thrift或grpc）必须包括，此为服务通信协议约定，其他自定义标签可用于服务分级、分版等隔离功能，多标签使用逗号分隔。

F、服务订阅配置，即services配置段，指定服务名与订阅标签，标签支持逗号分隔，可圈定可用服务范围与通信模式，该配置强制加入Consul分布式配置，格式为F:ServcieTags:{appName}:{svcName}:{hostname}，支持远程控制。

G、协议内置配置，即keyvalues配置段内含服务名配置，用以指定通信细节，通常可省略（有默认值），支持Consul分布式配置，格式为F:Config:{appName}:{svcName}:{protocolTag}:{item}。

H、自定义配置，即keyvalues配置段内test配置，用以自定义配置，支持Consul分布式配置，支持Consul不可用时的本地配置，格式为F:Config:{appName}:{item}。
 

3、接口调用与调试。

注意事项，

A、调用方式，参考Demo，核心代码如下，
using (var client = ConsulHelper.Instance.GetServiceClient("{svcName}"))
{
	var stub = client.GetStub<{stubType}>();
	var ret = stub.{action};
}

B、运行时ConsulHelper.Instance加载失败，请核对App.config或Web.config中是否正确引入consulplugin。

C、运行时GetServiceClient失败，请核对调用参数中serviceName是否与consulplugin中的svcName一致，同时请与Consul UI中确认是否存在满足指定标签（即标签包含）的指定名称已注册服务。

D、运行时Wcf获取Client失败，请注意App.config或Web.config中的system.serviceModel配置段，由于代理涉及反射，请确认endpoint的name属性与svcName一致，同时协议内置配置是否正确配置，如Protocol、ServicePath、ChanelType（格式为代理类全名,所在程序集名称）。

E、运行时Thrift与Grpc获取Client失败，请确认相应必须依赖库是否全部引入。

E、代理类引入（即泛型T），Wcf、Thrift与Grpc直接引入自动代码生成的代理类即可，Http请使用BitAuto.Ucar.Utils.Common.Service.Stub.HttpStub。

E、本地服务订阅标签无效，服务订阅配置以Consul分布式配置为准，请查阅Consul UI中相应分布式配置。


其他：

A、为简化依赖并突出核心功能，该版本不含性能度量与日志记录这两种监控功能，相应功能由其他未开源组件实现；

B、文档较简单，详细功能可参考注释。

C、如有问题可与我联系，邮箱为liujing6@yiche.com。
