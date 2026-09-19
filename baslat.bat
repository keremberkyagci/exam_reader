@echo off
chcp 65001 >nul
title Exam Reader - API & Web
echo.
echo =========================================
echo    Exam Reader Uygulamasi Baslatiliyor
echo =========================================
echo.

echo [1/2] API (Backend) baslatiliyor...
start "ExamReader API" cmd /k "cd ExamReader.Api && dotnet run"

echo [2/2] Web Arayuzu (Frontend) baslatiliyor...
start "ExamReader Web" cmd /k "cd exam-reader-web && ng serve"

echo.
echo Uygulama baslatildi!
echo Tarayicida su adresi acin: http://localhost:4200
echo.
pause
