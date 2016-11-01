@ECHO Off
pushd "%~dp0"

ECHO.
ECHO Building and installing SurfaceBlobDetection.
PAUSE
ECHO.

::-------------------------------------------------------------------------------
:: Try to copy a file to a protected directory to see if the script is being run 
:: as admin. If not, elevate or warn the user and quit. 
:: - Make sure we dont leave the file on the hard drive!

copy CreateShellXML.vbs "%ProgramFiles%" > NUL
IF NOT EXIST "%ProgramFiles%\CreateShellXML.vbs" goto NO_RIGHTS
del "%ProgramFiles%\CreateShellXML.vbs"

::-------------------------------------------------------------------------------
:: Check to see if the project is in a location that all users can access

FOR /F %%A IN ('cacls .') DO (
  CheckUserPermissions.VBS %%A
  IF ERRORLEVEL 1 GOTO USER_HAS_PERMISSIONS
)

ECHO.
ECHO Not all users have permission to access this directory. 
ECHO If you install from this directory, you may not be
ECHO able to access them from other user accounts.
ECHO.
ECHO Continue?
CHOICE

IF ERRORLEVEL 2 GOTO :USER_HAS_NO_PERMISSIONS

:USER_HAS_PERMISSIONS
::-------------------------------------------------------------------------------
:: Check existence MSBuild.exe

ECHO Checking for MSBuild.
SET MsBuildPath="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET LogFile=build.log.txt
SET MSBuildParameters=/p:Configuration=Release /noconsolelogger /fl /fileLoggerParameters:LogFile=%LogFile%;Append /nologo
IF NOT EXIST %MsBuildPath% GOTO CANNOT_FIND_MSBUILD

::-------------------------------------------------------------------------------
:: Create ObjectSets folder

SET ObjectTagSetFolder="%ProgramData%\Microsoft\Surface\v2.0\ObjectSets"
:: Make sure we are checking existance of the folder (by appending *), not file.
IF NOT EXIST %ObjectTagSetFolder%\* (
  ECHO Creating Object Tag Set folder.
  MKDIR %ObjectTagSetFolder%
  IF ERRORLEVEL 1 goto CANNOT_CREATE_OBJECTTAGSETDIR
)

::-------------------------------------------------------------------------------
:: Build the projects

:: Track any errors when building
SET BuildErrors=""
IF EXIST %LogFile% del %LogFile%

SET ExeDirPath="bin\release"












:SurfaceBlobDetection
ECHO Building SurfaceBlobDetection...
%MsBuildPath% SurfaceBlobDetection\SurfaceBlobDetection.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto SurfaceBlobDetectionError
ECHO Generating SurfaceBlobDetection XML...
CreateShellXML.vbs SurfaceBlobDetection\InstalledAppInfo\SurfaceBlobDetection.xml "%CD%" SurfaceBlobDetection %ExeDirPath%
ECHO Installing SurfaceBlobDetection to Surface Shell...
ECHO.
goto COMPLETE

:SurfaceBlobDetectionError
SET BuildErrors=true
ECHO Error building SurfaceBlobDetection. Please build in Visual Studio and rerun this batch file.
ECHO.











goto COMPLETE

:QUERY_REGISTRY
FOR /F "eol=H tokens=2*" %%A IN ('REG QUERY %1 /v %2') DO SET %3=%%B
goto :EOF

:NO_RIGHTS
ECHO.
ECHO Please rerun this batch file as administrator. 
PAUSE
ECHO.  
GOTO EOF

:USER_HAS_NO_PERMISSIONS
ECHO.
ECHO Please extract to a public directory and rerun this script.
PAUSE
ECHO.  
GOTO EOF

:CANNOT_FIND_MSBUILD
ECHO.
ECHO Cannot find MSBuild.exe to build.
ECHO Please ensure that version 3.5 of the .Net Framework is installed.
PAUSE
ECHO.
GOTO EOF

:CANNOT_CREATE_OBJECTTAGSETDIR
ECHO.
ECHO Cannot create %ObjectTagSetFolder% folder to deploy Object Tag Set XML files.
PAUSE
ECHO.
GOTO EOF

:COMPLETE
ECHO.
IF %BuildErrors%==true ECHO Projects installed, but some failed to build. Please build in Visual Studio to fix build errors
IF NOT %BuildErrors%==true ECHO Successfully installed.
PAUSE
ECHO.  

:EOF
popd
