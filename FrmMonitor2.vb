

Public Class FrmMonitor2

    Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Declare Function SetWindowPos Lib "user32" (ByVal hwnd As Integer, ByVal hWndInsertAfter As Integer, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer) As Integer

    Public Const SWP_HIDEWINDOW = &H80
    Public Const SWP_SHOWWINDOW = &H40

    'Dim WithEvents K As New Keyboard
    'Structure contain information about low-level keyboard input event
    Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Long) As Integer

    Public Declare Function apiBlockInput Lib "user32" Alias "BlockInput" (ByVal fBlock As Integer) As Integer


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim time As DateTime = DateTime.Now
        '  Dim format As String = "MMM ddd d HH:mm yyyy"
        Dim format As String = "HH:mm:ss"
        LblClock.Text = time.ToString(format)
        '  Me.TopMost = True
        '  apiBlockInput(1)
        '  SendKeys.Send("{F5}")
    End Sub

    Private Sub FrmMonitor2_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Dim intReturn As Integer = FindWindow("Shell_traywnd", "")
        SetWindowPos(intReturn, 0, 0, 0, 0, 0, SWP_SHOWWINDOW)
        '   Call KillControlAltDelete(False)
        '   System.Threading.Thread.Sleep(3000)
        '   apiBlockInput(0)
        '   My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\", "DisableTaskMgr", "")
    End Sub



    Private Sub RandomColor()
        Dim rand As New Random
        Me.BackColor = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256))
    End Sub

    Private Sub FrmMonitor2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label4.Text = ""
        Dim myScreens() As Screen = Screen.AllScreens
        ' RandomColor()
        Me.BackColor = FrmMonitor1.BackColor
        'Dim intReturn As Integer = FindWindow("Shell_traywnd", "")
        'SetWindowPos(intReturn, 0, 0, 0, 0, 0, SWP_HIDEWINDOW)
        Dim screenWidth2 As Integer = myScreens(1).WorkingArea.Width
        Dim screenHeight2 As Integer = myScreens(1).WorkingArea.Height
        ' Dim newForm As New FrmMonitor1()
        Me.Top = myScreens(1).WorkingArea.Top
        Me.Left = myScreens(1).WorkingArea.Left
        Me.Height = screenHeight2
        Me.Width = screenWidth2
        Me.Show()
        '  My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\", "DisableTaskMgr", "1")
        Label2.Text = "Version: " & Application.ProductVersion
        Timer1.Enabled = True
        ' Call KillControlAltDelete(True)
        ' apiBlockInput(1)
        TmrPop.Enabled = True
        '  Timer1.Enabled = True
        Tmrpop_Tick(Nothing, Nothing)
        LblClock.Top = 20
        LblClock.Left = (Me.Width / 2) - (LblClock.Width / 2)
    End Sub

    Private Sub Tmrpop_Tick(sender As Object, e As EventArgs) Handles TmrPop.Tick
        ' Initialize the random-number generator.
        ' Randomize()
        ' Generate random value between 1 and 6. 
        ' apiBlockInput(0)
        Dim LimitVertical As Integer = Me.Height - Panel1.Height
        Dim LimitHorizontal As Integer = Me.Width - Panel1.Width
        Dim ValueVertical As Integer = CInt(Int((LimitVertical * Rnd()) + 1))
        Dim ValueHorizontal As Integer = CInt(Int((LimitHorizontal * Rnd()) + 1))
        Panel1.Top = ValueVertical
        Panel1.Left = ValueHorizontal
    End Sub


   




End Class
