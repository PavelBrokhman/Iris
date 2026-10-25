@echo off
rem The QA copy of a project under the umbrella, for checks by hand - never its Release copy.
rem   qa.bat mosaic          - Mosaic Remover, real mode (Lada and Video2X really started)
rem   qa.bat mosaic dummy    - Mosaic Remover, dummy mode (they are imitated, the lists work on copies)
rem The rest after the project's name goes to the project's own qa.bat, which builds the QA copy and starts it.
rem A project without a QA copy of its own is said so; one is added as one more line below.
setlocal
set "PROJECT=%~1"
if "%PROJECT%"=="" goto :usage
shift

if /i "%PROJECT%"=="mosaic" (
    call "D:\Projects\Video Mosaic Censor Remover\qa.bat" %1 %2 %3
    exit /b %errorlevel%
)
if /i "%PROJECT%"=="videotosrt" goto :none
if /i "%PROJECT%"=="vlc" goto :none

echo Unknown project: %PROJECT%
goto :usage

:none
echo %PROJECT% has no QA copy of its own yet.
exit /b 1

:usage
echo qa.bat ^<project^> [what the project's qa.bat takes]
echo   mosaic        Mosaic Remover (mosaic dummy - the dummy mode)
echo   videotosrt    VideoToSRT - no QA copy yet
echo   vlc           VLC Playlists - no QA copy yet
exit /b 2
