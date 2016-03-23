Imports System.Windows.Forms

Public Class Settings
    Private PwStick As Stick
    Sub New(ByRef stickref As Stick)

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()
        PwStick = stickref
        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

    End Sub

    Private Sub Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sSettings As String = String.Empty
        Select Case PwStick.GetSettings(sSettings)
            Case Stick.ReturnVals.Successfull
                'MessageBox.Show("Settings successfully changed", "Change successfull", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim aSettings As String() = sSettings.Split("‡")
                tbAutoLockSecs.Text = aSettings(1)
                If aSettings(0) = "1" Then cbReturnKey.Checked = True
                If aSettings(2) = "1" Then cbLockWS.Checked = True


            Case Stick.ReturnVals.Failed
                MessageBox.Show("Settings Read failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.Close()
            Case Stick.ReturnVals.StickNotFound
                MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.Close()
            Case Stick.ReturnVals.StickLocked
                MessageBox.Show("Stick is locked, enter PIN to Unlock!", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Me.Close()
            Case Else
                MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.Close()
        End Select

    End Sub

    Private Sub btSave_Click(sender As Object, e As EventArgs) Handles btSave.Click
        If tbAutoLockSecs.Text = "" Then tbAutoLockSecs.Text = "0"

        If Convert.ToInt16(tbAutoLockSecs.Text) < 251 Then
            Dim enter As Byte
            Dim lockws As Byte
            Dim locktime As Byte = Convert.ToByte(tbAutoLockSecs.Text)
            If cbReturnKey.Checked Then
                enter = 1
            Else
                enter = 0
            End If

            If cbLockWS.Checked Then
                lockws = 1
            Else
                lockws = 0
            End If

            Select Case PwStick.ChangeSettings(enter, locktime, lockws)
                Case Stick.ReturnVals.Successfull
                    MessageBox.Show("Settings successfully changed", "Change successfull", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Case Stick.ReturnVals.Failed
                    MessageBox.Show("Settings change failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickNotFound
                    MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickLocked
                    MessageBox.Show("Stick is locked, enter PIN to Unlock!", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Case Else
                    MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
            Me.Close()
        Else
            MessageBox.Show("Max. Locktime Value = 250!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

    End Sub

    Private Sub btCancel_Click(sender As Object, e As EventArgs) Handles btCancel.Click
        Me.Close()
    End Sub

    Private Sub tbAutoLockSecs_KeyPress(sender As Object, e As KeyPressEventArgs) Handles tbAutoLockSecs.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 57, 8, 32
                ' Zahlen, Backspace und Space zulassen
            Case Else
                ' alle anderen Eingaben unterdrücken
                e.Handled = True
        End Select
    End Sub
End Class