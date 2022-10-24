var
  UserPage: TInputQueryWizardPage;
  ServersPage: TInputQueryWizardPage;

procedure InitializeWizard;
begin
  { Create the pages }  
  ServersPage := CreateInputQueryPage(wpWelcome,
    'Информация о сервисе', 'Адрес XML-интерфейса R-Keeper и сервера QRLine',
    'Введите адрес XML-интерфейса R-Keeper и адрес сервера QRLine, затем нажмите Далее');
  ServersPage.Add('адрес XML-интерфейса R-Keeper:', False);
  ServersPage.Add('адрес сервера QRLine для получения запросов:', False);
  ServersPage.Add('адрес сервера QRLine для отправки запросов:', False);
  ServersPage.Add('периодичность отправки запроса в секундах:', False);

  UserPage := CreateInputQueryPage(wpWelcome,
    'Информация о пользователе', 'Имя пользователя и пароль',
    'Введите имя пользователя и пароль для доступа к R-Keeper, затем нажмите Далее');
  UserPage.Add('Имя пользователя:', False);
  UserPage.Add('Пароль:', False);
end;

procedure ConvertConfig(xmlFileName: String);
var
  xmlFile: String;
  xmlInhalt: TArrayOfString;
  auth_keeper_name: String;
  auth_keeper_pass: String; 
  url_keeper: String;
  url_server_get: String;
  url_server_post: String;
  time_s: String;
  strName: String;
  strTest: String;
  tmpConfigFile: String;
  k: Integer;

begin
  xmlFile := ExpandConstant('{app}') + '\' + xmlFileName;
  tmpConfigFile:= ExpandConstant('{app}') + '\config.tmp';
  strName :=  UserPage.Values[0] +' '+ UserPage.Values[1];
  auth_keeper_name:= UserPage.Values[0];
  auth_keeper_pass:= UserPage.Values[1]; 
  url_keeper:= ServersPage.Values[0];
  url_server_get:= ServersPage.Values[1];
  url_server_post:= ServersPage.Values[2];
  time_s:= ServersPage.Values[3];

  if (FileExists(xmlFile)) then begin
    // Load the file to a String array
    LoadStringsFromFile(xmlFile, xmlInhalt);

    for k:=0 to GetArrayLength(xmlInhalt)-1 do begin
      strTest := xmlInhalt[k];
      if (Pos('key="url_server_get"', strTest) <> 0 ) then  begin
        strTest := '  <add key="url_server_get" value="' + url_server_get + '"/> ';
      end;

      if (Pos('key="url_server_post"', strTest) <> 0 ) then  begin
        strTest := '  <add key="url_server_post" value="' + url_server_post + '"/> ';
      end;

      if (Pos('key="url_keeper"', strTest) <> 0 ) then  begin
        strTest := '  <add key="url_keeper" value="' + url_keeper + '"/> ';
      end;

      if (Pos('key="time_s"', strTest) <> 0 ) then  begin
        strTest := '  <add key="time_s" value="' + time_s + '"/> ';
      end;

      if (Pos('key="auth_keeper_name"', strTest) <> 0 ) then  begin
        strTest := '  <add key="auth_keeper_name" value="' + auth_keeper_name + '"/> ';
      end;

      if (Pos('key="auth_keeper_pass"', strTest) <> 0 ) then  begin
        strTest := '  <add key="auth_keeper_pass" value="' + auth_keeper_pass + '"/> ';
      end;

      if (Pos('key="log_path"', strTest) <> 0 ) then  begin
        strTest := '  <add key="log_path" value="' + ExpandConstant('{app}') + '\log"/> ';
      end;

      SaveStringToFile(tmpConfigFile, strTest + #13#10,  True);
    end;

    DeleteFile(xmlFile); //delete the old exe.config
    RenameFile(tmpConfigFile,xmlFile);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  I: Longint;
  S: String;
begin
  { Validate certain pages before allowing the user to proceed }
  if CurPageID = UserPage.ID then begin
    if (UserPage.Values[0] = '') or (UserPage.Values[1] = '') then begin
      MsgBox('Вы должны ввести имя пользователя и пароль', mbError, MB_OK);
      Result := False;
    end else Result := True;
  end else if CurPageID = ServersPage.ID then begin
    S := ServersPage.Values[3];
    I := StrToIntDef(S, -1);
    if (ServersPage.Values[0] = '') or (ServersPage.Values[1] = '') or (ServersPage.Values[2] = '') or (ServersPage.Values[3] = '') then begin
      MsgBox('Вы должны ввести необходимые данные', mbError, MB_OK);
      Result := False;
    end else if I = -1 then begin
        MsgBox('Неверный формат ввода переодичности отправки запроса', mbError, MB_OK);
        Result := False;
    end else Result := True;
  end else
    Result := True;
end;


