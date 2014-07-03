Public Class FrmNotification

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        TmrScrollDown.Enabled = True

    End Sub

    Dim myScreens() As Screen = Screen.AllScreens


    Private Sub FrmNotification_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Top = myScreens(0).Bounds.Top + myScreens(0).Bounds.Height '+ Me.Height
        Me.Left = myScreens(0).Bounds.Left + myScreens(0).Bounds.Width - Me.Width - 5

        TmrScrollUp.enabled = True
    End Sub

    Private Sub v_Tick(sender As Object, e As EventArgs) Handles TmrScrollUp.Tick
        Dim Taskbarsize = My.Computer.Screen.Bounds.Height - My.Computer.Screen.WorkingArea.Height


        '  If Me.Top > (myScreens(0).Bounds.Top + myScreens(0).Bounds.Height) - (Me.Height * 2) Then
        If Me.Top > (myScreens(0).Bounds.Top + myScreens(0).Bounds.Height) - Taskbarsize - Me.Height Then
            Me.Top -= 10
        Else
            TmrScrollUp.Enabled = False
            TmrHold.Enabled = True
        End If

    End Sub


    Private Sub TmrAutoClose_Tick(sender As Object, e As EventArgs) Handles TmrScrollDown.Tick
        If Me.Top < myScreens(0).Bounds.Top + myScreens(0).Bounds.Height Then
            Me.Top += 10
        Else
            TmrScrollDown.Enabled = False
            Me.Top = myScreens(0).Bounds.Top + myScreens(0).Bounds.Height
            Me.Close()
        End If
    End Sub

    Private Sub TmrHold_Tick(sender As Object, e As EventArgs) Handles TmrHold.Tick
        TmrHold.Enabled = False
        TmrScrollDown.Enabled = True

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        TmrHold.Enabled = False

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        TmrHold.Enabled = False
    End Sub
End Class