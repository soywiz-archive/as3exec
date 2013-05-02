@echo off

REM SET BUILD=Release
SET BUILD=Debug

SET FILES=
SET FILES=%FILES% "%~dp0\cs\as3exec\as3exec\bin\%BUILD%\as3exec.exe"
SET FILES=%FILES% "%~dp0\cs\as3exec\as3exec\bin\%BUILD%\AxInterop.ShockwaveFlashObjects.dll"
SET FILES=%FILES% "%~dp0\cs\as3exec\as3exec\bin\%BUILD%\Interop.ShockwaveFlashObjects.dll"

SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
"%~dp0\utils\ILMerge.exe" %TARGET% /out:bin\as3exec.exe %FILES%