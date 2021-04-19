@echo off
rem SOURCE: http://blogs.msdn.com/b/oldnewthing/archive/2008/04/17/8399914.aspx

for /f "usebackq delims=" %%d in (`"dir /ad/b/s | sort /R"`) do (
  attrib -R "%%d"
  rd "%%d"
)
