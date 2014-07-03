Public Class FrmSplash
    Dim CountDown As Integer = 2

    Private Sub FrmSplash_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' FrmMain.Show()

       
    End Sub
    Private Sub FrmSplash_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label3.Text = Application.ProductVersion
        Timer1.Enabled = True

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If CountDown > 0 Then
            CountDown -= 1
        Else
            CountDown = 2

            
            Me.Close()
            Timer1.Enabled = False
        End If
        '     Label2.Text = CountDown
    End Sub

    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles GroupBox1.Enter

    End Sub
End Class