Public Class FrmPassword
    Dim AgitateTimes As Integer = 10
    Dim RandomizeTime As Integer = 5
    Dim PrevTextboxLeft As Integer
    Dim PrevTextboxTop As Integer
    Dim ProhibitRelocate As Boolean = False
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click



        If TextBox1.Text = EncryptionKey Then


            If CommandIsExit = True Then
                FrmMain.Close()

            ElseIf CommandIsEnableDisable = True Then
                CommandIsEnableDisable = False
                If FrmMain.CheckBox1.Checked = True Then
                    FrmMain.CheckBox1.Checked = False
                Else
                    FrmMain.CheckBox1.Checked = True
                End If
            Else

                FrmMain.Show()
                FrmMain.Visible = True
                FrmMain.ShowInTaskbar = True
            End If

            Me.Close()

        Else
            PrevTextboxLeft = TextBox1.Left
            PrevTextboxTop = TextBox1.Top
            TmrAgitate.Enabled = True
            TextBox1.BackColor = Color.Salmon

        End If

    End Sub

    Private Sub TmrAgitate_Tick(sender As Object, e As EventArgs) Handles TmrAgitate.Tick


        If AgitateTimes > 0 Then
            AgitateTimes -= 1


            Dim intDiceRoll As Integer
            intDiceRoll = GetRandomNumber(2, 8)
            Dim intDiceRoll2 As Integer
            intDiceRoll2 = GetRandomNumber(1, 4)



            If IsEven(AgitateTimes) = True Then
                TextBox1.Left = PrevTextboxLeft - intDiceRoll
            Else
                TextBox1.Left = PrevTextboxLeft + intDiceRoll
            End If


            If IsEven(intDiceRoll2) = True Then
                TextBox1.Top = PrevTextboxTop - intDiceRoll2
            Else
                TextBox1.Top = PrevTextboxTop + intDiceRoll2
            End If

        Else
            AgitateTimes = 10
            TmrAgitate.Enabled = False
            ProhibitRelocate = False
            TextBox1.Left = PrevTextboxLeft
            TextBox1.Top = PrevTextboxTop
            TextBox1.Enabled = True
            TextBox1.BackColor = Color.White

        End If



    End Sub
    Dim objRandom As New System.Random(CType(System.DateTime.Now.Ticks Mod System.Int32.MaxValue, Integer))

    Public Function GetRandomNumber(Optional ByVal Low As Integer = 1, Optional ByVal High As Integer = 100) As Integer
        ' Returns a random number,
        ' between the optional Low and High parameters
        Return objRandom.Next(Low, High + 1)
    End Function
    Public Function IsEven(ByVal Number As Long) As Boolean
        IsEven = (Number Mod 2 = 0)
    End Function

   
    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = Chr(Keys.Enter) Then
            Button1_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub FrmPassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximumSize = Me.Size
        Me.MinimumSize = Me.Size
        TextBox1.Focus()
    End Sub
End Class