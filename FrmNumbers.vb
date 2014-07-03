Public Class FrmNumbers
    Dim Countdown As Integer = 50
    Dim Opac As Integer = 100
    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        If AllowCancelDuringDelay = True Then
            Armed = False
            FrmMain.CheckBox4.Checked = False
            Me.Close()

        End If

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Timer1.Enabled = True

        Timer2.Enabled = True
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        If countdown > 0 Then
            countdown -= 1
        Else
            Countdown = 50
        End If

        Select Case countdown
            Case 50
                Label1.Text = "5"
                Timer2.Enabled = False
                Timer2.Enabled = True
                Opac = 100
            Case 40
                Label1.Text = "4"
                Timer2.Enabled = False
                Timer2.Enabled = True
                Opac = 100
            Case 30
                Label1.Text = "3"
                Timer2.Enabled = False
                Timer2.Enabled = True
                Opac = 100
            Case 20
                Label1.Text = "2"
                Timer2.Enabled = False
                Timer2.Enabled = True
                Opac = 100
            Case 10
                Label1.Text = "1"
                Timer2.Enabled = False
                Timer2.Enabled = True
                Opac = 100
            Case 0
                Label1.Text = "0"
                Timer2.Enabled = False
                Opac = 100
                LockIt()
                Me.Close()
        End Select

    End Sub

    Sub LockIt()
        FrmMain.LockSeq()

        Beep()
        Me.Close()

    End Sub



    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If Opac > 0 Then
            Opac -= 1
        Else
            Opac = 100
        End If

        Me.Opacity = Opac / 100
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Label1_Click(Nothing, Nothing)
    End Sub

    Private Sub Label2_Paint(sender As Object, e As PaintEventArgs) Handles Label2.Paint

    End Sub
End Class