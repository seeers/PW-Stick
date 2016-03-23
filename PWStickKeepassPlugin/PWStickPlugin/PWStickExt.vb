Imports KeePass.Plugins
Imports KeePassLib
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports KeePassLib.Security
Imports System.Text
Imports System.IO.Compression



Public NotInheritable Class PWStickExt
    Inherits Plugin


    Private m_tsSeparator As ToolStripSeparator = Nothing
    Private _ReadFromStickMenu As ToolStripMenuItem = Nothing
    Private _ChangePinMenu As ToolStripMenuItem = Nothing
    Private _EraseStick As ToolStripMenuItem = Nothing
    Private _StickSettings As ToolStripMenuItem = Nothing
    Private _sendToButtonMenu As ToolStripMenuItem
    Private _submenuitem1 As ToolStripMenuItem
    Private _submenuitem2 As ToolStripMenuItem
    Private _submenuitem3 As ToolStripMenuItem
    Private _submenuitem4 As ToolStripMenuItem
    Private _submenuitem5 As ToolStripMenuItem
    Private _submenuitem6 As ToolStripMenuItem
    Private _submenuitem7 As ToolStripMenuItem
    Private _submenuitem8 As ToolStripMenuItem
    Private _submenuitem9 As ToolStripMenuItem
    Private _submenuitem10 As ToolStripMenuItem

    Private _sendGroupToStick As ToolStripMenuItem

    Private _host As IPluginHost = Nothing
    Private PwStick As Stick


    Dim allcontrols As List(Of Oneuser)

    Dim fromright As Boolean
    Dim _arduinogroupUuid As PwUuid

    Public Overrides Function Initialize(host As IPluginHost) As Boolean
        _host = host

        ' Get a reference to the 'Tools' menu item container
        Dim tsMenu As ToolStripItemCollection = _host.MainWindow.ToolsMenu.DropDownItems
        Dim dropMenus As ToolStripItemCollection = _host.MainWindow.EntryContextMenu.Items
        Dim groupMenus As ToolStripItemCollection = _host.MainWindow.GroupContextMenu.Items


        PwStick = New Stick


        m_tsSeparator = New ToolStripSeparator()
        tsMenu.Add(m_tsSeparator)

        _ReadFromStickMenu = New ToolStripMenuItem()
        _ReadFromStickMenu.Text = "ReadFromStick"

        _EraseStick = New ToolStripMenuItem()
        _EraseStick.Text = "Erase Stick"

        _ChangePinMenu = New ToolStripMenuItem()
        _ChangePinMenu.Text = "Stick: Change PIN"

        _sendToButtonMenu = New ToolStripMenuItem()
        _sendToButtonMenu.Text = "SendToHWPW-Stick"

        _StickSettings = New ToolStripMenuItem()
        _StickSettings.Text = "Stick Settings"

        _submenuitem1 = New ToolStripMenuItem
        _submenuitem1.Text = "Button1"
        _submenuitem2 = New ToolStripMenuItem
        _submenuitem2.Text = "Button2"
        _submenuitem3 = New ToolStripMenuItem
        _submenuitem3.Text = "Button3"
        _submenuitem4 = New ToolStripMenuItem
        _submenuitem4.Text = "Button4"
        _submenuitem5 = New ToolStripMenuItem
        _submenuitem5.Text = "Button5"
        _submenuitem6 = New ToolStripMenuItem
        _submenuitem6.Text = "Button6"
        _submenuitem7 = New ToolStripMenuItem
        _submenuitem7.Text = "Button7"
        _submenuitem8 = New ToolStripMenuItem
        _submenuitem8.Text = "Button8"
        _submenuitem9 = New ToolStripMenuItem
        _submenuitem9.Text = "Button9"
        _submenuitem10 = New ToolStripMenuItem
        _submenuitem10.Text = "Button10"

        _sendGroupToStick = New ToolStripMenuItem()
        _sendGroupToStick.Text = "Send all Items to Stick"

        _sendToButtonMenu.DropDownItems.Add(_submenuitem1)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem2)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem3)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem4)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem5)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem6)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem7)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem8)
        _sendToButtonMenu.DropDownItems.Add(_submenuitem9)
        '_sendToButtonMenu.DropDownItems.Add(_submenuitem10)
        dropMenus.Add(_sendToButtonMenu)


        allcontrols = New List(Of Oneuser)
        For i = 0 To 8
            allcontrols.Add(New Oneuser)
        Next


        AddHandler _ReadFromStickMenu.Click, AddressOf Me.ReadFromStick
        AddHandler _ChangePinMenu.Click, AddressOf Me.ChangePIN
        AddHandler _EraseStick.Click, AddressOf Me.EraseStick
        AddHandler _StickSettings.Click, AddressOf Me.StickSetttings
        AddHandler _submenuitem1.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem2.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem3.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem4.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem5.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem6.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem7.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem8.Click, AddressOf Me.WriteToStick
        AddHandler _submenuitem9.Click, AddressOf Me.WriteToStick
        AddHandler _sendGroupToStick.Click, AddressOf Me.SendGroupToStick

        tsMenu.Add(_EraseStick)
        tsMenu.Add(_ChangePinMenu)
        tsMenu.Add(_StickSettings)
        tsMenu.Add(_ReadFromStickMenu)
        groupMenus.Add(_sendGroupToStick)
        Return True

    End Function



    Public Overrides Sub Terminate()
        ' Remove all of our menu items
        Dim tsMenu As ToolStripItemCollection = _host.MainWindow.ToolsMenu.DropDownItems
        RemoveHandler _ReadFromStickMenu.Click, AddressOf Me.ReadFromStick
        RemoveHandler _ChangePinMenu.Click, AddressOf Me.ChangePIN
        tsMenu.Remove(_ReadFromStickMenu)
        tsMenu.Remove(m_tsSeparator)
        tsMenu.Remove(_ChangePinMenu)
        tsMenu.Remove(_EraseStick)
    End Sub


    Private Sub SendGroupToStick(sender As Object, e As EventArgs)

        Dim dButtontitles As New Dictionary(Of String, String)
        Dim SuccessCounter As Integer = 0
        For Each oneentry As KeePassLib.PwEntry In _host.MainWindow.GetSelectedGroup.Entries
            Try
                Dim btnTitle As String = oneentry.Strings.[Get](PwDefs.TitleField).ReadString()
                If btnTitle.Contains("Button ") Then
                    dButtontitles.Add(btnTitle, btnTitle)

                    Dim protectedString As ProtectedString = oneentry.Strings.GetSafe(PwDefs.PasswordField)
                    Dim bytespw As Byte() = protectedString.ReadUtf8()
                    Dim encoding As New UTF8Encoding()
                    Dim password As String = encoding.GetString(bytespw)
                    protectedString = oneentry.Strings.GetSafe(PwDefs.UserNameField)
                    Dim bytesusr As Byte() = protectedString.ReadUtf8()
                    Dim username As String = encoding.GetString(bytesusr)
                    Dim returnval As Stick.ReturnVals


                    Select Case btnTitle
                        Case "Button 1"
                            returnval = PwStick.ChangeEntry(New Byte() {&H41}, username, password)
                        Case "Button 2"
                            returnval = PwStick.ChangeEntry(New Byte() {&H42}, username, password)
                        Case "Button 3"
                            returnval = PwStick.ChangeEntry(New Byte() {&H43}, username, password)
                        Case "Button 4"
                            returnval = PwStick.ChangeEntry(New Byte() {&H44}, username, password)
                        Case "Button 5"
                            returnval = PwStick.ChangeEntry(New Byte() {&H45}, username, password)
                        Case "Button 6"
                            returnval = PwStick.ChangeEntry(New Byte() {&H46}, username, password)
                        Case "Button 7"
                            returnval = PwStick.ChangeEntry(New Byte() {&H47}, username, password)
                        Case "Button 8"
                            returnval = PwStick.ChangeEntry(New Byte() {&H48}, username, password)
                        Case "Button 9"
                            returnval = PwStick.ChangeEntry(New Byte() {&H49}, username, password)
                        Case Else
                            returnval = Stick.ReturnVals.Skipped

                    End Select

                    Select Case returnval
                        Case Stick.ReturnVals.Successfull
                            SuccessCounter += 1
                        Case Stick.ReturnVals.Failed
                            MessageBox.Show("Operation failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        Case Stick.ReturnVals.StickNotFound
                            MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        Case Stick.ReturnVals.StickLocked
                            MessageBox.Show("Stick is locked, enter PIN to change values !", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Exit Sub
                        Case Stick.ReturnVals.Skipped
                            SuccessCounter = SuccessCounter
                        Case Else
                            MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                    End Select


                End If

            Catch ex As ArgumentException
                MessageBox.Show("Double Title detected... skip entry...")
            Catch ex As Exception
                MessageBox.Show(ex.Message)
                Exit Sub
            End Try
        Next

        If SuccessCounter > 0 Then
            MessageBox.Show(SuccessCounter.ToString & " Accounts successfully written", "Change successfull", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("0 Accounts written to stick", "Nothing written", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If


    End Sub

    Private Sub WriteToStick(sender As Object, e As EventArgs)
        If Not _host.Database.IsOpen Then
            Return
        End If

        Dim pwEntry As PwEntry = _host.MainWindow.GetSelectedEntry(True)
        If pwEntry Is Nothing Then
            Return
        End If


        Dim title As String = pwEntry.Strings.[Get](PwDefs.TitleField).ReadString()
        Dim protectedString As ProtectedString = pwEntry.Strings.GetSafe(PwDefs.PasswordField)

        Dim bytespw As Byte() = protectedString.ReadUtf8()
        Dim encoding As New UTF8Encoding()
        Dim password As String = encoding.GetString(bytespw)
        protectedString = pwEntry.Strings.GetSafe(PwDefs.UserNameField)
        Dim bytesusr As Byte() = protectedString.ReadUtf8()
        Dim username As String = encoding.GetString(bytesusr)
        Dim returnval As Stick.ReturnVals

        Select Case True
            Case sender Is _submenuitem1
                returnval = PwStick.ChangeEntry(New Byte() {&H41}, username, password)
            Case sender Is _submenuitem2
                returnval = PwStick.ChangeEntry(New Byte() {&H42}, username, password)
            Case sender Is _submenuitem3
                returnval = PwStick.ChangeEntry(New Byte() {&H43}, username, password)
            Case sender Is _submenuitem4
                returnval = PwStick.ChangeEntry(New Byte() {&H44}, username, password)
            Case sender Is _submenuitem5
                returnval = PwStick.ChangeEntry(New Byte() {&H45}, username, password)
            Case sender Is _submenuitem6
                returnval = PwStick.ChangeEntry(New Byte() {&H46}, username, password)
            Case sender Is _submenuitem7
                returnval = PwStick.ChangeEntry(New Byte() {&H47}, username, password)
            Case sender Is _submenuitem8
                returnval = PwStick.ChangeEntry(New Byte() {&H48}, username, password)
            Case sender Is _submenuitem9
                returnval = PwStick.ChangeEntry(New Byte() {&H49}, username, password)
            Case sender Is _submenuitem10
                returnval = PwStick.ChangeEntry(New Byte() {&H50}, username, password)
            Case Else
                MessageBox.Show("Unknown Sender")
        End Select

        Select Case returnval
            Case Stick.ReturnVals.Successfull
                MessageBox.Show("Change was successfull", "Change successfull", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Case Stick.ReturnVals.Failed
                MessageBox.Show("Operation failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Case Stick.ReturnVals.StickNotFound
                MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Case Stick.ReturnVals.StickLocked
                MessageBox.Show("Stick is locked, enter PIN to change values !", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Case Else
                MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Select


    End Sub



    Private Sub ReadFromStick(sender As Object, e As EventArgs)
        If _host.Database.IsOpen Then

            Dim allreceived As String = String.Empty
            Dim returnval As Stick.ReturnVals = PwStick.readfromdevice(allreceived)

            Select Case returnval
                Case Stick.ReturnVals.Successfull
                    AddArduinoGroup()
                    Dim grp As PwGroup = _host.Database.RootGroup.FindGroup(ArduinoGroupUuid, True)

                    grp.DeleteAllObjects(_host.Database)
                    Dim allusers As String() = allreceived.Split("‡")
                    Try
                        For i = 0 To allcontrols.Count - 1
                            allcontrols(i).username = allusers(i).Split("†")(0)
                            allcontrols(i).password = allusers(i).Split("†")(1)
                            If allusers(i).Split("†")(2) = "1" Then
                                allcontrols(i).breturn = True
                            Else
                                allcontrols(i).breturn = False
                            End If

                            Dim pe As New PwEntry(True, True)

                            ' Set some of the string fields
                            pe.Strings.[Set](PwDefs.TitleField, New ProtectedString(False, "Button " + (i + 1).ToString()))
                            pe.Strings.[Set](PwDefs.UserNameField, New ProtectedString(True, allcontrols(i).username))
                            pe.Strings.[Set](PwDefs.PasswordField, New ProtectedString(True, allcontrols(i).password))

                            grp.AddEntry(pe, True)

                        Next

                    Catch ex As Exception
                        For i = 0 To allcontrols.Count - 1
                            allcontrols(i).username = "Error"
                            allcontrols(i).password = "Error"
                            allcontrols(i).breturn = False
                        Next

                    End Try

                    _host.MainWindow.UpdateUI(False, Nothing, False, grp, True, Nothing, True)


                Case Stick.ReturnVals.Failed
                    MessageBox.Show("Operation failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickNotFound
                    MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickLocked
                    MessageBox.Show("Stick is locked, enter PIN to read values !", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Case Else
                    MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select

        Else
            MessageBox.Show("Keine Datenbank geöffnet", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End If
    End Sub

    Private Sub StickSetttings(sender As Object, e As EventArgs)
        Dim setfrm As Settings
        setfrm = New Settings(PwStick)
        setfrm.ShowDialog()
    End Sub

    Private Sub ChangePIN(sender As Object, e As EventArgs)
        Select Case (PwStick.IsStickLocked)
            Case Stick.ReturnVals.Successfull
                Dim changeform As New ChangePin(PwStick)
                changeform.ShowDialog()
            Case Stick.ReturnVals.Failed
                MessageBox.Show("Operation failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Case Stick.ReturnVals.StickNotFound
                MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Case Stick.ReturnVals.StickLocked
                MessageBox.Show("Stick is locked, enter PIN to read values !", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Case Else
                MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Select

    End Sub

    Private Sub EraseStick(sender As Object, e As EventArgs)
        Dim result As Integer = MessageBox.Show("Achtung alle Daten auf dem Stick werden gelöscht." & vbNewLine & "Sind Sie sicher?" & vbNewLine & "Das löschen kann bis zu 10 Sekunden dauern", "Alle Daten löschen?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Select Case (PwStick.EraseStick)
                Case Stick.ReturnVals.Successfull
                    MessageBox.Show("Stick wurde formatiert - default PIN:0000 (4x Taste 10)", "Stick wurde formatiert", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Case Stick.ReturnVals.Failed
                    MessageBox.Show("Operation failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickNotFound
                    MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickLocked
                    MessageBox.Show("Stick is locked, Diese Meldung sollte nie Erscheinen!!! falls doch bitte Bug melden", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Case Else
                    MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
        End If

    End Sub




    Private Sub AddArduinoGroup()
        If Not _host.Database.IsOpen Then
            Return
        End If

        If IsArduinoGroupExist() Then
            Return
        End If

        Dim arduinogroup As New PwGroup(False, True, "Password Stick", PwIcon.Key)
        arduinogroup.Uuid = Me.ArduinoGroupUuid
        _host.Database.RootGroup.AddGroup(arduinogroup, True)

        '_host.Database.Save(null);
        '_host.Database.Modified = false;

        '_host.Database.RootGroup,
        _host.MainWindow.UpdateUI(False, Nothing, True, arduinogroup, True, Nothing,
            True)
    End Sub

    Private ReadOnly Property ArduinoGroupUuid() As PwUuid
        Get
            If _arduinogroupUuid Is Nothing Then
                _arduinogroupUuid = New PwUuid(New Guid("F85843F4-01BB-4428-A9F1-84F661F6B19C").ToByteArray())
            End If
            Return _arduinogroupUuid
        End Get
    End Property

    Private Function IsArduinoGroupExist() As Boolean
        Dim res As Boolean = False
        Debug.Assert(_host.Database.IsOpen)

        Dim grp As PwGroup = _host.Database.RootGroup.FindGroup(ArduinoGroupUuid, True)
        res = (grp IsNot Nothing)
        '
        '                 * Wenn das Ding in RecycleBin liegt kriegen wir eine Exception
        '                 * wenn wir eine weitere Gruppe erstellen -> Gleiche UUID
        '                 
        '                if ((null != grp.ParentGroup) &&
        '                    grp.ParentGroup.Uuid.Equals(_host.Database.RecycleBinUuid))
        '                    res = false;
        '                 

        If res Then
        End If

        Return res
    End Function




End Class

Class Oneuser
    Public username As String
    Public password As String
    Public breturn As Boolean
End Class

