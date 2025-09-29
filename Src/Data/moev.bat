@echo off

rem --- 开始移动文件夹 ---
xcopy "Data" "../Client" /E /I /Y
echo 移动完成！
pause