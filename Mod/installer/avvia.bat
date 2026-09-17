@echo off
chcp 65001 >nul
title Little Witch in the Woods - traduzione italiana
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_installer\installer.ps1"
if errorlevel 1 pause
