<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ChangePin
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.tbPin = New System.Windows.Forms.TextBox()
        Me.btChangePIN = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btClose = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'tbPin
        '
        Me.tbPin.Location = New System.Drawing.Point(53, 12)
        Me.tbPin.MaxLength = 4
        Me.tbPin.Name = "tbPin"
        Me.tbPin.Size = New System.Drawing.Size(73, 20)
        Me.tbPin.TabIndex = 0
        '
        'btChangePIN
        '
        Me.btChangePIN.Location = New System.Drawing.Point(10, 39)
        Me.btChangePIN.Name = "btChangePIN"
        Me.btChangePIN.Size = New System.Drawing.Size(75, 23)
        Me.btChangePIN.TabIndex = 1
        Me.btChangePIN.Text = "ChangePIN"
        Me.btChangePIN.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 65)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(100, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Note: 0 = Button 10"
        '
        'btClose
        '
        Me.btClose.Location = New System.Drawing.Point(91, 39)
        Me.btClose.Name = "btClose"
        Me.btClose.Size = New System.Drawing.Size(75, 23)
        Me.btClose.TabIndex = 3
        Me.btClose.Text = "Close"
        Me.btClose.UseVisualStyleBackColor = True
        '
        'ChangePin
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(175, 84)
        Me.ControlBox = False
        Me.Controls.Add(Me.btClose)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btChangePIN)
        Me.Controls.Add(Me.tbPin)
        Me.Name = "ChangePin"
        Me.Text = "ChangePin"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tbPin As System.Windows.Forms.TextBox
    Friend WithEvents btChangePIN As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btClose As System.Windows.Forms.Button
End Class
