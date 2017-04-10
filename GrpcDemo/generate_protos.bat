setlocal

cd /d %~dp0

set TOOLS_PATH=Tools\windows_x86

%TOOLS_PATH%\protoc.exe -I Protos --csharp_out ./  Protos/demoservice.proto --grpc_out ./ --plugin=protoc-gen-grpc=%TOOLS_PATH%\grpc_csharp_plugin.exe

endlocal