Imports System.Windows.Forms

Public Class ChangePin
    Private PwStick As Stick

    Sub New(ByRef stickref As Stick)

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()
        PwStick = stickref
        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

    End Sub

    Private Sub tbPin_KeyPress(sender As Object, e As Windows.Forms.KeyPressEventArgs) Handles tbPin.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 57, 8, 32
                ' Zahlen, Backspace und Space zulassen
            Case Else
                ' alle anderen Eingaben unterdrücken
                e.Handled = True
        End Select
    End Sub

    Private Sub btChangePIN_Click(sender As Object, e As EventArgs) Handles btChangePIN.Click
        If tbPin.Text.Length = 4 Then
            Select Case PwStick.ChangePin(tbPin.Text)
                Case Stick.ReturnVals.Successfull
                    MessageBox.Show("Pin successfully changed to: " & tbPin.Text, "Change successfull", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Case Stick.ReturnVals.Failed
                    MessageBox.Show("Pin change failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickNotFound
                    MessageBox.Show("Stick not Found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Stick.ReturnVals.StickLocked
                    MessageBox.Show("Stick is locked, enter PIN to Unlock!", "Stick Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Case Else
                    MessageBox.Show("Unknown Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
        Else
            MessageBox.Show("The pin must contain 4 Digits")
        End If

        Me.Close()

    End Sub

    Private Sub btClose_Click(sender As Object, e As EventArgs) Handles btClose.Click
        Me.Close()
    End Sub

    Private Sub ChangePin_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class