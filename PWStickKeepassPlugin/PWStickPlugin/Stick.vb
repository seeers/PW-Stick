Imports System.Windows.Forms
Public Class Stick
    Private found As Boolean = False
    Private SPort As System.IO.Ports.SerialPort
    Private SP_Empfangen As String
    Private inByte As String
    Private allreceived As String
    Private startbytes() As Byte = {&HFE, &HFE}
    Private byteuserend() As Byte = {&H2}
    Private bytepwend() As Byte = {&H3}
    Private bytesend() As Byte = {&H4}
    Private bytefalse() As Byte = {&H0}
    Private bytetrue() As Byte = {&H1}
    Private CMD_Get_All_EEPROM() As Byte = {&HFE, &HFE, &H40, &H12}
    Private CMD_Erase() As Byte = {&HFE, &HFE, &H36, &H36}
    Private CMD_isReady() As Byte = {&HFE, &HFE, &H35, &H35}
    Private CMD_ChangePin() As Byte = {&HFE, &HFE, &H34}
    Private CMD_ChangeSettings() As Byte = {&HFE, &HFE, &H61}
    Private CMD_GetSettings() As Byte = {&HFE, &HFE, &H60}
    Private CMD_GetFWVersion() As Byte = {&HFE, &HFE, &H37, &H37}
    Private Const doneterminator As String = "Ã¾DONE"
    Const MaxChars As Integer = 45

    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return found
        End Get
    End Property

    Public ReadOnly Property GetPort
        Get
            Return SPort.PortName
        End Get
    End Property

    Sub New()
        FindConnectStick()
    End Sub

    Private Sub FindConnectStick()
        Try
            Dim fullfriendlyname As String = String.Empty
            Dim comportname As String = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("Arduino", fullfriendlyname)

            If comportname Is Nothing Then
                found = False
                Return
            End If

            SPort = New System.IO.Ports.SerialPort
            SPort.DtrEnable = True
            SPort.PortName = comportname
            SPort.RtsEnable = True
            SPort.Encoding = System.Text.Encoding.GetEncoding(1252)
            'SPort.Encoding = System.Text.Encoding.UTF8
            SPort.Open()
            SPort.Write(CMD_isReady, 0, CMD_isReady.Length)
            'SPort.Write(bytesend, 0, bytesend.Length)
            SPort.ReadTimeout = 1000

            Dim returnStr As String = SPort.ReadTo(doneterminator)
            If returnStr.Contains("locked") Or returnStr.Contains("OK") Then
                found = True
            Else
                found = False
            End If
            SPort.Close()
        Catch ex As Exception
            found = False
        End Try
    End Sub

    Private Function CheckStick(Optional ByRef isLocked As Boolean = True) As Boolean
        Try
            If SPort.IsOpen Then SPort.Close()
            SPort.Open()

            SPort.Write(CMD_isReady, 0, CMD_isReady.Length)

            SPort.ReadTimeout = 1000
            Dim returnStr As String = SPort.ReadTo(doneterminator)
            SPort.Close()
            If returnStr.Contains("locked") Then
                isLocked = True
                Return True
            ElseIf returnStr.Contains("OK") Then
                isLocked = False
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            FindConnectStick()
            If found Then
                Return True
            Else
                Return False
            End If

        End Try
    End Function


    Public Function IsStickLocked() As ReturnVals
        Dim islocked As Boolean = False
        If CheckStick(islocked) Then
            If islocked Then
                Return ReturnVals.StickLocked
            Else
                Return ReturnVals.Successfull
            End If
        Else
            Return ReturnVals.StickNotFound
        End If

    End Function

    Public Function EraseStick() As ReturnVals
        Try
            If CheckStick() Then
                SPort.Open()
                SPort.Write(CMD_Erase, 0, CMD_Erase.Length)
                SPort.Write(bytesend, 0, bytesend.Length)
                SPort.ReadTimeout = 10000
                Dim returnStr As String = SPort.ReadTo(doneterminator)
                SPort.Close()
                If returnStr.Contains("EraseDone") Then
                    Return ReturnVals.Successfull
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If

        Catch ex As Exception
            Return ReturnVals.Failed
        End Try
    End Function

    Public Function GetFirmwareVersion(ByRef Version As Version) As ReturnVals
        Try
            Version = New Version("0.0.0.1")
            If CheckStick() Then
                SPort.Encoding = System.Text.Encoding.GetEncoding(1252)
                SPort.Open()
                SPort.Write(CMD_GetFWVersion, 0, CMD_GetFWVersion.Length)
                SPort.Write(bytesend, 0, bytesend.Length)
                SPort.ReadTimeout = 2000
                '  System.Threading.Thread.Sleep(1000)
                Dim returnStr As String = SPort.ReadTo(doneterminator)
                SPort.Close()

                If returnStr.Contains(".") Then
                    returnStr = returnStr.Replace("Version", "")
                    Version = New Version(returnStr)
                    Return ReturnVals.Successfull
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If

        Catch ex As Exception
            Return ReturnVals.Failed
        End Try

    End Function


    Public Function readfromdevice(ByRef returnvalue As String) As ReturnVals

        If CheckStick() Then
            Try
                SPort.Open()
                SPort.ReadExisting() 'Clear Buffer
                SPort.Write(CMD_Get_All_EEPROM, 0, CMD_Get_All_EEPROM.Length)
                SPort.ReadTimeout = 2000
                returnvalue = SPort.ReadTo(doneterminator)
                SPort.Close()
                If returnvalue.Contains("†") And returnvalue.Contains("‡") Then
                    Return ReturnVals.Successfull
                ElseIf returnvalue.Contains("locked") Then
                    Return ReturnVals.StickLocked
                Else
                    Return ReturnVals.Failed
                End If

            Catch ex As Exception
                MessageBox.Show(ex.Message)
                Return ReturnVals.Failed
            End Try
        Else
            Return ReturnVals.StickNotFound
        End If

    End Function



    Public Function ChangeEntry(ByVal adress As Byte(), ByVal username As String, ByVal password As String) As ReturnVals
        Try
            If username.Length > MaxChars Then Throw New Exception("Es wurden zu viele Zeichen beim Benutzernamen verwendet! Erlaube Zeichen: " & MaxChars)
            If password.Length > MaxChars Then Throw New Exception("Es wurden zu viele Zeichen beim Passwort verwendet! Erlaube Zeichen: " & MaxChars)
            If CheckStick() Then
                '  SPort.Encoding = System.Text.Encoding.GetEncoding(1252)
                SPort.Open()
                SPort.Write(startbytes, 0, startbytes.Length)
                SPort.Write(adress, 0, adress.Length)

                '   Dim usernamebytes As Byte() = System.Text.Encoding.GetEncoding(1252).GetBytes(username)
                '  SPort.Write(usernamebytes, 0, usernamebytes.Length)

                SPort.Write(username)

                SPort.Write(byteuserend, 0, byteuserend.Length)
                SPort.Write(password)
                SPort.Write(bytepwend, 0, bytepwend.Length)
                SPort.Write(bytefalse, 0, bytefalse.Length)
                SPort.Write(bytesend, 0, bytesend.Length)
                ' SPort.Encoding = System.Text.Encoding.UTF8
                SPort.ReadTimeout = 1000
                Dim returnStr As String = ""
                Try
                    returnStr = SPort.ReadTo(doneterminator)
                Catch ex As Exception

                End Try
                ' Dim debugbytes As Byte() = System.Text.Encoding.GetEncoding(1252).GetBytes(returnStr)
                SPort.Close()
                If returnStr.Contains("WriteUser") And returnStr.Contains(username) Then
                    Return ReturnVals.Successfull
                ElseIf returnStr.Contains("locked") Then
                    Return ReturnVals.StickLocked
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If



        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return ReturnVals.Failed
        End Try
    End Function

    Public Function ChangePin(ByVal newpin As String) As ReturnVals
        Try
            If CheckStick() Then
                SPort.Open()
                SPort.Write(CMD_ChangePin, 0, CMD_ChangePin.Length)
                SPort.Write(newpin)
                SPort.Write(bytesend, 0, bytesend.Length)
                SPort.ReadTimeout = 1000
                Dim returnStr As String = SPort.ReadTo(doneterminator)
                SPort.Close()
                '  MessageBox.Show(returnStr & " Pin:" & newpin)
                If returnStr.Contains("NEWPIN:" & newpin) Then
                    Return ReturnVals.Successfull
                ElseIf returnStr.Contains("locked") Then
                    Return ReturnVals.StickLocked
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If

        Catch ex As Exception
            Return ReturnVals.Failed
        End Try

    End Function

    Public Function ChangeSettings(ByVal ENTER As Byte, ByVal locktime As Byte, ByVal LockWorkStation As Byte) As ReturnVals
        Try
            If CheckStick() Then
                Dim configdata As Byte() = New Byte() {ENTER, locktime, LockWorkStation}
                SPort.Open()
                SPort.Write(CMD_ChangeSettings, 0, CMD_ChangeSettings.Length)
                SPort.Write(configdata, 0, configdata.Length)
                SPort.Write(bytesend, 0, bytesend.Length)
                SPort.ReadTimeout = 1000
                Dim returnStr As String = SPort.ReadTo(doneterminator)
                SPort.Close()
                '  MessageBox.Show(returnStr & " Pin:" & newpin)
                If returnStr.Contains("Enter:" & ENTER) And returnStr.Contains("Lock:" & locktime) And returnStr.Contains("LockWS:" & LockWorkStation) Then
                    Return ReturnVals.Successfull
                ElseIf returnStr.Contains("locked") Then
                    Return ReturnVals.StickLocked
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If

        Catch ex As Exception
            Return ReturnVals.Failed
        End Try

    End Function


    Public Function GetSettings(ByRef Settings As String) As ReturnVals
        Try
            If CheckStick() Then
                SPort.Encoding = System.Text.Encoding.GetEncoding(1252)
                SPort.Open()
                SPort.Write(CMD_GetSettings, 0, CMD_GetSettings.Length)
                SPort.Write(bytesend, 0, bytesend.Length)
                SPort.ReadTimeout = 1000
                Dim returnStr As String = SPort.ReadTo(doneterminator)
                SPort.Close()

                If returnStr.Contains("‡") Then
                    Settings = returnStr
                    Return ReturnVals.Successfull
                Else
                    Return ReturnVals.Failed
                End If
            Else
                Return ReturnVals.StickNotFound
            End If

        Catch ex As Exception
            Return ReturnVals.Failed
        End Try

    End Function


    Public Function FindArduinoLeonardo() As Boolean
        Try
            Dim fullfriendlyname As String = String.Empty
            Dim comportname As String = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("Arduino", fullfriendlyname)
            If comportname.ToUpper.Contains("COM") Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function StartBootloader(ByRef UploadPort As String) As Boolean
        Try
            Dim fullfriendlyname As String = String.Empty
            Dim comportname As String = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("Arduino", fullfriendlyname)
            If comportname Is Nothing Then Return False

            If SPort Is Nothing Then
                SPort = New System.IO.Ports.SerialPort
                SPort.DtrEnable = True
                SPort.PortName = comportname
                SPort.RtsEnable = True
                'SPort.Encoding = System.Text.Encoding.UTF8
                'MessageBox.Show("Port wurde erstellt")
            End If

            If SPort.IsOpen Then SPort.Close()
            SPort.BaudRate = 1200
            SPort.Open()
            Threading.Thread.Sleep(100)
            SPort.Close()
            SPort.BaudRate = 9600

            fullfriendlyname = String.Empty
            For i As Integer = 0 To 15
                UploadPort = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("Arduino Leonardo bootloader", fullfriendlyname)
                Threading.Thread.Sleep(200)
                If Not UploadPort Is Nothing Then Exit For
            Next
            ' MessageBox.Show(UploadPort & "   " & fullfriendlyname)
            If Not UploadPort Is Nothing AndAlso UploadPort.ToUpper.StartsWith("COM") Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try



    End Function


    Public Enum ReturnVals
        Successfull = 0
        Failed = 1
        StickNotFound = 2
        StickLocked = 3
        Skipped = 4
    End Enum

End Class
