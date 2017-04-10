setlocal

cd /d %~dp0

set TOOLS_PATH=Tools

%TOOLS_PATH%\thrift-0.10.0 --gen csharp -out ./  Protos/demoservice.thrift

endlocal