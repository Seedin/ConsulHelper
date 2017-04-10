namespace csharp ThriftDemo

struct ServiceKey
{
	1:string key
}

struct ServiceValue
{
	1:string value
}

service DemoService
{
	ServiceValue	GetKeyValue(1:ServiceKey key)
}