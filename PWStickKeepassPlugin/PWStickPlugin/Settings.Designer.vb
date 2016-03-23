<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Settings
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
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.tbAutoLockSecs = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cbReturnKey = New System.Windows.Forms.CheckBox()
        Me.btSave = New System.Windows.Forms.Button()
        Me.btCancel = New System.Windows.Forms.Button()
        Me.cbLockWS = New System.Windows.Forms.CheckBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.cbLockWS)
        Me.GroupBox1.Controls.Add(Me.cbReturnKey)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.tbAutoLockSecs)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(266, 95)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Stick Settings"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 27)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(71, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Autolock min."
        '
        'tbAutoLockSecs
        '
        Me.tbAutoLockSecs.Location = New System.Drawing.Point(84, 24)
        Me.tbAutoLockSecs.MaxLength = 3
        Me.tbAutoLockSecs.Name = "tbAutoLockSecs"
        Me.tbAutoLockSecs.Size = New System.Drawing.Size(58, 20)
        Me.tbAutoLockSecs.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(148, 27)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(115, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "(0 = No lock .Max 250)"
        '
        'cbReturnKey
        '
        Me.cbReturnKey.AutoSize = True
        Me.cbReturnKey.Location = New System.Drawing.Point(9, 50)
        Me.cbReturnKey.Name = "cbReturnKey"
        Me.cbReturnKey.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.cbReturnKey.Size = New System.Drawing.Size(88, 17)
        Me.cbReturnKey.TabIndex = 3
        Me.cbReturnKey.Text = "Return Key   "
        Me.cbReturnKey.UseVisualStyleBackColor = True
        '
        'btSave
        '
        Me.btSave.Location = New System.Drawing.Point(161, 113)
        Me.btSave.Name = "btSave"
        Me.btSave.Size = New System.Drawing.Size(52, 23)
        Me.btSave.TabIndex = 1
        Me.btSave.Text = "save"
        Me.btSave.UseVisualStyleBackColor = True
        '
        'btCancel
        '
        Me.btCancel.Location = New System.Drawing.Point(226, 113)
        Me.btCancel.Name = "btCancel"
        Me.btCancel.Size = New System.Drawing.Size(52, 23)
        Me.btCancel.TabIndex = 2
        Me.btCancel.Text = "cancel"
        Me.btCancel.UseVisualStyleBackColor = True
        '
        'cbLockWS
        '
        Me.cbLockWS.AutoSize = True
        Me.cbLockWS.Location = New System.Drawing.Point(26, 72)
        Me.cbLockWS.Name = "cbLockWS"
        Me.cbLockWS.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.cbLockWS.Size = New System.Drawing.Size(71, 17)
        Me.cbLockWS.TabIndex = 4
        Me.cbLockWS.Text = "Lock WS"
        Me.cbLockWS.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(103, 73)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(133, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Sends WIN+L Key on lock"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(103, 51)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(98, 13)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Sends ENTER Key"
        '
        'Settings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(290, 144)
        Me.Controls.Add(Me.btCancel)
        Me.Controls.Add(Me.btSave)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "Settings"
        Me.Text = "Settings"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents tbAutoLockSecs As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cbReturnKey As System.Windows.Forms.CheckBox
    Friend WithEvents btSave As System.Windows.Forms.Button
    Friend WithEvents btCancel As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cbLockWS As System.Windows.Forms.CheckBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
End Class
