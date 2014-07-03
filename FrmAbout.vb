Public Class FrmAbout

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()

    End Sub

    Private Sub FrmAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximumSize = Me.Size
        Me.MinimumSize = Me.Size
        Me.Left = FrmMain.Left - ((Me.Width - FrmMain.Width) / 2)
        Me.Top = FrmMain.Top - ((Me.Height - FrmMain.Height) / 2)
        Label1.Text = "v." & Application.ProductVersion
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class