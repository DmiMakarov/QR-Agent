; Имя приложения
#define   Name       "QR Agent"
; Версия приложения
#define   Version    "0.0.1"
; Имя исполняемого модуля
#define   ExeName    "QR-Agent.exe"

;------------------------------------------------------------------------------
;   Параметры установки
;------------------------------------------------------------------------------
[Setup]

; Уникальный идентификатор приложения, 
;сгенерированный через Tools -> Generate GUID
AppId={{ACB4F941-2D45-4047-9581-48B2498FDB27}
; Прочая информация, отображаемая при установке
AppName={#Name}
AppVersion={#Version}

; Путь установки по-умолчанию
DefaultDirName={pf}\{#Name}
; Имя группы в меню "Пуск"
DefaultGroupName={#Name}

; Каталог, куда будет записан собранный setup и имя исполняемого файла
OutputDir=D:\r-keeper\QR-agent\setup
OutputBaseFileName=QR-agentInstall

; Параметры сжатия
Compression=lzma
SolidCompression=yes
DisableDirPage = no

;------------------------------------------------------------------------------
;   Устанавливаем языки для процесса установки
;------------------------------------------------------------------------------
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; 
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl";

;------------------------------------------------------------------------------
;   Файлы, которые надо включить в пакет установщика
;------------------------------------------------------------------------------
[Files]

; Исполняемый файл
Source: "..\QR-Agent\bin\Debug\QR-Agent.exe"; DestDir: "{app}"; Flags: ignoreversion

; Прилагающиеся ресурсы
Source: "..\QR-Agent\bin\Debug\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; AfterInstall: ConvertConfig('QR-Agent.exe.config')

; .NET Framework 4.7.2
Source: ".\ndp472-devpack-enu.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected


;------------------------------------------------------------------------------
;   Секция кода включенная из отдельного файла
;------------------------------------------------------------------------------
[Code]
#include "dotnet.pas"
#include "myWizardForms.pas"

[Run]
Filename: {tmp}\ndp472-devpack-enu.exe; Parameters: "/q:a /c:""install /l /q"""; Check: not IsRequiredDotNetDetected; StatusMsg: Microsoft Framework 4.7.2 is installed. Please wait...
Filename: "{app}\QR-Agent.exe"; Parameters: "--install"
Filename: {sys}\sc.exe; Parameters: "start QR-Agent"; 


[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop QR-Agent"; 
Filename: {sys}\sc.exe; Parameters: "delete QR-Agent"