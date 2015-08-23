@echo off
setlocal
call d:\vs14\vc\bin\vcvars32.bat

nmake %*
@echo on
