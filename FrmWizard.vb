Public Class FrmWizard
    Dim WizardStep As Integer = 0
    Dim AbortIt As Boolean = True

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
 
        If WizardStep = 1 Then
            If TextBox1.Text <> TextBox2.Text Then
                MsgBox("The password and confirm fields are not matching")
                Exit Sub
            End If
            If Len(Trim(TextBox1.Text)) < 5 Then
                MsgBox("The password should be at least 6 characters long")
                Exit Sub
            End If
        End If

        If WizardStep = 2 Then
            If CheckBox1.Checked = False And CheckBox2.Checked = False Then
                MsgBox("You have to select one lock type!")
                Exit Sub
            End If
        End If

        If WizardStep = 3 Then
            If FileSuccessfullyCreated = False Then
                Label11.Text = "You failed to create an encrypted file in the previous step." & vbNewLine & _
                    "For safety reasons the wizard will not allow you to enable the program" & vbNewLine & _
                    "You are strongly adviced to review program settings and create an encrypted file " & vbNewLine & "by using Raptor interafce" & vbNewLine & _
                    "For more info read program documentation"
                CheckBox3.Checked = False
                CheckBox3.Enabled = False

            End If
        End If



        WizardStep += 1
        If WizardStep > 0 Then
            Button2.Visible = True
        End If
        StepByStep()


    End Sub

    Sub StepByStep()
        Select Case WizardStep
            Case 0
                GroupBox0.Visible = True
                GroupBox1.Visible = False
                GroupBox2.Visible = False
                GroupBox3.Visible = False
                GroupBox4.Visible = False
            Case 1
                GroupBox0.Visible = False
                GroupBox1.Visible = True
                GroupBox2.Visible = False
                GroupBox3.Visible = False
                GroupBox4.Visible = False
            Case 2
                GroupBox0.Visible = False
                GroupBox1.Visible = False
                GroupBox2.Visible = True
                GroupBox3.Visible = False
                GroupBox4.Visible = False
            Case 3
                GroupBox0.Visible = False
                GroupBox1.Visible = False
                GroupBox2.Visible = False
                GroupBox3.Visible = True
                GroupBox4.Visible = False
            Case 4
                GroupBox0.Visible = False
                GroupBox1.Visible = False
                GroupBox2.Visible = False
                GroupBox3.Visible = False
                GroupBox4.Visible = True
                Button3.Visible = True
                Button1.Visible = False
            Case Else

        End Select

    End Sub

    Private Sub FrmWizard_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        '    FirstTimeRun = False
        '    FrmMain.Form1_Load(Nothing, Nothing)
    End Sub



    Private Sub FrmWizard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximumSize = Me.Size
        Me.MinimumSize = Me.Size
        GroupBox0.Top = GroupBox1.Top
        GroupBox0.Left = GroupBox1.Left
        GroupBox2.Top = GroupBox1.Top
        GroupBox2.Left = GroupBox1.Left
        GroupBox3.Top = GroupBox1.Top
        GroupBox3.Left = GroupBox1.Left
        GroupBox4.Top = GroupBox1.Top
        GroupBox4.Left = GroupBox1.Left
        'GroupBox5.Top = GroupBox1.Top
        'GroupBox5.Left = GroupBox1.Left


        Label3.Text = "v.: " & Application.ProductVersion
    End Sub


    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveSettings()
        SaveSetting("Raptor", "Setting", "FirstTimeRun", False)
        Me.Close()
        FirstTimeRun = False

    End Sub

    Sub SaveSettings()
        SaveSetting("Raptor", "Setting", "SystemLock", CheckBox2.Checked)
        SaveSetting("Raptor", "Setting", "RaptorLock", CheckBox1.Checked)
        SaveSetting("Raptor", "Setting", "Enabled", CheckBox3.Checked)
        SaveSetting("Raptor", "Setting", "EncryptionKey", TextBox1.Text)
        FrmMain.TextBox3.Text = EncryptionKey
        FrmMain.Encrypt()
        FirstTimeRun = False
        FrmMain.Form1_Load(Nothing, Nothing)
    End Sub



    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        WizardStep -= 1
        If WizardStep < 4 Then
            Button3.Visible = False
            Button1.Visible = True
        End If

        StepByStep()

    End Sub

    'Private Sub TmrClose_Tick(sender As Object, e As EventArgs) Handles TmrClose.Tick
    '    AbortIt = False
    '    Me.Close()
    'End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            CheckBox1.Checked = False
        Else
            '   CheckBox1.Checked = False
        End If


    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            CheckBox2.Checked = False
        Else
            '   CheckBox2.Checked = False
        End If

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        FrmMain.LoadDrives()
        FrmMain.TextBox3.Text = TextBox1.Text
        FrmMain.WriteKey()
        If FileSuccessfullyCreated = True Then
        Else
            label10.visible = True
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        FirstTimeRun = False
        SaveSetting("Raptor", "Setting", "FirstTimeRun", FirstTimeRun)
        Me.Close()
    End Sub
End Class