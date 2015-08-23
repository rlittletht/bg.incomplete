
ODIR=ship
SRCDIR=..

CS_FLAGS=/debug:full /incr+
SHARED=..\..\shared

!ifdef DEBUG
ODIR=debug
CS_FLAGS=$(CS_FLAGS) /define:DEBUG /debug:full /debug+
!else
!endif

target: chdir bg.exe

clean: 
	-del /q $(ODIR)\*.*
	
chdir:
	@-mkdir $(ODIR) > NUL 2>&1
	@cd $(ODIR)  
	@echo Changed directory to $(ODIR)...

AssemblyInfo.netmodule: ..\AssemblyInfo.cs
	csc $(CS_FLAGS) /target:module /out:AssemblyInfo.netmodule ..\AssemblyInfo.cs

bg._bg.resources: $(SRCDIR)\bg.resx
	resgen $(SRCDIR)\bg.resx bg._bg.resources

#mshtml.dll: $(SRCDIR)\mshtml.dll
#	copy $(SRCDIR)\mshtml.dll .

#Interop.SHDocVw.dll: $(SRCDIR)\Interop.SHDocVw.dll
#	copy $(SRCDIR)\Interop.SHDocVw.dll .
#
#AxInterop.SHDocVw.dll: $(SRCDIR)\AxInterop.SHDocVw.dll
#	copy $(SRCDIR)\AxInterop.SHDocVw.dll .
	
#bg.m_frm.resources: $(SRCDIR)\bg.resx
#	resgen $(SRCDIR)\bg.resx bg.m_frm.resources
	
#  bg.m_frm.resources	
#/res:bg.m_frm.resources  
bg.exe: AssemblyInfo.netmodule $(SRCDIR)\bggrapher.cs $(SRCDIR)\bgreporter.cs $(SRCDIR)\bgmealcharter.cs $(SRCDIR)\bg.cs $(SRCDIR)\App.ico $(SHARED)\ole32.cs $(SHARED)\comport.cs $(SHARED)\ui.cs $(SRCDIR)\graph.cs $(SRCDIR)\hover.cs bg._bg.resources #  AxInterop.SHDocVw.dll Interop.SHDocVw.dll mshtml.dll
	csc $(CS_FLAGS) /target:winexe /out:bg.exe /addmodule:AssemblyInfo.netmodule $(SRCDIR)\bg.cs $(SHARED)\comport.cs $(SHARED)\ole32.cs $(SHARED)\ui.cs $(SRCDIR)\graph.cs  $(SRCDIR)\bggrapher.cs $(SRCDIR)\bgreporter.cs $(SRCDIR)\bgmealcharter.cs $(SRCDIR)\hover.cs /res:bg._bg.resources #  /r:AxInterop.SHDocVw.dll /r:Interop.SHDocVw.dll /r:mshtml.dll 
	
	

        
