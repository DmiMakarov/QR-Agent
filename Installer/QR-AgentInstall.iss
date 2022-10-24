; ��� ����������
#define   Name       "QR Agent"
; ������ ����������
#define   Version    "0.0.1"
; ��� ������������ ������
#define   ExeName    "QR-Agent.exe"

;------------------------------------------------------------------------------
;   ��������� ���������
;------------------------------------------------------------------------------
[Setup]

; ���������� ������������� ����������, 
;��������������� ����� Tools -> Generate GUID
AppId={{ACB4F941-2D45-4047-9581-48B2498FDB27}
; ������ ����������, ������������ ��� ���������
AppName={#Name}
AppVersion={#Version}

; ���� ��������� ��-���������
DefaultDirName={pf}\{#Name}
; ��� ������ � ���� "����"
DefaultGroupName={#Name}

; �������, ���� ����� ������� ��������� setup � ��� ������������ �����
OutputDir=D:\r-keeper\QR-agent\setup
OutputBaseFileName=QR-agentInstall

; ��������� ������
Compression=lzma
SolidCompression=yes
DisableDirPage = no

;------------------------------------------------------------------------------
;   ������������� ����� ��� �������� ���������
;------------------------------------------------------------------------------
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; 
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl";

;------------------------------------------------------------------------------
;   �����, ������� ���� �������� � ����� �����������
;------------------------------------------------------------------------------
[Files]

; ����������� ����
Source: "..\QR-Agent\bin\Debug\QR-Agent.exe"; DestDir: "{app}"; Flags: ignoreversion

; ������������� �������
Source: "..\QR-Agent\bin\Debug\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; AfterInstall: ConvertConfig('QR-Agent.exe.config')

; .NET Framework 4.7.2
Source: ".\ndp472-devpack-enu.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected


;------------------------------------------------------------------------------
;   ������ ���� ���������� �� ���������� �����
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