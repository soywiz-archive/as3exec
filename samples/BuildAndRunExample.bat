@echo off
CALL "%AIR_SDK_HOME%\bin\mxmlc" -static-link-runtime-shared-libraries -source-path=..\as3 Example.as
"%~dp0\..\as3exec.exe" Example.swf