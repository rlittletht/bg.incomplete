@echo off
setlocal
call d:\vs.net\vc7\bin\vcvars32.bat
rem set path=%path%;f:\vs6\vc98\bin;k:\vs6\common\msdev98\bin
set include=%include%;f:\wmsdk\wmfsdk\include
set lib=%lib%;f:\wmsdk\wmfsdk\lib

nmake %*
@echo on
